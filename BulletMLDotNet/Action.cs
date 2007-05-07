using System;
using System.Collections.Generic;

namespace BulletML
{
    public abstract class ActionContent
    {
        protected Action _parent = null;
        protected List<Expression> _parameters = null;

        public Action Parent
        {
            get { return _parent; }
        }

        public List<Expression> Parameters
        {
            get { return _parameters; }
        }


        internal ActionContent()
        {
        }

        internal void SetParent(Action parent)
        {
            _parent = parent;
        }
    }

    public class Action : ActionContent
    {
        protected List<ActionContent> _actions = new List<ActionContent>();

        internal Action()
        {
        }

        public List<ActionContent> Content
        {
            get { return _actions; }
        }

        internal void AddActionContent(ActionContent a)
        {
            _actions.Add(a);
            a.SetParent(this);
        }
    }

    public class LabeledAction : Action
    {
        private string _label;

        public string Label
        {
            get { return _label; }
        }

        internal LabeledAction(string label)
        {
            _label = label;
        }
    }

    public class ActionRef : Action
    {
        private string _refLabel;

        internal ActionRef(string refLabel, List<Expression> parameters)
        {
            _refLabel = refLabel;
            _parameters = parameters;
        }

        internal void ResolveReference(LabeledAction a)
        {
            if (_refLabel != a.Label)
            {
                throw new ArgumentException("Bad LabeledAction reference.");
            }
            _parent = a.Parent;
            _actions = a.Content;
        }

        public string RefLabel
        {
            get { return _refLabel; }
        }
    }

    public class Fire : ActionContent
    {
        protected Direction _direction;
        protected Speed _speed;
        protected Bullet _bullet;

        internal Fire() // for FireRef construction
        {
            _direction = null;
            _speed = null;
            _bullet = null;
        }

        internal Fire(Direction dir, Speed speed, Bullet bul)
        {
            _direction = dir;
            _speed = speed;
            _bullet = bul;
        }

        public Direction Direction
        {
            get { return _direction; }
        }

        public Speed Speed
        {
            get { return _speed; }
        }

        public Bullet Bullet
        {
            get { return _bullet; }
        }
    }

    public class LabeledFire : Fire
    {
        private string _label;

        internal LabeledFire(string label, Direction dir, Speed speed, Bullet bul) : base(dir, speed, bul)
        {
            _label = label;
        }

        public string Label
        {
            get { return _label; }
        }
    }

    public class FireRef : Fire
    {
        private string _refLabel;

        internal FireRef(string refLabel, List<Expression> parameters)
        {
            _refLabel = refLabel;
            _parameters = parameters;
        }

        internal void ResolveReference(LabeledFire f)
        {
            if (_refLabel != f.Label)
            {
                throw new ArgumentException("Bad LabeledFire reference.");
            }
            _parent = f.Parent;
            _direction = f.Direction;
            _speed = f.Speed;
            _bullet = f.Bullet;
        }

        public string RefLabel
        {
            get { return _refLabel; }
        }
    }

    public class ChangeDirection : ActionContent
    {
        private Direction _direction;
        private Expression _term;

        internal ChangeDirection(Direction dir, Expression term)
        {
            _direction = dir;
            _term = term;
        }

        public Direction Direction
        {
            get { return _direction; }
        }

        public Expression Term
        {
            get { return _term; }
        }
    }

    public class ChangeSpeed : ActionContent
    {
        private Speed _speed;
        private Expression _term;

        internal ChangeSpeed(Speed speed, Expression term)
        {
            _speed = speed;
            _term = term;
        }

        public Expression Term
        {
            get { return _term; }
        }

        public Speed Speed
        {
            get { return _speed; }
        }
    }

    public class Accel : ActionContent
    {
        private Speed _horizontal;
        private Speed _vertical;
        private Expression _term;

        internal Accel(Speed horizontal, Speed vertical, Expression term)
        {
            _horizontal = horizontal;
            _vertical = vertical;
            _term = term;
        }

        public Speed Horizontal
        {
            get { return _horizontal; }
        }

        public Speed Vertical
        {
            get { return _vertical; }
        }

        public Expression Term
        {
            get { return _term; }
        }
    }

    public class Vanish : ActionContent
    {
        internal Vanish()
        {
        }
    }

    public class Repeat : ActionContent
    {
        private Expression _times;
        private Action _action;

        internal Repeat(Expression times, Action action)
        {
            _times = times;
            _action = action;
        }

        public Expression Times
        {
            get { return _times; }
        }

        public Action Action
        {
            get { return _action; }
        }
    }

    public class Wait : ActionContent
    {
        private Expression _value;

        internal Wait(Expression value)
        {
            _value = value;
        }

        public Expression Value
        {
            get { return _value; }
        }
    }
}
