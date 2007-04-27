using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.XPath;

namespace BulletML
{
    public class BulletMLParser
    {
        private Dictionary<string, LabeledAction> _labeledActions = new Dictionary<string, LabeledAction>();
        private Dictionary<string, LabeledBullet> _labeledBullets = new Dictionary<string, LabeledBullet>();
        private Dictionary<string, LabeledFire> _labeledFires = new Dictionary<string, LabeledFire>();

        private List<BulletRef> _bulletRefs = new List<BulletRef>();

        public Dictionary<string, LabeledAction> Actions
        {
            get { return _labeledActions; }
        }

        public Dictionary<string, LabeledBullet> Bullets
        {
            get { return _labeledBullets; }
        }

        public Dictionary<string, LabeledFire> Fires
        {
            get { return _labeledFires; }
        }

        public void Reset()
        {
            _labeledActions.Clear();
            _labeledBullets.Clear();
            _labeledFires.Clear();
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
            // Resolve references
            foreach (BulletRef bref in _bulletRefs)
            {
                bref.ResolveReference(_labeledBullets[bref.RefLabel]);
            }
            _bulletRefs.Clear();
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
                        ActionContent v = ParseVanish();
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
            Direction dir = null;
            Speed speed = null;
            List<Action> actions = new List<Action>();
            XPathNodeIterator bulletContent = bulletNode.SelectChildren(XPathNodeType.Element);
            while (bulletContent.MoveNext())
            {
                switch (bulletContent.Current.Name)
                {
                    case "direction":
                        dir = ParseDirection(bulletContent.Current);
                        break;
                    case "speed":
                       speed = ParseSpeed(bulletContent.Current);
                        break;
                    case "action":
                        actions.Add(ParseAction(bulletContent.Current));
                        break;
                    case "actionRef":
                        actions.Add(ParseActionRef(bulletContent.Current));
                        break;
                }
            }
            if (label == "")
            {
                return new Bullet(dir, speed, actions);
            }
            else
            {
                return new LabeledBullet(dir, speed, actions, label);
            }
        }

        private BulletRef ParseBulletRef(XPathNavigator bulletRefNode)
        {
            string label = bulletRefNode.GetAttribute("label", "");
            BulletRef bref = new BulletRef(label);
            _bulletRefs.Add(bref);
            return bref;
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
            Expression term = null;
            while (changeDirectionContent.MoveNext())
            {
                switch (changeDirectionContent.Current.Name)
                {
                    case "direction":
                        dir = ParseDirection(changeDirectionContent.Current);
                        break;
                    case "term":
                        term = ParseTerm(changeDirectionContent.Current);
                        break;
                }
            }
            return new ChangeDirection(dir, term);
        }

        private static ChangeSpeed ParseChangeSpeed(XPathNavigator changeSpeedNode)
        {
            XPathNodeIterator changeSpeedContent = changeSpeedNode.SelectChildren(XPathNodeType.Element);
            Speed speed = null;
            Expression term = null;
            while (changeSpeedContent.MoveNext())
            {
                switch (changeSpeedContent.Current.Name)
                {
                    case "speed":
                        speed = ParseSpeed(changeSpeedContent.Current);
                        break;
                    case "term":
                        term = ParseTerm(changeSpeedContent.Current);
                        break;
                }
            }
            return new ChangeSpeed(speed, term);
        }

        private static Expression ParseTerm(XPathNavigator termNode)
        {
            return Expression.Parse(termNode.Value);
        }

        private static Accel ParceAccel(XPathNavigator accelNode)
        {
            XPathNodeIterator accelContent = accelNode.SelectChildren(XPathNodeType.Element);
            Speed horizontal = null, vertical = null;
            Expression term = null;
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
                        term = ParseTerm(accelContent.Current);
                        break;
                }
            }
            return new Accel(horizontal, vertical, term);
        }

        private static Vanish ParseVanish()
        {
            return new Vanish();
        }

        private Repeat ParseRepeat(XPathNavigator repeatNode)
        {
            XPathNodeIterator repeatContent = repeatNode.SelectChildren(XPathNodeType.Element);
            Expression times = null;
            Action action = null;
            while (repeatContent.MoveNext())
            {
                switch (repeatContent.Current.Name)
                {
                    case "times":
                        times = ParseTerm(repeatContent.Current);
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
            return new Wait(ParseTerm(waitNode));
        }
    }
}
