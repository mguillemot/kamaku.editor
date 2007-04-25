using System.Collections.Generic;

namespace BulletML
{
    public class ParameterBind
    {
        private Dictionary<int, float> _paramValues = new Dictionary<int, float>();
        private float _rank;

        public float Rank
        {
            get { return _rank; }
            set { _rank = value; }
        }

        public float this[int paramNumber]
        {
            get
            {
                return _paramValues[paramNumber];
            }
            set
            {
                _paramValues[paramNumber] = value;
            }
        }
    }
}
