using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEScripts.Modules
{
    using ItemContent = Helpers.ItemContent;

    #region Usings

    using CargoHelper = Helpers.CargoHelper;
    using GridBlocksHelper = Helpers.GridBlocksHelper;
    using InventoryHelper = Helpers.InventoryHelper;

    #endregion Usings


    #region SpaceEngineers
    public class GridCargo
    {
        private IMyGridTerminalSystem GTS { get; set; }

        public Dictionary<string, ItemContent> Items { get; set; }

        public GridCargo()
        {
        }

        private GridCargo(IMyGridTerminalSystem gts)
        {
            GTS = gts;
        }

        public static GridCargo Get(IMyGridTerminalSystem gts)
        {
            return new GridCargo(gts);
        }

        public GridCargo ReadAllCargo()
        {
            var inventoryBlocks = GridBlocksHelper.Get(GTS).GetAllInventoryBlocks();
            var inventories = inventoryBlocks.SelectMany(t => InventoryHelper.GetInventories(t));
            Items = CargoHelper.GetItemsInInventories(inventories);
            return this;
        }

        public IEnumerable<Resource> GetResources()
        {
            var result = Items.Values.Where(t => t.IsOre).Select(t => new Resource { Name = t.ItemName, OreQuantity = t.Quantity }).ToDictionary(t => t.Name);
            Items.Values.Where(t => t.IsIngot).ToList().ForEach(t =>
            {
                if (result.ContainsKey(t.ItemName))
                {
                    result[t.ItemName].IngotQuantity = t.Quantity;
                }
                else
                {
                    result.Add(t.ItemName, new Resource { Name = t.ItemName, IngotQuantity = t.Quantity });
                }
            });
            return result.Values;
        }
    }

    public class Resource
    {
        public string Name { get; set; }
        public int OreQuantity { get; set; }
        public int IngotQuantity { get; set; }
    }
    #endregion
}
