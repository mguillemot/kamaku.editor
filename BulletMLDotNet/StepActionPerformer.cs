using System.Collections.Generic;

namespace BulletML
{
    public class StepActionPerformer
    {
        private Action _action;
        private int _firstInactive = 0;
        private Dictionary<StepActionPerformer, int> _activeRepeats = new Dictionary<StepActionPerformer, int>();

        public StepActionPerformer(Action a)
        {
            _action = a;
        }

        public List<ActionContent> PerformFrame()
        {
            List<ActionContent> actionsOfFrame = new List<ActionContent>();
            foreach (StepActionPerformer repeat in _activeRepeats.Keys)
            {
                actionsOfFrame.AddRange(repeat.PerformFrame());
                _activeRepeats[repeat]--;
                if (_activeRepeats[repeat] == 0)
                {
                    // Repeat is over
                    _activeRepeats.Remove(repeat);
                }
            }
            return actionsOfFrame;
        }

        private ActionContent PerformStep(out bool endOfFrame)
        {
            endOfFrame = false;
            if (_i >= _action.Content.Count)
            {
                _i = 0;
                endOfFrame = true;
                return null;
            }
            ActionContent subAction = _action.Content[_i];
            Repeat r = subAction as Repeat;
            if (r != null)
            {
                if (_subPerf == null)
                {
                    _subPerf = new StepActionPerformer(r.Action);
                }
                bool subEnded;
                ActionContent subActionContent = _subPerf.PerformStep(out subEnded);
                if (subActionContent == null)
                {
                    _repeat++;
                }
                if (_repeat == r.Times)
                {
                    _i++;
                    _repeat = 0;
                    _subPerf = null;
                }
                return subActionContent;
            }
            else
            {
                _i++;
                return subAction;
            }
        }
    }
}
