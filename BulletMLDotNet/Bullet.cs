using System;
using System.Collections.Generic;

namespace BulletML
{
    public class Bullet
    {
        protected Direction _direction;
        protected Speed _speed;
        protected List<Action> _actions = new List<Action>();

        internal Bullet() // for BulletRef construction
        {
            _direction = null;
            _speed = null;
        }

        internal Bullet(Direction dir, Speed speed, List<Action> actions)
        {
            _direction = dir;
            _speed = speed;
            _actions = actions;
        }

        public Direction Direction
        {
            get { return _direction; }
        }

        public Speed Speed
        {
            get { return _speed; }
        }

        public List<Action> Actions
        {
            get { return _actions; }
        }
    }

    public class LabeledBullet : Bullet
    {
        private string _label;

        public string Label
        {
            get { return _label; }
        }

        internal LabeledBullet(Direction dir, Speed speed, List<Action> actions, string label)
            : base(dir, speed, actions)
        {
            _label = label;
        }
    }

    public class BulletRef : Bullet
    {
        private string _refLabel;

        internal BulletRef(string refLabel)
        {
            _refLabel = refLabel;
        }

        internal void ResolveReference(LabeledBullet b)
        {
            if (_refLabel != b.Label)
            {
                throw new ArgumentException("Bad LabeledBullet reference.");
            }
            _direction = b.Direction;
            _speed = b.Speed;
            _actions = b.Actions;
        }

        public string RefLabel
        {
            get { return _refLabel; }
        }
    }
}
