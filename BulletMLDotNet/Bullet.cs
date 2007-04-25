using System.Collections.Generic;

namespace BulletML
{
    public class Bullet
    {
        private Direction _direction;
        private Speed _speed;
        private List<Action> _actions = new List<Action>();

        public Direction Direction
        {
            get { return _direction; }
            set { _direction = value; }
        }

        public Speed Speed
        {
            get { return _speed; }
            set { _speed = value; }
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

        public LabeledBullet(string label)
        {
            _label = label;
        }
    }
}
