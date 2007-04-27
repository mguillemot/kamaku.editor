using System;
using System.Collections.Generic;
using System.Drawing;
using BulletML;
using SdlDotNet.Graphics;
using SdlDotNet.Graphics.Primitives;

namespace Kamaku
{
    public enum DrawingType { Point, Circle, Rectangle, Line }

    public class Bullet
    {
        private PointF _position;
        private float _speed = 0.0f;
        private float _direction = 0.0f;
        private bool _collidable = true;
        private Color _color = Color.Pink;
        private Size _size;
        private short _radius;
        private int _trailSize;
        private DrawingType _drawingType = DrawingType.Point;

        // for generators only
        private bool _generator = false;
        private float _minSpeed;
        private float _maxSpeed;
        private float _minAngle;
        private float _maxAngle;
        private StepActionPerformer _performer;
        private float _sequenceLastAngle;

        public float MinSpeed
        {
            get { return _minSpeed; }
            set { _minSpeed = value; }
        }

        public float MaxSpeed
        {
            get { return _maxSpeed; }
            set { _maxSpeed = value; }
        }

        public float MinAngle
        {
            get { return _minAngle; }
            set { _minAngle = value; }
        }

        public float MaxAngle
        {
            get { return _maxAngle; }
            set { _maxAngle = value; }
        }

        public Point Position
        {
            get
            {
                return Point.Ceiling(_position);
            }
            set
            {
                _position.X = value.X;
                _position.Y = value.Y;
            }
        }

        public bool Collidable
        {
            get { return _collidable; }
            set { _collidable = value; }
        }

        public float Speed
        {
            get { return _speed; }
            set { _speed = value; }
        }

        public float Direction
        {
            get { return _direction; }
            set { _direction = value; }
        }

        public Color Color
        {
            get { return _color; }
            set { _color = value; }
        }

        public bool Generator
        {
            get { return _generator; }
        }

        public void ActivateGenerator(Action a)
        {
            _generator = true;
            _sequenceLastAngle = 0f;
            _performer = new StepActionPerformer(a);
        }

        public void DesactivateGenerator()
        {
            _generator = false;
            _performer = null;
        }

        public Bullet()
        {
        }

        public Bullet(int x, int y)
        {
            _position.X = x;
            _position.Y = y;
        }

        public Bullet(int x, int y, float speed, float direction) : this(x, y)
        {
            Direction = direction;
            Speed = speed;
        }

        public void Update(ParameterBind bind, Point playerShip)
        {
            _position.X += Convert.ToSingle(Speed * Math.Cos(Direction));
            _position.Y += Convert.ToSingle(Speed * Math.Sin(Direction));
            if (Generator)
            {
                List<ActionContent> actions = _performer.PerformFrame(bind);
                foreach (ActionContent ac in actions)
                {
                    PerformActionContent(ac, bind, playerShip);
                }
            }
        }

        public void GraphicalRepresentation(out IPrimitive shape, out Color color)
        {
            Point tail = Position;
            const int length = 5;
            tail.X -= Convert.ToInt32(length * Math.Cos(Direction));
            tail.Y -= Convert.ToInt32(length * Math.Sin(Direction));
            shape = new Line(tail, Position);
            color = Color.Pink;
        }

        public bool OutOfScreen()
        {
            Rectangle screen = new Rectangle(0, 0, Settings.Width, Settings.Height);
            return !screen.Contains(Position);
        }

        public void DrawAsPoint(Color c)
        {
            _color = c;
            _drawingType = DrawingType.Point;
        }

        public void DrawAsRectangle(Color c, Size s)
        {
            _color = c;
            _size = s;
            _drawingType = DrawingType.Rectangle;
        }

        public void DrawAsCircle(Color c, short radius)
        {
            _color = c;
            _radius = radius;
            _drawingType = DrawingType.Circle;
        }

        public void DrawAsLine(Color c, int trailSize)
        {
            _color = c;
            _trailSize = trailSize;
            _drawingType = DrawingType.Line;
        }

        public void Draw(Surface s)
        {
            switch (_drawingType)
            {
                case DrawingType.Circle:
                    s.Draw(new Circle(Position, _radius), _color, false, false);
                    break;
                case DrawingType.Rectangle:
                    s.Fill(new Rectangle(Position, _size), _color);
                    break;
                case DrawingType.Line:
                    Point trail = Position;
                    trail.X -= Convert.ToInt32(_trailSize * Math.Cos(_direction));
                    trail.Y -= Convert.ToInt32(_trailSize * Math.Sin(_direction));
                    break;
                default:
                    s.Draw(Position, _color);
                    break;
            }
        }

        /*
        private void Generate()
        {
            
            float speed = MinSpeed + Rand.NextFloat() * (MaxSpeed - MinSpeed);
            float direction = MinAngle + Rand.NextFloat() * (MaxAngle - MinAngle);
            Engine.AddBullet(new Bullet(Position.X, Position.Y, speed, direction));
             
        }
         * */

        private void PerformActionContent(ActionContent a, ParameterBind bind, Point playerShip)
        {
            Fire f = a as Fire;
            Vanish v = a as Vanish;
            if (f != null)
            {
                float speed = 1f; // default speed
                if (f.Speed != null)
                {
                    speed = f.Speed.Value.Evaluate(bind) * 2;
                }
                else
                {
                    Console.WriteLine("default speed");
                }
                float angleBML = 0f; // default direction
                if (f.Direction != null)
                {
                    Console.WriteLine("direction=" + f.Direction.Value);
                    angleBML = f.Direction.Value.Evaluate(bind);
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
                        _sequenceLastAngle = angleBML + AngleToward(playerShip);
                        break;
                    case DirectionReference.Sequence:
                        _sequenceLastAngle += angleBML;
                        break;
                }
                Bullet b = new Bullet(Position.X, Position.Y, speed, _sequenceLastAngle);
                if (f.Bullet.Action != null)
                {
                    // bullet is an emitter
                    b.ActivateGenerator(f.Bullet.Action);
                }
                Engine.AddBullet(b);
            }
            else if (v != null)
            {
                Engine.RemoveBullet(this);
            }
        }

        public float AngleToward(Point to)
        {
            float dx = to.X - _position.X;
            float dy = to.Y - _position.Y;
            float angle;
            if (dx != 0)
            {
                angle = (float) Math.Atan(dy/dx);
            }
            else if (dy > 0)
            {
                angle = (float) (Math.PI/2);
            }
            else
            {
                angle = (float) (-Math.PI/2);
            }
            if (dx < 0)
            {
                angle += (float) Math.PI;
            }
            return angle;
        }
    }
}
