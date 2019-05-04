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

        private static Helper helper;
        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
            helper = new Helper(GridTerminalSystem);
            Echo("Lauching script...\n");
        }

        public void Main(string argument, UpdateType updateSource)
        {
            helper.UpdateTimer();

            Echo("Running Ucat's automated sifters" + helper.TimerChar);

            // Get all blocks in group
            var sifters = helper.Grid.GetGroupBlocks(controlledSifters);
            Echo("Sifters: " + sifters.Count);

            // Empty second inventory of sifters into one cargo container (that should be handled by some inventory manager script)
            var emptyCargo = helper.Grid.GetCargoContainers(emptyCargoContainer).FirstOrDefault();
            var inventoriesToEmpty = sifters.Select(t => t.GetInventory(1));
            inventoriesToEmpty.ToList().ForEach(t => t.TransferItemTo(emptyCargo.GetInventory(0), 0));

            // Get all inventories except the ones in the group
            var inventories = helper.Grid.GetBlocks().Where(t => t.HasInventory && !sifters.Contains(t))
                .SelectMany(t => InventoryHelper.GetInventories(t));
            Echo("inventories: " + inventories.Count());

            Echo("---");
            Echo(sifters.First().GetInventory(0).GetItemAt(0).Value.Type.SubtypeId);
            Echo(sifters.First().GetInventory(0).GetItemAt(0).Value.Type.TypeId);
            Echo(sifters.First().GetInventory(0).GetItemAt(0).Value.Type.GetItemInfo().IsIngot ? "1" : "0");
            Echo("---");

            // Select first inventory with gravel
            var inventory = inventories.FirstOrDefault(t => CargoHelper.HasIngot(t, CargoHelper.STONE));

            if (inventory == null)
            {
                Echo("No gravel found!");
            }
            else
            {
                Echo("Emptying container: " + ((IMyTerminalBlock)inventory.Owner).CustomName);
                var item = inventory.FindItem(MyItemType.MakeIngot(CargoHelper.STONE));
                // Move gravel into sifters
                if (item.HasValue)
                {
                    sifters.ForEach(sifter =>
                    {
                        sifter.GetInventory(0).TransferItemFrom(inventory, item.Value);
                    });
                }
            }

            // Turn off empty sifters and turn on sifters working
            sifters.ForEach(t => { if (t.GetInventory(0).ItemCount > 0) TerminalBlockHelper.TurnOn(t); else TerminalBlockHelper.TurnOff(t); });
            Echo("Sifters working: " + sifters.Select(t => t.IsWorking).Count());
        }

        #endregion
    }
}
