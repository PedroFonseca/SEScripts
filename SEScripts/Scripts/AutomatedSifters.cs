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

namespace SEScripts.Scripts.AutomatedSifters
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

        const string controlledSifters = "Automated Sifters";
        const string emptyCargoContainer = "Empty";

        private static Helper H { get; set; }
        private List<IMyTerminalBlock> AllSifters { get; set; }
        private List<IMyInventory> ContainerInventories { get; set; }
        private IMyInventory EmptyCargoInventory { get; set; }
        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
            H = new Helper(GridTerminalSystem);
            Echo("Lauching script...\n If you add more sifters recompile script");

            // Get needed blocks in grid
            AllSifters = H.Grid.GetGroupBlocks(controlledSifters);
            EmptyCargoInventory = H.Grid.GetCargoContainers(emptyCargoContainer).First().GetInventory(0);
            ContainerInventories = H.Grid.GetBlocks().Where(t => t is IMyCargoContainer).Select(t => t.GetInventory(0)).ToList();

            // Empty sifters into one cargo container before starting script (that should be handled by some inventory manager script)
            AllSifters.Select(t => t.GetInventory(0)).Where(t => t.ItemCount > 0).ToList()
                .ForEach(t => CargoHelper.MoveAllCargo(t, EmptyCargoInventory));
        }

        public void Main(string argument, UpdateType updateSource)
        {
            H.UpdateTimer();

            Echo("Running Ucat's automated sifters" + H.TimerChar);

            if(H.TimerChar != "|")
            {
                // Don't run script on every execution
                return;
            }

            // Get gravel in inventories
            var gravel = ContainerInventories.SelectMany(t => CargoHelper.GetItemsInInventory(t)).Where(t => (t.IsIngot && t.ItemName == CargoHelper.STONE)).FirstOrDefault(); ;
            if (gravel == null)
            {
                Echo("All done, nothing to sift");
                return;
            }

            var divided = (MyFixedPoint)(gravel.Quantity.RawValue > 1000 ? ((decimal)gravel.Quantity.RawValue / AllSifters.Count / 1000000) : 1);

            if (divided.RawValue < 1000000)
            {
                Echo("\nNot worth moving " + gravel.Quantity.ToIntSafe() + " " + " gravel.\nWaiting for more...\n");
                return;
            }

            var sifterOne = AllSifters.First();
            var moveGravel = sifterOne.GetInventory(0).CurrentVolume.RawValue < sifterOne.GetInventory(0).MaxVolume.RawValue / 2;
            var emptySecondInventory = sifterOne.GetInventory(1).CurrentVolume.RawValue > sifterOne.GetInventory(1).MaxVolume.RawValue / 2;
            if (moveGravel)
            {
                Echo("Moving gravel into sifters: " + divided.ToIntSafe());
            }

            AllSifters.ForEach(t =>
            {
                // Move target quantity into refinery if not full
                if (moveGravel)
                {
                    t.GetInventory(0).TransferItemFrom(gravel.Inventory, gravel.Item, divided);
                }

                // Empty second inventory if half full
                if (emptySecondInventory) {
                    Echo("Emptying");
                    CargoHelper.MoveAllCargo(t.GetInventory(1), EmptyCargoInventory);
                }

                // Turn off sifter when it's empty, on otherwise
                if (t.GetInventory(0).ItemCount == 0)
                    TerminalBlockHelper.TurnOff(t);
                else
                    TerminalBlockHelper.TurnOn(t);
            });
        }

        #endregion
    }
}
