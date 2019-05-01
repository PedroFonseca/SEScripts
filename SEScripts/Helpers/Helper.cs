using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEScripts.Helpers
{
    #region SpaceEngineers
    public class Helper
    {
        private IMyGridTerminalSystem GTS { get; set; }

        public GridBlocksHelper Grid { get; private set; }

        // Timmer is used to show something different every iteration
        private static int t = 0;
        private readonly string[] timerChar = new string[] { "\\", "|", "/", "-" };
        public string TimerChar { get { return timerChar[t]; } }

        public Helper(IMyGridTerminalSystem gts)
        {
            GTS = gts;
            Grid = GridBlocksHelper.Get(gts);
        }

        public void UpdateTimer()
        {
            t = t == 3 ? 0 : t + 1;
        }
    }
    #endregion SpaceEngineers
}