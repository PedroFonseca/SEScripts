using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;

namespace SEScripts.Helpers
{
    public class TerminalBlockHelper
    {
        public static IMyTerminalBlock GetBlockByName(List<IMyTerminalBlock> blocks, string blockName)
        {
            foreach (var block in blocks)
            {
                if (block.CustomName == blockName)
                {
                    return block;
                }
            }
            return null;
        }

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
}