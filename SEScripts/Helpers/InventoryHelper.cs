using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game.ModAPI.Ingame;

namespace SEScripts.Helpers
{
    #region SpaceEngineers

    public class InventoryHelper
    {
        public static IEnumerable<IMyInventory> GetInventories(IMyTerminalBlock block)
        {
            return Enumerable.Range(0, block.InventoryCount).Select(t => block.GetInventory(t));
        }
    }

    #endregion SpaceEngineers
}