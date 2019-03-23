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

        public void Sort(string sourceGroup, string oresGroup, string ingotsGroup, string componentsGroup)
        {
            var sourceBlocks = GridBlocksHelper.Get(GTS).GetGroupBlocks(sourceGroup);
            var sourceInventories = sourceBlocks.SelectMany(t => InventoryHelper.GetInventories(t));
            //var sourceCargo = CargoHelper.GetItemsInInventory

            var oresBlocks = GridBlocksHelper.Get(GTS).GetGroupBlocks(oresGroup);
            var ingotsBlocks = GridBlocksHelper.Get(GTS).GetGroupBlocks(ingotsGroup);
            var componentsBlocks = GridBlocksHelper.Get(GTS).GetGroupBlocks(componentsGroup);
        }
    }

    #endregion SpaceEngineers
}