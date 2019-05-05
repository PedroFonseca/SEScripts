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
        public List<string> oresPriority = new List<string>() { "Stone", "Scrap", "Iron", "Nickel", "Magnesium", "Silicon", "Cobalt", "Gold", "Silver", "Uranium", "Platinum" };

        private static Helper H { get; set; }
        private List<IMyTerminalBlock> AllRefineries { get; set; }
        private List<IMyInventory> ContainerInventories { get; set; }
        private IMyInventory EmptyCargoInventory { get; set; }
        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
            H = new Helper(GridTerminalSystem);
            Echo("Lauching script...\n If you add more refineries recompile script");

            // Get needed blocks in grid
            AllRefineries = H.Grid.GetGroupBlocks(controlledRefineries);
            EmptyCargoInventory = H.Grid.GetCargoContainers(emptyCargoContainer).First().GetInventory(0);
            ContainerInventories = H.Grid.GetBlocks().Where(t => t is IMyCargoContainer).Select(t => t.GetInventory(0)).ToList();

            // Empty refineries into one cargo container before starting script (that should be handled by some inventory manager script)
            AllRefineries.Select(t => t.GetInventory(0)).Where(t => t.ItemCount > 0).ToList()
                .ForEach(t => CargoHelper.MoveAllCargo(t, EmptyCargoInventory));
        }

        public void Main(string argument, UpdateType updateSource)
        {
            H.UpdateTimer();

            Echo("Running Ucat's automated refineries" + H.TimerChar);

            // Get ores in inventories
            var ores = ContainerInventories.SelectMany(t => CargoHelper.GetItemsInInventory(t)).Where(t => (t.IsOre && t.ItemName != CargoHelper.ICE) || (t.IsIngot && t.ItemName == CargoHelper.SCRAP)).ToList();
            //Echo("Ores Found:\n" + ores.Select(t => t.ItemName + ": " + t.Quantity.ToIntSafe() + "\n").Aggregate((acc, t) => acc + t));

            // Get most priority ore and quantity for each refinery
            //var itemName = oresPriority.FirstOrDefault(t => ores.Any(r => r.ItemName == t));

            // Get ore with most quantity first
            Echo("Sorting ores by most quantity.\n");
            var max = ores.Max(r => r.Quantity.RawValue);
            var itemName = ores.FirstOrDefault(t => t.Quantity.RawValue == max)?.ItemName;
            if (itemName == null)
            {
                Echo("\nAll done, nothing to refine");
                return;
            }

            var item = ores.FirstOrDefault(t => t.ItemName == itemName);
            var divided = (MyFixedPoint) (item.Quantity.RawValue > 1000 ? ((decimal)item.Quantity.RawValue / AllRefineries.Count / 1000000) : 1);
            

            if (divided.RawValue < 1000000)
            {
                Echo("\nNot worth moving " + item.Quantity.ToIntSafe() + " " + itemName + ".\nWaiting for more...");
                return;
            }

            var refineryOne = AllRefineries.First();
            var moveOres = refineryOne.GetInventory(0).CurrentVolume.RawValue < refineryOne.GetInventory(0).MaxVolume.RawValue / 2;
            var emptySecondInventory = refineryOne.GetInventory(1).CurrentVolume.RawValue > refineryOne.GetInventory(1).MaxVolume.RawValue / 2;

            if (moveOres)
            {
                Echo("Moving " + divided.ToIntSafe() + " " + itemName + " into each refinery.");
            }

            AllRefineries.ForEach(t =>
            {
                // Move target quantity into refinery if not full
                if (moveOres)
                {
                    t.GetInventory(0).TransferItemFrom(item.Inventory, item.Item, divided);
                }

                // Empty second inventory
                if (emptySecondInventory)
                {
                    CargoHelper.MoveAllCargo(t.GetInventory(1), EmptyCargoInventory);
                }

                // Turn off refinery when it's empty, on otherwise
                if (t.GetInventory(0).ItemCount == 0)
                    TerminalBlockHelper.TurnOff(t);
                else
                    TerminalBlockHelper.TurnOn(t);
            });
        }

        #endregion
    }
}
