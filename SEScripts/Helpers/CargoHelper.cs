using System;
using System.Collections.Generic;
using System.Text;
//Add reference to steam\SteamApps\common\SpaceEngineers\bin64\VRage.Math.dll
//Add reference to steam\SteamApps\common\SpaceEngineers\bin64\Sandbox.Common.dll
//Only 5 game namespaces are allowed in Programmable blocks
//http://steamcommunity.com/sharedfiles/filedetails/?id=360966557
//using Sandbox.ModAPI;  // NOT AVAILABLE for Programmable blocks
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
//using Sandbox.Common.ObjectBuilders;
using VRage;
using VRage.Library;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRageMath;
using VRage.Game.ModAPI.Ingame;

public static class CargoHelper
{
    public static Dictionary<string, ItemContent> GetItemsInInventories(List<IMyTerminalBlock> inventoryBlocks, int inventoryIndex = 0)
    {
        if (inventoryBlocks.Count == 0)
            return new Dictionary<string, ItemContent>();

        var result = new Dictionary<string, ItemContent>();
        foreach (var inventory in inventoryBlocks)
        {
            foreach (var item in GetItemsInInventory(inventory.GetInventory(inventoryIndex)))
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

    public static Dictionary<string, ItemContentAdvanced> GetItemsInInventory(IMyInventory inventory)
    {
        List<MyInventoryItem> items = new List<MyInventoryItem>();
        inventory.GetItems(items);
        var itemsDic = new Dictionary<string, ItemContentAdvanced>();
        for (var i = 0; i < items.Count; i++)
        {
            var item = items[i];
            var name = item.Type.SubtypeId;
            var quantity = (int)(item.Amount.RawValue / 1000000);
            if (!itemsDic.ContainsKey(name))
            {
                //Add item do return structure
                itemsDic.Add(name, new ItemContentAdvanced
                {
                    ItemName = name,
                    Quantity = (int)(item.Amount.RawValue / 1000000),
                    Index = i
                });
            }
            else
            {
                //There are multiple stacks of the item, so we should stack them together
                inventory.TransferItemTo(inventory, i, itemsDic[name].Index, true);
                itemsDic[name].Quantity += quantity;
            }
        }
        return itemsDic;
    }

    public class ItemContent
    {
        public string ItemName { get; set; }
        public int Quantity { get; set; }
    }

    public class ItemContentAdvanced : ItemContent
    {
        public int Index { get; set; }
    }
}