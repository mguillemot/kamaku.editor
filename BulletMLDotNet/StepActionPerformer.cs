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

        public List<ActionContent> PerformFrame(float rank)
        {
            List<ActionContent> actionsOfFrame = new List<ActionContent>();
            bool endOfFrame = false;
            do
            {
                Step step = PerformStep(rank);
                switch (step.Type)
                {
                    case StepType.None:
                        break;
                    case StepType.ActionContent:
                        actionsOfFrame.Add(step.ActionContent);
                        break;
                    case StepType.Ended:
                        return actionsOfFrame;
                    case StepType.Wait:
                        endOfFrame = true;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            } while (!endOfFrame);
            return actionsOfFrame;
        }

        private Step PerformStep(float rank)
        {
            // Currently waiting
            if (_wait != 0)
            {
                _wait--;
                return new Step(StepType.Wait);
            }
            // Currently repeating an action
            if (_activeRepeat != null)
            {
                Step step = _activeRepeat.PerformStep(rank);
                if (step.Type == StepType.Ended)
                {
                    _repeat--;
                    if (_repeat == 0)
                    {
                        _activeRepeat = null;
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
                return new Step(StepType.Ended);
            }
            ActionContent subAction = _action.Content[_firstInactive];
            _firstInactive++;
            Repeat r = subAction as Repeat;
            Fire f = subAction as Fire;
            Wait w = subAction as Wait;
            Action a = subAction as Action;
            if (f != null)
            {
                return new Step(StepType.ActionContent, f);
            }
            else if (w != null)
            {
                _wait = Convert.ToInt32(w.Value.Evaluate(rank, w.Parameters)) - 1;
                return new Step(StepType.Wait);
            }
            else if (r != null)
            {
                _repeat = Convert.ToInt32(r.Times.Evaluate(rank, r.Parameters));
                _activeRepeat = new StepActionPerformer(r.Action, true);
                return new Step(StepType.None);
            }
            else if (a != null)
            {
                _repeat = 1;
                _activeRepeat = new StepActionPerformer(a);
                // TODO: bind paramters?
                return new Step(StepType.None);
            }
            return new Step(StepType.ActionContent, subAction);
        }
    }
}
