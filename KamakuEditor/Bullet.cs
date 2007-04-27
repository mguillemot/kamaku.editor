using System;
using System.Drawing;
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
            set { _generator = value; }
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

        public void Update()
        {
            _position.X += Convert.ToSingle(Speed * Math.Cos(Direction));
            _position.Y += Convert.ToSingle(Speed * Math.Sin(Direction));
            if (Generator)
            {
                Generate();
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

        private void Generate()
        {
            float speed = MinSpeed + Rand.NextFloat() * (MaxSpeed - MinSpeed);
            float direction = MinAngle + Rand.NextFloat() * (MaxAngle - MinAngle);
            Engine.AddBullet(new Bullet(Position.X, Position.Y, speed, direction));
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
