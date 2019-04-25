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

        public Helper()
        {
        }

        private Helper(IMyGridTerminalSystem gts)
        {
            GTS = gts;
        }


        public List<IMyTerminalBlock> GetGroupBlocks(string groupName)
        {
            var aux = new List<IMyTerminalBlock>();
            GTS.GetBlockGroupWithName(groupName).GetBlocks(aux);
            return aux;
        }
    }
    #endregion SpaceEngineers
}