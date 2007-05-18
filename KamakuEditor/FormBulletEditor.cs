using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using BulletML;
using SdlDotNet.Core;
using SdlDotNet.Graphics;
using SdlDotNet.Particles;
using SdlDotNet.Particles.Emitters;

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
        private Bullet _topGenerator;

        public FormBulletEditor()
        {
            InitializeComponent();
        }

        private void FormBulletEditor_Load(object sender, EventArgs e)
        {
            _topGenerator = new Bullet(100, 100);
            //emitter.MinSpeed = 1;
            //emitter.MaxSpeed = 5;
            //emitter.MinAngle = 0;
            //emitter.MaxAngle = Convert.ToSingle(Math.PI);
            _topGenerator.Speed = 0;
            _topGenerator.DrawAsCircle(Color.Green, 3);
            Engine.Bullets.AddLast(_topGenerator);

            SdlDotNet.Core.Events.Fps = Settings.Fps;
            SdlDotNet.Core.Events.Tick += new EventHandler<TickEventArgs>(Events_Tick);
            Thread thread = new Thread(new ThreadStart(SdlDotNet.Core.Events.Run));
            thread.IsBackground = true;
            thread.Name = "SDL";
            thread.Priority = ThreadPriority.Normal;
            thread.Start();
        }

        private void Events_Tick(object sender, TickEventArgs e)
        {
            Rectangle ship;
            _frame.Fill(Color.Black);
            ship = new Rectangle(_mouseLoction.X - 4, _mouseLoction.Y - 5, 8, 9);
            _frame.Fill(ship, Color.Red);
            foreach (Bullet b in Engine.Bullets)
            {
                b.Update(_mouseLoction);
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
            if (_topGenerator.Generator)
            {
                buttonValidate.Text = "Start";
                _topGenerator.DesactivateGenerator();
            }
            else
            {
                buttonValidate.Text = "Stop";
                BulletMLParser parser = new BulletMLParser();
                parser.Parse(richTextBoxBulletML.Text);
                Console.WriteLine("BulletML parsed: {0} actions, {1} bullets, {2} fires.", parser.Actions.Count,
                                  parser.Bullets.Count, parser.Fires.Count);
                _topGenerator.ActivateGenerator(parser.Actions["top"]);
            }
        }
    }
}