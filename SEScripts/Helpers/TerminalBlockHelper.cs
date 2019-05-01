using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;

namespace SEScripts.Helpers
{
    #region SpaceEngineers
    public class TerminalBlockHelper
    {
        public static void TurnOn(IMyTerminalBlock block)
        {
            var action = block.GetActionWithName("OnOff_On");
            action.Apply(block);
        }

        public static void TurnOff(IMyTerminalBlock block)
        {
            var action = block.GetActionWithName("OnOff_Off");
            action.Apply(block);
        }
    }
    #endregion SpaceEngineers
}