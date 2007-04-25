using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.XPath;

namespace BulletML
{
    public class BulletMLParser
    {
        private Dictionary<string, Action> _actions = new Dictionary<string, Action>();
        private Dictionary<string, Bullet> _bullets = new Dictionary<string, Bullet>();
        private Dictionary<string, Fire> _fires = new Dictionary<string, Fire>();

        public Dictionary<string, Action> Actions
        {
            get { return _actions; }
        }

        public Dictionary<string, Bullet> Bullets
        {
            get { return _bullets; }
        }

        public Dictionary<string, Fire> Fires
        {
            get { return _fires; }
        }

        public void Reset()
        {
            _actions.Clear();
            _bullets.Clear();
            _fires.Clear();
        }

        public void Parse(string bulletML)
        {
            XPathDocument xdoc = new XPathDocument(new StringReader(bulletML));
            XPathNavigator nav = xdoc.CreateNavigator();
            XmlNamespaceManager nsmanager = new XmlNamespaceManager(nav.NameTable);
            nsmanager.AddNamespace("bml", "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml");
            XPathNodeIterator iter = nav.Select("/bml:bulletml/bml:action", nsmanager);
            while (iter.MoveNext())
            {
                LabeledAction a = (LabeledAction)ParseAction(iter.Current);
                Actions[a.Label] = a;
            }
            iter = nav.Select("/bml:bulletml/bml:bullet", nsmanager);
            while (iter.MoveNext())
            {
                LabeledBullet b = (LabeledBullet)ParseBullet(iter.Current);
                Bullets[b.Label] = b;
            }
            iter = nav.Select("/bml:bulletml/bml:fire", nsmanager);
            while (iter.MoveNext())
            {
                LabeledFire f = (LabeledFire)ParseFire(iter.Current);
                Fires[f.Label] = f;
            }
        }

        private Action ParseAction(XPathNavigator actionNode)
        {
            string label = actionNode.GetAttribute("label", "");
            Action a;
            if (label != "")
            {
                a = new LabeledAction(label);
            }
            else
            {
                a = new Action();
            }
            XPathNodeIterator actionContent = actionNode.SelectChildren(XPathNodeType.Element);
            while (actionContent.MoveNext())
            {
                switch (actionContent.Current.Name)
                {
                    case "changeDirection":
                        ActionContent cd = ParseChangeDirection(actionContent.Current);
                        a.AddActionContent(cd);
                        break;
                    case "accel":
                        ActionContent acc = ParceAccel(actionContent.Current);
                        a.AddActionContent(acc);
                        break;
                    case "vanish":
                        ActionContent v = ParseVanish(actionContent.Current);
                        a.AddActionContent(v);
                        break;
                    case "changeSpeed":
                        ActionContent cs = ParseChangeSpeed(actionContent.Current);
                        a.AddActionContent(cs);
                        break;
                    case "repeat":
                        ActionContent r = ParseRepeat(actionContent.Current);
                        a.AddActionContent(r);
                        break;
                    case "wait":
                        ActionContent w = ParseWait(actionContent.Current);
                        a.AddActionContent(w);
                        break;
                    case "fire":
                        ActionContent fa = ParseFire(actionContent.Current);
                        a.AddActionContent(fa);
                        break;
                    case "fireRef":
                        ActionContent far = ParseFireRef(actionContent.Current);
                        a.AddActionContent(far);
                        break;
                    case "action":
                        ActionContent sa = ParseAction(actionContent.Current);
                        a.AddActionContent(sa);
                        break;
                    case "actionRef":
                        ActionContent sar = ParseActionRef(actionContent.Current);
                        a.AddActionContent(sar);
                        break;
                }
            }
            return a;
        }

        private Action ParseActionRef(XPathNavigator actionRefNode)
        {
            string label = actionRefNode.GetAttribute("label", "");
            return Actions[label];
        }

        private Fire ParseFire(XPathNavigator fireNode)
        {
            string label = fireNode.GetAttribute("label", "");
            Direction dir = null;
            Speed speed = null;
            Bullet bul = null;
            XPathNodeIterator fireContent = fireNode.SelectChildren(XPathNodeType.Element);
            while (fireContent.MoveNext())
            {
                switch (fireContent.Current.Name)
                {
                    case "direction":
                        dir = ParseDirection(fireContent.Current);
                        break;
                    case "speed":
                        speed = ParseSpeed(fireContent.Current);
                        break;
                    case "bullet":
                        bul = ParseBullet(fireContent.Current);
                        break;
                    case "bulletRef":
                        bul = ParseBulletRef(fireContent.Current);
                        break;
                }
            }
            if (label != "")
            {
                return new LabeledFire(label, dir, speed, bul);
            }
            else
            {
                return new Fire(dir, speed, bul);
            }
        }

        private Fire ParseFireRef(XPathNavigator fireRefNode)
        {
            string label = fireRefNode.GetAttribute("label", "");
            return Fires[label];
        }

