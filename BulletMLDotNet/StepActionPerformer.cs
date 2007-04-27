using System;
using System.Collections.Generic;

namespace BulletML
{
    public class StepActionPerformer
    {
        private enum StepType { None, ActionContent, Ended, Wait }

        private struct Step
        {
            private StepType _type;
            private ActionContent _actionContent;

            public Step(StepType type, ActionContent ac)
            {
                _type = type;
                _actionContent = ac;
            }

            public Step(StepType type)
            {
                if (type == StepType.ActionContent)
                {
                    throw new ArgumentException("ActionContent steps must have a bound ActionContent");
                }
                _type = type;
                _actionContent = null;
            }

            public StepType Type
            {
                get { return _type; }
            }

            public ActionContent ActionContent
            {
                get { return _actionContent; }
            }
        }

        private Action _action;
        private int _wait;
        private int _repeat;
        private int _firstInactive = 0;
        private StepActionPerformer _activeRepeat;
        private bool _loop = false;

        public StepActionPerformer(Action a)
        {
            _action = a;
        }

        private StepActionPerformer(Action a, bool loop) : this(a)
        {
            _loop = loop;
        }

        public List<ActionContent> PerformFrame(ParameterBind bind)
        {
            List<ActionContent> actionsOfFrame = new List<ActionContent>();
            bool endOfFrame = false;
            do
            {
                Step step = PerformStep(bind);
                switch (step.Type)
                {
                    case StepType.None:
                        break;
                    case StepType.ActionContent:
                        actionsOfFrame.Add(step.ActionContent);
                        break;
                    case StepType.Ended:
                        return actionsOfFrame; //null?
                    case StepType.Wait:
                        endOfFrame = true;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            } while (!endOfFrame);
            return actionsOfFrame;
        }

        private Step PerformStep(ParameterBind bind)
        {
            // Currently waiting
            if (_wait != 0)
            {
                _wait--;
                //Console.WriteLine("Wait (reamining " + _wait + ")");
                return new Step(StepType.Wait);
            }
            // Currently repeating an action
            if (_activeRepeat != null)
            {
                Step step = _activeRepeat.PerformStep(bind);
                if (step.Type == StepType.Ended)
                {
                    _repeat--;
                    if (_repeat == 0)
                    {
                        _activeRepeat = null;
                        //Console.WriteLine("Repeat ended.");
                    }
                    else
                    {
                        //Console.WriteLine("Reapeating " + _repeat + " times more");
                    }
                    return new Step(StepType.None);
                }
                else
                {
                    return step;
                }
            }
            // Process the next sub-action
            if (_firstInactive >= _action.Content.Count)
            {
                if (_loop)
                {
                    _firstInactive = 0;
                }
                //Console.WriteLine("End of subaction list");
                return new Step(StepType.Ended);
            }
            ActionContent subAction = _action.Content[_firstInactive];
            _firstInactive++;
            Repeat r = subAction as Repeat;
            Fire f = subAction as Fire;
            Wait w = subAction as Wait;
            Vanish v = subAction as Vanish;
            if (r != null)
            {
                _repeat = Convert.ToInt32(r.Times.Evaluate(bind));
                _activeRepeat = new StepActionPerformer(r.Action, true);
                //Console.WriteLine("Activate repeat (" + _repeat + " times)");
                return new Step(StepType.None);
            }
            else if (f != null)
            {
                //Console.WriteLine("Fire!");
                /*
                if (f.Bullet.Actions.Count > 0)
                {
                    // firing an emitter
                    foreach (Action a in f.Bullet.Actions)
                    {
                        _activeSubActions.Add(new StepActionPerformer(a, false));
                    }
                }
                 */
                return new Step(StepType.ActionContent, f);
            }
            else if (w != null)
            {
                //Console.WriteLine("Set wait " + (w.Value - 1));
                _wait = Convert.ToInt32(w.Value.Evaluate(bind)) - 1;
                return new Step(StepType.Wait);
            }
            else if (v != null)
            {
                return new Step(StepType.ActionContent, v);
            }
            //Console.WriteLine("unkown subaction");
            return new Step(StepType.None);
        }
    }
}
