using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System;
using VRage;
using VRage.Collections;
using VRage.Game.Components;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game;
using VRageMath;

namespace SEScripts.Scripts.AutomatedRefineries
{
    #region Usings

    using Helper = Helpers.Helper;
    using GridBlocksHelper = Helpers.GridBlocksHelper;
    using CargoHelper = Helpers.CargoHelper;
    using InventoryHelper = Helpers.InventoryHelper;
    using TerminalBlockHelper = Helpers.TerminalBlockHelper;

    #endregion Usings

    using GridCargo = Modules.GridCargo;
    using LcdOutputHelper = Helpers.LcdOutputHelper;
    

    public class Program : Skeleton
    {
        #region SpaceEngineers

        const string controlledRefineries = "Automated Refineries";
        const string emptyCargoContainer = "Empty";

        private static Helper helper;
        public Program()
        {
            Echo("Script ready to be launched..\n");
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
            helper = new Helper(GridTerminalSystem);
        }

        public void Main(string argument, UpdateType updateSource)
        {
            helper.UpdateTimer();

            Echo("Running Ucat's automated refineries" + helper.TimerChar);

            // Get all refineries in group
            var refineries = helper.Grid.GetGroupBlocks(controlledRefineries);
            Echo("Refineries: " + refineries.Count);

            // Empty refineries into one cargo container (that should be handled by some inventory manager script)
            var emptyCargo = helper.Grid.GetCargoContainers(emptyCargoContainer).FirstOrDefault();
            var inventoriesToEmpty = refineries.Select(t => t.GetInventory(1));
            inventoriesToEmpty.ToList().ForEach(t => t.TransferItemTo(emptyCargo.GetInventory(0), 0));

            // Get all inventories except refineries
            var inventories = helper.Grid.GetBlocks().Where(t => t.HasInventory && !(t is IMyRefinery))
                .SelectMany(t => InventoryHelper.GetInventories(t));
            //Echo("inventories: " + inventories.Count());

            var sourceInventory = inventories.FirstOrDefault(t => CargoHelper.GetOres(t).Any());
            // If there are ores to precess
            if (sourceInventory != null)
            {
                var inventoryItem = CargoHelper.GetOres(sourceInventory).First();
                // Fill refinery inventories
                refineries.ForEach(refinery =>
                {
                    refinery.GetInventory(0).TransferItemFrom(sourceInventory, inventoryItem);
                });
                //ores.First().Value.Inventory.TransferItemTo(refineries.First().GetInventory(0), ores.First().Value.Item);
            }

            //TODO: Split ores by available refineries

            // Turn off empty refineries and turn on inventories working
            refineries.ForEach(t => { if (t.GetInventory(0).ItemCount > 0) TerminalBlockHelper.TurnOn(t); else TerminalBlockHelper.TurnOff(t); });
            Echo("Refineries working: " + refineries.Select(t => t.IsWorking).Count());
        }

        #endregion
    }
}