        private Bullet ParseBullet(XPathNavigator bulletNode)
        {
            string label = bulletNode.GetAttribute("label", "");
            Bullet b;
            if (label == "")
            {
                b = new Bullet();
            }
            else
            {
                b = new LabeledBullet(label);
            }
            XPathNodeIterator bulletContent = bulletNode.SelectChildren(XPathNodeType.Element);
            while (bulletContent.MoveNext())
            {
                switch (bulletContent.Current.Name)
                {
                    case "direction":
                        b.Direction = ParseDirection(bulletContent.Current);
                        break;
                    case "speed":
                        b.Speed = ParseSpeed(bulletContent.Current);
                        break;
                    case "action":
                        b.Actions.Add(ParseAction(bulletContent.Current));
                        break;
                    case "actionRef":
                        b.Actions.Add(ParseActionRef(bulletContent.Current));
                        break;
                }
            }
            return b;
        }

        private Bullet ParseBulletRef(XPathNavigator bulletRefNode)
        {
            string label = bulletRefNode.GetAttribute("label", "");
            return Bullets[label];
        }

        private static Direction ParseDirection(XPathNavigator directionNode)
        {
            DirectionReference reference;
            string type = directionNode.GetAttribute("type", "");
            switch (type)
            {
                case "absolute":
                    reference = DirectionReference.Absolute;
                    break;
                case "relative":
                    reference = DirectionReference.Relative;
                    break;
                case "sequence":
                    reference = DirectionReference.Sequence;
                    break;
                default: // includes "aim"
                    reference = DirectionReference.Aim;
                    break;
            }
            Expression value = Expression.Parse(directionNode.Value);
            return new Direction(value, reference);
        }

        private static Speed ParseSpeed(XPathNavigator speedNode)
        {
            string type = speedNode.GetAttribute("type", "");
            SpeedReference reference;
            switch (type)
            {
                case "relative":
                    reference = SpeedReference.Relative;
                    break;
                case "sequence":
                    reference = SpeedReference.Sequence;
                    break;
                default: // includes "absolute"
                    reference = SpeedReference.Absolute;
                    break;
            }
            Expression value = Expression.Parse(speedNode.Value);
            return new Speed(value, reference);
        }

        private static ChangeDirection ParseChangeDirection(XPathNavigator changeDirectionNode)
        {
            XPathNodeIterator changeDirectionContent = changeDirectionNode.SelectChildren(XPathNodeType.Element);
            Direction dir = null;
            int term = 0;
            while (changeDirectionContent.MoveNext())
            {
                switch (changeDirectionContent.Current.Name)
                {
                    case "direction":
                        dir = ParseDirection(changeDirectionContent.Current);
                        break;
                    case "term":
                        term = ParseIntValue(changeDirectionContent.Current);
                        break;
                }
            }
            return new ChangeDirection(dir, term);
        }

        private static ChangeSpeed ParseChangeSpeed(XPathNavigator changeSpeedNode)
        {
            XPathNodeIterator changeSpeedContent = changeSpeedNode.SelectChildren(XPathNodeType.Element);
            Speed speed = null;
            int term = 0;
            while (changeSpeedContent.MoveNext())
            {
                switch (changeSpeedContent.Current.Name)
                {
                    case "speed":
                        speed = ParseSpeed(changeSpeedContent.Current);
                        break;
                    case "term":
                        term = ParseIntValue(changeSpeedContent.Current);
                        break;
                }
            }
            return new ChangeSpeed(speed, term);
        }

        private static int ParseIntValue(XPathNavigator intNode)
        {
            return int.Parse(intNode.Value);
        }

        private static Accel ParceAccel(XPathNavigator accelNode)
        {
            XPathNodeIterator accelContent = accelNode.SelectChildren(XPathNodeType.Element);
            Speed horizontal = null, vertical = null;
            int term = 0;
            while (accelContent.MoveNext())
            {
                switch (accelContent.Current.Name)
                {
                    case "horizontal":
                        horizontal = ParseSpeed(accelContent.Current);
                        break;
                    case "vertical":
                        vertical = ParseSpeed(accelContent.Current);
                        break;
                    case "term":
                        term = ParseIntValue(accelContent.Current);
                        break;
                }
            }
            return new Accel(horizontal, vertical, term);
        }

        private static Vanish ParseVanish(XPathNavigator vanishNode)
        {
            return new Vanish();
        }

        private Repeat ParseRepeat(XPathNavigator repeatNode)
        {
            XPathNodeIterator repeatContent = repeatNode.SelectChildren(XPathNodeType.Element);
            int times = 0;
            Action action = null;
            while (repeatContent.MoveNext())
            {
                switch (repeatContent.Current.Name)
                {
                    case "times":
                        times = ParseIntValue(repeatContent.Current);
                        break;
                    case "action":
                        action = ParseAction(repeatContent.Current);
                        break;
                    case "actionRef":
                        action = ParseActionRef(repeatContent.Current);
                        break;
                }
            }
            return new Repeat(times, action);
        }

        private static Wait ParseWait(XPathNavigator waitNode)
        {
            return new Wait(ParseIntValue(waitNode));
        }
    }
}
