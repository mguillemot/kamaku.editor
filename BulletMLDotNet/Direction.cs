namespace BulletML
{
    public enum DirectionReference { Aim, Absolute, Relative, Sequence }

    public class Direction
    {
        private Expression _value;
        private DirectionReference _reference;

        internal Direction(Expression value, DirectionReference reference)
        {
            _value = value;
            _reference = reference;
        }

        public Expression Value
        {
            get { return _value; }
        }

        public DirectionReference Reference
        {
            get { return _reference; }
        }
    }
}
