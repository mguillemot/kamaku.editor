using System;
using System.Collections.Generic;

namespace BulletML
{
    public class Bullet
    {
        protected Direction _direction;
        protected Speed _speed;
        protected Action _action;
        protected List<Expression> _parameters;

        internal Bullet() // for BulletRef construction
        {
            _direction = null;
            _speed = null;
            _action = null;
            _parameters = null;
        }

        internal Bullet(Direction dir, Speed speed, Action action)
        {
            _direction = dir;
            _speed = speed;
            _action = action;
            _parameters = null;
        }

        public Direction Direction
        {
            get { return _direction; }
        }

        public Speed Speed
        {
            get { return _speed; }
        }

        public Action Action
        {
            get { return _action; }
        }

        public List<Expression> Parameters
        {
            get { return _parameters; }
        }
    }

    public class LabeledBullet : Bullet
    {
        private string _label;

        public string Label
        {
            get { return _label; }
        }

        internal LabeledBullet(Direction dir, Speed speed, Action action, string label)
            : base(dir, speed, action)
        {
            _label = label;
        }
    }

    public class BulletRef : Bullet
    {
        private string _refLabel;

        internal BulletRef(string refLabel, List<Expression> parameters)
        {
            _refLabel = refLabel;
            _parameters = parameters;
        }

        internal void ResolveReference(LabeledBullet b)
        {
            if (_refLabel != b.Label)
            {
                throw new ArgumentException("Bad LabeledBullet reference.");
            }
            _direction = b.Direction;
            _speed = b.Speed;
            _action = b.Action;
        }

        public string RefLabel
        {
            get { return _refLabel; }
        }
    }
}
