using System;
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

        public FormBulletEditor()
        {
            InitializeComponent();
        }

        private void FormBulletEditor_Load(object sender, EventArgs e)
        {
            Bullet gen = new Bullet(50, 50);
            gen.MinSpeed = 1;
            gen.MaxSpeed = 5;
            gen.MinAngle = 0;
            gen.MaxAngle = Convert.ToSingle(Math.PI);
            gen.Speed = 0.8f;
            gen.Generator = true;
            gen.DrawAsCircle(Color.Green, 3);
            //Engine.Bullets.AddLast(gen);

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
            Rectangle ship = new Rectangle();
            _frame.Fill(Color.Black);
            if (_mouseOver)
            {
                ship = new Rectangle(_mouseLoction.X - 4, _mouseLoction.Y - 5, 8, 9);
                _frame.Fill(ship, Color.Red);
            }
            if (_performer != null)
            {
                ActionContent action = _performer.PerformFrame();
                PerformActionContent(action);
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
            _particles.Update();
            _particles.Render(_frame);
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
            BulletMLParser parser = new BulletMLParser();
            parser.Parse(richTextBoxBulletML.Text);
            Console.WriteLine("BulletML parsed: {0} actions, {1} bullets, {2} fires.", parser.Actions.Count, parser.Bullets.Count, parser.Fires.Count);
            _performer = new StepActionPerformer(parser.Actions["top"]);
            _bind = new ParameterBind();
        }

        private void PerformActionContent(ActionContent a)
        {
            Fire f = a as Fire;
            if (f != null)
            {
                float speed = f.Speed.Value.Evaluate(_bind);
                //switch (f.Direction.Reference)
                //{
                //    case DirectionReference.Aim:
                        float direction = f.Direction.Value.Evaluate(_bind);
                        Engine.AddBullet(new Bullet(10, 10, speed, direction));
                //        break;
                //}
                
                
                //f.Speed
                //f.Bullet
            }
        }
    }
}