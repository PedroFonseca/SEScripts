using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRage;
using VRage.Library;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRageMath;
using VRage.Game.ModAPI.Ingame;

namespace SEScripts.Modules
{
    #region Usings

    using CargoHelper = Helpers.CargoHelper;
    using GridBlocksHelper = Helpers.GridBlocksHelper;
    using LcdOutputHelper = Helpers.LcdOutputHelper;
    using InventoryHelper = Helpers.InventoryHelper;

    #endregion Usings

    #region SpaceEngineers

    /// <summary>
    /// This module will autosort inventories and group items together with same type
    /// It will move items from one container into another to try and keep all items of same type together
    /// </summary>
    public class AutoSort
    {
        private IMyGridTerminalSystem GTS { get; set; }

        public AutoSort()
        {
        }

        private AutoSort(IMyGridTerminalSystem gts)
        {
            GTS = gts;
        }

        public static AutoSort Get(IMyGridTerminalSystem gts)
        {
            return new AutoSort(gts);
        }

        public void Sort(string groupName)
        {
            var groupBlocks = GridBlocksHelper.Get(GTS).GetGroupBlocks(groupName);
            var groupInventories = groupBlocks.SelectMany(t => InventoryHelper.GetInventories(t));
            var groupCargo = CargoHelper.GetItemsInInventories(groupInventories);

            var oresBlocks = GridBlocksHelper.Get(GTS).GetGroupBlocks(oresGroup);
            var ingotsBlocks = GridBlocksHelper.Get(GTS).GetGroupBlocks(ingotsGroup);
            var componentsBlocks = GridBlocksHelper.Get(GTS).GetGroupBlocks(componentsGroup);
        }
    }

    #endregion SpaceEngineers
}