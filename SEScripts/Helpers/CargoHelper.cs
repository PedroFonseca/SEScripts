using System.Linq;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame;

namespace SEScripts.Helpers
{
    #region SpaceEngineers

    public static class CargoHelper
    {
        public static Dictionary<string, ItemContent> GetItemsInInventories(List<IMyTerminalBlock> inventoryBlocks, int inventoryIndex = 0)
        {
            if (inventoryBlocks.Count == 0)
                return new Dictionary<string, ItemContent>();

            var result = new Dictionary<string, ItemContent>();
            foreach (var inventory in inventoryBlocks)
            {
                foreach (var item in GetItemsInInventory(inventory.GetInventory(inventoryIndex)).ToDictionary(t => t.ItemName, t => t))
                {
                    if (!result.ContainsKey(item.Key))
                    {
                        result.Add(item.Key, item.Value as ItemContent);
                    }
                    else
                    {
                        result[item.Key].Quantity += item.Value.Quantity;
                    }
                }
            }
            return result;
        }

        public static IEnumerable<ItemContent> GetItemsInInventory(IMyInventory inventory)
        {
            List<MyInventoryItem> items = new List<MyInventoryItem>();
            inventory.GetItems(items);

            return items.Select((t, i) => new ItemContent
            {
                Index = i,
                ItemName = t.Type.SubtypeId,
                Quantity = (int)t.Amount.RawValue / 1000000,
                IsOre = t.Type.GetItemInfo().IsOre,
                IsIngot = t.Type.GetItemInfo().IsIngot,
            });
        }

        public class ItemContent
        {
            public string ItemName { get; set; }
            public bool IsOre { get; set; }
            public bool IsIngot { get; set; }
            public int Quantity { get; set; }
            public int Index { get; set; }
        }
    }

    #endregion SpaceEngineers
}