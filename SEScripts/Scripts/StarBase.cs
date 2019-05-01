//using Sandbox.ModAPI.Ingame;
//using Sandbox.ModAPI.Interfaces;
//using SpaceEngineers.Game.ModAPI;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using VRage.Game.ModAPI.Ingame;
//using VRageMath;

//namespace SEScripts.Scripts
//{
//    #region Usings

//    using GridCargo = Modules.GridCargo;
//    using LcdOutputHelper = Helpers.LcdOutputHelper;
//    using GridBlocksHelper = Helpers.GridBlocksHelper;
//    using InventoryHelper = Helpers.InventoryHelper;
//    using CargoHelper = Helpers.CargoHelper;

//    #endregion Usings

//    public class StarBase : Skeleton
//    {
//        #region SpaceEngineers

//        // Timmer is used to show something different every iteration
//        public static int timer = 0;

//        public void Main(string argument, UpdateType updateSource)
//        {
//            var debug = string.Empty;

//            // Move timmer
//            timer = timer == 3 ? 0 : timer + 1;

//            // Gets grid contents
//            var cargo = GridCargo.Get(GridTerminalSystem).ReadAllCargo();

//            // Show contents of all ores and ingots on grid
//            var lcds = GridBlocksHelper.Prefixed(GridTerminalSystem, "SB LCD Resources 1").GetLcdsPrefixed();
//            var resources = cargo.GetResources().OrderBy(t => t.OreQuantity + t.IngotQuantity).Select(t => string.Format("{0}: {1} ({2})", t.Name, t.IngotQuantity, t.OreQuantity));
//            LcdOutputHelper.ShowLinesWithProgress(lcds, resources, "== StarBase Ingots (Ores)==", timer);

//            //var a = cargo.Items.Values.Where(t => t.ItemName == "Platinum");
//            //var b = "    " + a.FirstOrDefault(t => t.IsOre)?.Quantity + " | " + a.FirstOrDefault(t => t.IsIngot)?.Quantity;
//            //LcdOutputHelper.ShowMessageOnLcd(GridBlocksHelper.Prefixed(GridTerminalSystem, "SB LCD Resources 2").GetLcdsPrefixed()[0], new LcdOutputHelper.LcdMessage(b, Color.Red));

//            //var message = string.Empty;
//            //var inventoryBlocks = GridBlocksHelper.Get(GridTerminalSystem).GetAllInventoryBlocks();
//            //message = " InventoryBlocks: " + inventoryBlocks.Count();
//            //var inventories = inventoryBlocks.SelectMany(t => InventoryHelper.GetInventories(t));
//            //message += "\n Inventories: " + inventories.Count();
//            //var items = inventories.SelectMany(t => CargoHelper.GetItemsInInventory(t));
//            //message += "\n Items: " + items.Count();
//            //var plat = items.Where(t => t.ItemName == "Platinum");
//            //message += "\n Plats " + plat.Count();
//            //message += "\n" + string.Join("\n ", plat.Select(t => (t.IsOre ? "Ore - " : "Ingot - ") + t.Quantity));
//            //var a = cargo.Items.Values.Where(t => t.ItemName == "Platinum");
//            //var b = "    " + a.FirstOrDefault(t => t.IsOre)?.Quantity + " | " + a.FirstOrDefault(t => t.IsIngot)?.Quantity;
//            //LcdOutputHelper.ShowMessageOnLcd(GridBlocksHelper.Prefixed(GridTerminalSystem, "SB LCD Resources 2").GetLcdsPrefixed()[0], new LcdOutputHelper.LcdMessage(timer+"", Color.Red));


//            //ShowContainerContents.Get(GridTerminalSystem)
//            //    .PrintContentsWithSubtype("SB LCD Ores 1", "SB - Ores", "=== StartBase Ore / Ingot ===", timer);

//            //// Show contents of ores container 2
//            //ShowContainerContents.Get(GridTerminalSystem)
//            //    .PrintContentsWithSubtype("SB LCD Ores 2", "SB Ores", "=== StartBase Ore / Ingot ===", timer);
//        }

//        #endregion SpaceEngineers
//    }
//}