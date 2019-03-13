using System;
using System.Collections.Generic;
using System.Text;
//Add reference to steam\SteamApps\common\SpaceEngineers\bin64\VRage.Math.dll
//Add reference to steam\SteamApps\common\SpaceEngineers\bin64\Sandbox.Common.dll
//Only 5 game namespaces are allowed in Programmable blocks
//http://steamcommunity.com/sharedfiles/filedetails/?id=360966557
//using Sandbox.ModAPI;  // NOT AVAILABLE for Programmable blocks
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
//using Sandbox.Common.ObjectBuilders;
using VRage;
using VRage.Library;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRageMath;
using VRage.Game.ModAPI.Ingame;

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