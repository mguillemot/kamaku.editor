using System.Collections.Generic;

namespace BulletML
{
    public abstract class ActionContent
    {
        private Action _parent = null;

        public Action Parent
        {
            get { return _parent; }
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
        private List<ActionContent> _actions = new List<ActionContent>();

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

    public class Fire : ActionContent
    {
        private Direction _direction;
        private Speed _speed;
        private Bullet _bullet;

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

    public class ChangeDirection : ActionContent
    {
        private Direction _direction;
        private int _term;

        internal ChangeDirection(Direction dir, int term)
        {
            _direction = dir;
            _term = term;
        }

        public Direction Direction
        {
            get { return _direction; }
        }

        public int Term
        {
            get { return _term; }
        }
    }

    public class ChangeSpeed : ActionContent
    {
        private Speed _speed;
        private int _term;

        internal ChangeSpeed(Speed speed, int term)
        {
            _speed = speed;
            _term = term;
        }

        public int Term
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
        private int _term;

        internal Accel(Speed horizontal, Speed vertical, int term)
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

        public int Term
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
        private int _times;
        private Action _action;

        internal Repeat(int times, Action action)
        {
            _times = times;
            _action = action;
        }

        public int Times
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
        private int _value;

        internal Wait(int value)
        {
            _value = value;
        }

        public int Value
        {
            get { return _value; }
            set { _value = value; }
        }
    }
}
