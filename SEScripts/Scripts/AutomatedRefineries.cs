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
        public List<string> oresPriority = new List<string>() { "Stone", "Iron", "Nickel", "Magnesium", "Silicon", "Cobalt", "Gold", "Silver", "Uranium", "Platinum" };

        private static Helper helper;
        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
            helper = new Helper(GridTerminalSystem);
            Echo("Emptying refineries and lauching script...\n");

            // Empty refineries into one cargo container before starting script (that should be handled by some inventory manager script)
            var refineries = helper.Grid.GetGroupBlocks(controlledRefineries);
            var emptyCargo = helper.Grid.GetCargoContainers(emptyCargoContainer).FirstOrDefault();
            var inventoriesToEmpty = refineries.Select(t => t.GetInventory(0));
            inventoriesToEmpty.ToList().ForEach(t => t.TransferItemTo(emptyCargo.GetInventory(0), 0));
        }

        public void Main(string argument, UpdateType updateSource)
        {
            helper.UpdateTimer();

            Echo("Running Ucat's automated refineries" + helper.TimerChar);

            // Get all refineries in group
            var refineries = helper.Grid.GetGroupBlocks(controlledRefineries);
            Echo("Refineries: " + refineries.Count);

            // Empty second inventory of refineries into one cargo container (that should be handled by some inventory manager script)
            var emptyCargo = helper.Grid.GetCargoContainers(emptyCargoContainer).FirstOrDefault();
            var inventoriesToEmpty = refineries.Select(t => t.GetInventory(1));
            inventoriesToEmpty.ToList().ForEach(t => t.TransferItemTo(emptyCargo.GetInventory(0), 0));

            // Get all inventories except refineries
            var inventories = helper.Grid.GetBlocks().Where(t => t.HasInventory && !(t is IMyRefinery))
                .SelectMany(t => InventoryHelper.GetInventories(t));
            Echo("inventories: " + inventories.Count());

            // Get inventories with ores
            var ores = inventories.SelectMany(t => CargoHelper.GetItemsInInventory(t)).Where(t => t.IsOre && t.ItemName != CargoHelper.ICE).ToList();
            Echo("Inventories with ores: " + ores.Count());

            if (ores.Count == 0)
            {
                Echo("No ores found!");
            }
            else
            {
                //var oreToRefine = oresPriority.FirstOrDefault(t => ores.Any(r => r.ItemName == t));
                //var oresToMove = ores.Where(t => t.ItemName == oreToRefine);
                //var quantityAvailable = oresToMove.Sum(t => t.Quantity);
                //var quantityPerRefinery = quantityAvailable / refineries.Count;
                //Echo("Splitting " + quantityAvailable + " " + oreToRefine + " between " + refineries + " refineries.");


                // Get the most priority ore available
                var oreNameToRefine = oresPriority.FirstOrDefault(t => ores.Any(r => r.ItemName == t));
                var oreToRefine = ores.FirstOrDefault(t => t.ItemName == oreNameToRefine);
                var containerWithOresToMove = oreToRefine?.Inventory?.Owner as IMyTerminalBlock;

                var quantityAvailable = containerWithOresToMove.GetInventory(0).GetItemAmount(oreToRefine.Item.Type);
                var quantityPerRefinery = quantityAvailable.ToIntSafe() / refineries.Count;
                Echo("Splitting " + quantityAvailable + " " + oreToRefine + " between " + refineries + " refineries (from " + containerWithOresToMove.CustomName + ")");

                // Split ores by available refineries
                refineries.ForEach(refinery =>
                {   
                    refinery.GetInventory(0).TransferItemFrom(containerWithOresToMove.GetInventory(0), oreToRefine.Item, quantityPerRefinery);
                });
            }
            //var inventoriesWithOres = inventories.Where(t => CargoHelper.GetOres(t).Any()).ToList();
            //Echo("inventories with ores: " + inventoriesWithOres.Count());

            //var sourceInventory = inventoriesWithOres.FirstOrDefault();
            //// If there are ores to precess
            //if (sourceInventory != null)
            //{
            //    Echo("Source inventory: " + ((IMyTerminalBlock)sourceInventory.Owner).CustomName);
            //    var inventoryItem = CargoHelper.GetOres(sourceInventory).First();
            //    // Fill refinery inventories
            //    refineries.ForEach(refinery =>
            //    {
            //        refinery.GetInventory(0).TransferItemFrom(sourceInventory, inventoryItem);
            //    });
            //    //ores.First().Value.Inventory.TransferItemTo(refineries.First().GetInventory(0), ores.First().Value.Item);
            //} else
            //{
            //    Echo("No ores found!");
            //}



            // Turn off empty refineries and turn on inventories working
            refineries.ForEach(t => { if (t.GetInventory(0).ItemCount > 0) TerminalBlockHelper.TurnOn(t); else TerminalBlockHelper.TurnOff(t); });
            Echo("Refineries working: " + refineries.Select(t => t.IsWorking).Count());
        }

        #endregion
    }
}
