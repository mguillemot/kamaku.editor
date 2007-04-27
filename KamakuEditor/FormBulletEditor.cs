using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using SdlDotNet.Core;
using SdlDotNet.Graphics;
using SdlDotNet.Particles;
using SdlDotNet.Particles.Emitters;
using BulletML;

namespace Kamaku
{
    public delegate void RefreshDelegate();

    public partial class FormBulletEditor : Form
    {
        private bool _mouseOver = false;
        private Point _mouseLoction;
        private Surface _frame = new Surface(Settings.Width, Settings.Height);
        private ParticleSystem _particles = new ParticleSystem();
        private int _hitCount = 0;
        private StepActionPerformer _performer = null;
        private ParameterBind _bind;
        private Bullet _emitter;
        private float _sequenceLastAngle = 0f;

        public FormBulletEditor()
        {
            InitializeComponent();
        }

        private void FormBulletEditor_Load(object sender, EventArgs e)
        {
            _emitter = new Bullet(100, 100);
            _emitter.MinSpeed = 1;
            _emitter.MaxSpeed = 5;
            _emitter.MinAngle = 0;
            _emitter.MaxAngle = Convert.ToSingle(Math.PI);
            _emitter.Speed = 0;
            _emitter.Generator = false;
            _emitter.DrawAsCircle(Color.Green, 3);
            Engine.Bullets.AddLast(_emitter);

            SdlDotNet.Core.Events.Fps = Settings.Fps;
            SdlDotNet.Core.Events.Tick += new EventHandler<TickEventArgs>(Events_Tick);
            Thread thread = new Thread(new ThreadStart(SdlDotNet.Core.Events.Run));
            thread.IsBackground = true;
            thread.Name = "SDL";
            thread.Priority = ThreadPriority.Normal;
            thread.Start();
        }

        void Events_Tick(object sender, TickEventArgs e)
        {
            Rectangle ship;
            _frame.Fill(Color.Black);
            ship = new Rectangle(_mouseLoction.X - 4, _mouseLoction.Y - 5, 8, 9);
            _frame.Fill(ship, Color.Red);
            if (_performer != null)
            {
                List<ActionContent> actions = _performer.PerformFrame(_bind);
                if (actions != null)
                {
                    //Console.WriteLine(actions.Count + " actions");
                    foreach (ActionContent ac in actions)
                    {
                        PerformActionContent(ac);
                    }
                }
                else
                {
                    _performer = null;
                    //buttonValidate.Text = "Start"; thrad pb
                    Console.WriteLine("BulletML ended");
                }
            }
            foreach (Bullet b in Engine.Bullets)
            {
                b.Update();
                b.Draw(_frame);
                if (_mouseOver && b.Collidable && ship.Contains(b.Position))
                {
                    ShipCollides();
                    Engine.RemoveBullet(b);
                    _hitCount++;
                }
                else if (b.OutOfScreen())
                {
                    Engine.RemoveBullet(b);
                }
            }
            lock (_particles)
            {
                _particles.Update();
                _particles.Render(_frame);
            }
            surface.Blit(_frame);
            Engine.EndOfFrame();
            if (InvokeRequired)
            {
                Invoke(new RefreshDelegate(RefreshValues));
            }
        }

        private void RefreshValues()
        {
            textBoxHitCount.Text = _hitCount.ToString();
        }

        public void ShipCollides()
        {
            lock (_particles)
            {
                ParticlePixelEmitter _particlesEmitter = new ParticlePixelEmitter(_particles);
                _particlesEmitter.ColorMin = Color.Black;
                _particlesEmitter.ColorMax = Color.Red;
                _particlesEmitter.Frequency = 10000;
                _particlesEmitter.Life = 5;
                _particlesEmitter.LifeMin = 1;
                _particlesEmitter.LifeMax = 50;
                _particlesEmitter.SpeedMin = 1;
                _particlesEmitter.SpeedMax = 10;
                _particlesEmitter.Height = 10;
                _particlesEmitter.Width = 10;
                _particlesEmitter.X = _mouseLoction.X;
                _particlesEmitter.Y = _mouseLoction.Y;
            }
        }

        private void surface_MouseMove(object sender, MouseEventArgs e)
        {
            _mouseOver = true;
            _mouseLoction = e.Location;
        }

        private void surface_MouseLeave(object sender, EventArgs e)
        {
            _mouseOver = false;
        }

        private void surface_MouseUp(object sender, MouseEventArgs e)
        {
            ShipCollides();
        }

        private void buttonValidate_Click(object sender, EventArgs e)
        {
            if (_performer != null)
            {
                buttonValidate.Text = "Start";
                _performer = null;
                _sequenceLastAngle = 0f;
            }
            else
            {
                buttonValidate.Text = "Stop";
                BulletMLParser parser = new BulletMLParser();
                parser.Parse(richTextBoxBulletML.Text);
                Console.WriteLine("BulletML parsed: {0} actions, {1} bullets, {2} fires.", parser.Actions.Count,
                                  parser.Bullets.Count, parser.Fires.Count);
                _performer = new StepActionPerformer(parser.Actions["top"]);
                _bind = new ParameterBind();
            }
        }

        private void PerformActionContent(ActionContent a)
        {
            Fire f = a as Fire;
            if (f != null)
            {
                float speed = 1f; // default speed
                if (f.Speed != null)
                {
                    speed = f.Speed.Value.Evaluate(_bind)*2;
                }
                else
                {
                    Console.WriteLine("default speed");
                }
                float angleBML = 0f; // default direction
                if (f.Direction != null)
                {
                    Console.WriteLine("direction=" + f.Direction.Value);
                    angleBML = f.Direction.Value.Evaluate(_bind);
                }
                else
                {
                    Console.WriteLine("default direction");
                }
                angleBML = (float)(angleBML / 180 * Math.PI);
                switch (f.Direction.Reference)
                {
                    case DirectionReference.Absolute:
                    case DirectionReference.Relative: // what does "Relative" mean in this situation ?
                        _sequenceLastAngle = angleBML;
                        break;
                    case DirectionReference.Aim:
                        _sequenceLastAngle = angleBML + _emitter.AngleToward(_mouseLoction);
                        break;
                    case DirectionReference.Sequence:
                        _sequenceLastAngle += angleBML;
                        break;
                }
                Engine.AddBullet(new Bullet(_emitter.Position.X, _emitter.Position.Y, speed, _sequenceLastAngle));
            }
        }
    }
}