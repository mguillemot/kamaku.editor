using System.Collections.Generic;

namespace Kamaku
{
    public class Engine
    {
        private static LinkedList<Bullet> _currentFrameBullets = new LinkedList<Bullet>();
        private static List<Bullet> _addedBullets = new List<Bullet>();
        private static List<Bullet> _removedBullets = new List<Bullet>();

        private Engine()
        {
        }

        public static LinkedList<Bullet> Bullets
        {
            get { return _currentFrameBullets; }
        }

        public static void AddBullet(Bullet b)
        {
            _addedBullets.Add(b);
        }

        public static void RemoveBullet(Bullet b)
        {
            _removedBullets.Add(b);
        }

        public static void EndOfFrame()
        {
            foreach (Bullet b in _removedBullets)
            {
                _currentFrameBullets.Remove(b);
            }
            foreach (Bullet b in _addedBullets)
            {
                _currentFrameBullets.AddLast(b);
            }
            _removedBullets.Clear();
            _addedBullets.Clear();
        }
    }
}
