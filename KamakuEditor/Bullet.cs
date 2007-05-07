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
        private float _rank = 0f;

        // changes in progress
        private float _changeSpeedStep;
        private int _changeSpeedTerm = 0;
        private float _changeDirectionStep;
        private int _changeDirectionTerm = 0;

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
            set
            {
                _direction = Convert.ToSingle(value % (2 * Math.PI));
            }
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

        public void Update(Point playerShip)
        {
            if (_changeSpeedTerm > 0)
            {
                _speed += _changeSpeedStep;
                _changeSpeedTerm--;
            }
            if (_changeDirectionTerm > 0)
            {
                Direction += _changeDirectionStep;
                _changeDirectionTerm--;
            }
            _position.X += Convert.ToSingle(Speed * Math.Cos(Direction));
            _position.Y += Convert.ToSingle(Speed * Math.Sin(Direction));
            if (Generator)
            {
                List<ActionContent> actions = _performer.PerformFrame(_rank);
                foreach (ActionContent ac in actions)
                {
                    PerformActionContent(ac, playerShip);
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
                    trail.X -= Convert.ToInt32(_trailSize * Math.Cos(Direction));
                    trail.Y -= Convert.ToInt32(_trailSize * Math.Sin(Direction));
                    break;
                default:
                    s.Draw(Position, _color);
                    break;
            }
        }

        private void PerformActionContent(ActionContent a, Point playerShip)
        {
            Fire f = a as Fire;
            Vanish v = a as Vanish;
            ChangeSpeed cs = a as ChangeSpeed;
            ChangeDirection cd = a as ChangeDirection;
            if (f != null)
            {
                float speed = 1f; // default speed
                Speed sp = (f.Speed != null) ? f.Speed : f.Bullet.Speed;
                if (sp != null)
                {
                    switch (sp.Reference)
                    {
                        case SpeedReference.Absolute:
                            speed = sp.Value.Evaluate(_rank, a.Parameters);
                            break;
                        case SpeedReference.Relative:
                            speed = _speed + sp.Value.Evaluate(_rank, a.Parameters);
                            break;
                        case SpeedReference.Sequence:
                            speed = Engine.LastFireSpeed + sp.Value.Evaluate(_rank, a.Parameters);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                Direction dir = (f.Direction != null) ? f.Direction : f.Bullet.Direction;
                if (dir != null)
                {
                    float angleBML = dir.Value.Evaluate(_rank, a.Parameters);
                    angleBML = (float)(angleBML / 180 * Math.PI);
                    switch (dir.Reference)
                    {
                        case DirectionReference.Absolute:
                            _sequenceLastAngle = angleBML;
                            break;
                        case DirectionReference.Relative:
                            _sequenceLastAngle = Direction + angleBML;
                            break;
                        case DirectionReference.Aim:
                            _sequenceLastAngle = angleBML + AngleToward(playerShip);
                            break;
                        case DirectionReference.Sequence:
                            _sequenceLastAngle += angleBML;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                else
                {
                    _sequenceLastAngle = AngleToward(playerShip);
                }
                Engine.LastFireSpeed = speed;
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
            else if (cs != null)
            {
                _changeSpeedTerm = Convert.ToInt32(cs.Term.Evaluate(_rank, a.Parameters));
                float speedBML = cs.Speed.Value.Evaluate(_rank, a.Parameters);
                switch (cs.Speed.Reference)
                {
                    case SpeedReference.Absolute:
                        _changeSpeedStep = (speedBML - _speed) / _changeSpeedTerm;
                        break;
                    case SpeedReference.Relative:
                        _changeSpeedStep = speedBML / _changeSpeedTerm;
                        break;
                    case SpeedReference.Sequence:
                        // TODO
                        throw new NotImplementedException();
                        break;
                }
            }
            else if (cd != null)
            {
                _changeDirectionTerm = Convert.ToInt32(cd.Term.Evaluate(_rank, a.Parameters));
                float directionBML = cd.Direction.Value.Evaluate(_rank, a.Parameters);
                directionBML = Convert.ToSingle(directionBML / 180 * Math.PI);
                switch (cd.Direction.Reference)
                {
                    case DirectionReference.Aim:
                        _changeDirectionStep = (AngleToward(playerShip) + directionBML - Direction) / _changeDirectionTerm;
                        break;
                    case DirectionReference.Absolute:
                        _changeDirectionStep = (directionBML - Direction) / _changeDirectionTerm;
                        break;
                    case DirectionReference.Relative:
                        _changeDirectionStep = directionBML / _changeDirectionTerm;
                        break;
                    case DirectionReference.Sequence:
                        // TODO
                        throw new NotImplementedException();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
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
