using System.Linq;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame;

namespace SEScripts.Helpers
{
    #region SpaceEngineers

    public static class CargoHelper
    {
        public static Dictionary<string, ItemContent> GetItemsInInventories(IEnumerable<IMyInventory> inventories)
        {
            var result = new Dictionary<string, ItemContent>();
            foreach (var inventory in inventories)
            {
                foreach (var item in GetItemsInInventory(inventory))
                {
                    if (!result.ContainsKey(item.Key))
                    {
                        result.Add(item.Key, item);
                    }
                    else
                    {
                        result[item.Key].Quantity += item.Quantity;
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
                Inventory = inventory,
                ItemName = t.Type.SubtypeId,
                Quantity = t.Amount.ToIntSafe(),
                IsOre = t.Type.GetItemInfo().IsOre,
                IsIngot = t.Type.GetItemInfo().IsIngot,
            });
        }
    }

    public class ItemContent
    {
        public IMyInventory Inventory { get; set; }
        public string ItemName { get; set; }
        public bool IsOre { get; set; }
        public bool IsIngot { get; set; }
        public int Quantity { get; set; }
        public int Index { get; set; }
        public string Key { get { return ItemName + (IsOre ? "1" : "0") + (IsIngot ? "1" : "0"); } }
    }

    #endregion SpaceEngineers
}