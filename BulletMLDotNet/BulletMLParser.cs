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
        private List<ActionRef> _actionRefs = new List<ActionRef>();
        private List<BulletRef> _bulletRefs = new List<BulletRef>();
        private List<FireRef> _fireRefs = new List<FireRef>();

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
            foreach (ActionRef aref in _actionRefs)
            {
                aref.ResolveReference(_labeledActions[aref.RefLabel]);
            }
            _actionRefs.Clear();
            foreach (BulletRef bref in _bulletRefs)
            {
                bref.ResolveReference(_labeledBullets[bref.RefLabel]);
            }
            _bulletRefs.Clear();
            foreach (FireRef fref in _fireRefs)
            {
                fref.ResolveReference(_labeledFires[fref.RefLabel]);
            }
            _fireRefs.Clear();
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

        private ActionRef ParseActionRef(XPathNavigator actionRefNode)
        {
            string label = actionRefNode.GetAttribute("label", "");
            ActionRef aref = new ActionRef(label, new List<Expression>());
            XPathNodeIterator actionRefContent = actionRefNode.SelectChildren(XPathNodeType.Element);
            while (actionRefContent.MoveNext())
            {
                if (actionRefContent.Current.Name == "param")
                {
                    Expression e = ParseParam(actionRefContent.Current);
                    aref.Parameters.Add(e);
                }
            }
            _actionRefs.Add(aref);
            return aref;
        }

        private static Expression ParseParam(XPathNavigator paramNode)
        {
            return Expression.Parse(paramNode.Value);
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

        private FireRef ParseFireRef(XPathNavigator fireRefNode)
        {
            string label = fireRefNode.GetAttribute("label", "");
            FireRef fref = new FireRef(label, new List<Expression>());
            XPathNodeIterator fireRefContent = fireRefNode.SelectChildren(XPathNodeType.Element);
            while (fireRefContent.MoveNext())
            {
                if (fireRefContent.Current.Name == "param")
                {
                    Expression e = ParseParam(fireRefContent.Current);
                    fref.Parameters.Add(e);
                }
            }
            _fireRefs.Add(fref);
            return fref;
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
            Action action = null;
            if (actions.Count == 1)
            {
                action = actions[0];
            }
            else if (actions.Count > 1)
            {
                action = new Action();
                foreach (Action a in actions)
                {
                    action.AddActionContent(a);
                }
            }
            if (label == "")
            {
                return new Bullet(dir, speed, action);
            }
            else
            {
                return new LabeledBullet(dir, speed, action, label);
            }
        }

        private BulletRef ParseBulletRef(XPathNavigator bulletRefNode)
        {
            string label = bulletRefNode.GetAttribute("label", "");
            BulletRef bref = new BulletRef(label, new List<Expression>());
            XPathNodeIterator bulletRefContent = bulletRefNode.SelectChildren(XPathNodeType.Element);
            while (bulletRefContent.MoveNext())
            {
                if (bulletRefContent.Current.Name == "param")
                {
                    Expression e = ParseParam(bulletRefContent.Current);
                    bref.Parameters.Add(e);
                }
            }
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
