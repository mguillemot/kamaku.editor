namespace BulletML
{
    public enum SpeedReference { Absolute, Relative, Sequence }

    public class Speed
    {
        private Expression _value;
        private SpeedReference _reference;

        internal Speed(Expression value, SpeedReference reference)
        {
            _value = value;
            _reference = reference;
        }

        public Expression Value
        {
            get { return _value; }
        }

        public SpeedReference Reference
        {
            get { return _reference; }
        }
    }
}
