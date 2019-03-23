using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRage;
using VRage.Library;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRageMath;
using VRage.Game.ModAPI.Ingame;

namespace SEScripts.Modules
{
    #region Usings

    using CargoHelper = Helpers.CargoHelper;
    using GridBlocksHelper = Helpers.GridBlocksHelper;
    using LcdOutputHelper = Helpers.LcdOutputHelper;

    #endregion Usings

    #region SpaceEngineers

    public class ShowContainerContents
    {
        private IMyGridTerminalSystem GTS { get; set; }

        public ShowContainerContents()
        {
        }

        private ShowContainerContents(IMyGridTerminalSystem gts)
        {
            GTS = gts;
        }

        public static ShowContainerContents Get(IMyGridTerminalSystem gts)
        {
            return new ShowContainerContents(gts);
        }

        public void PrintContents(string lcdName, string containerName, string title)
        {
            PrintResultsOnLcd(lcdName, StringifyContainerContent(containerName), title);
        }

        public void PrintContentsWithSubtype(string lcdName, string containerName, string title, int timer)
        {
            var results = StringifyContainerContent(containerName);

            var containers = GridBlocksHelper.Prefixed(GTS, containerName).GetCargoContainers();
            if (containers.Count == 0)
                results = "Container not found.";

            var items = CargoHelper.GetItemsInInventory(containers[0].GetInventory(0));

            var itemsString = items.Select(item => item.ItemName + "(" + (item.IsOre ? "Ore" : "Ingot") + ")" + " - " + item.Quantity);

            results = String.Join("\n", itemsString);

            //public List<string> GetItemsInInventory(string containerName)
            //{
            //    // Get containers
            //    var containers = GridBlocksHelper.Prefixed(GTS, containerName).GetCargoContainers();
            //    if (containers.Count == 0)
            //        return new List<string> { "Container not found." };

            //    // Get items in inventories
            //    var itemsInDestinyInventory = CargoHelper.GetItemsInInventories(containers);

            //    // Build a string with the items
            //    return itemsInDestinyInventory.Values.Select(item => item.ItemName + " - " + item.Quantity).ToList();
            //}

            PrintResultsOnLcd(lcdName, results, title, timer);
        }

        public void PrintGroupContents(string lcdName, string title, string groupName, int timer)
        {
            PrintResultsOnLcd(lcdName, StringifyGroupContent(groupName), title, timer);
        }

        public void PrintContents(string lcdName, string containerName, Dictionary<string, int> componentDesiredQuantities)
        {
            PrintResultsOnLcd(lcdName, StringifyContainerContent(containerName, componentDesiredQuantities));
        }

        public string StringifyContainerContent(string containerName)
        {
            // Get containers
            var containers = GridBlocksHelper.Prefixed(GTS, containerName).GetCargoContainers();
            if (containers.Count == 0)
                return "Container not found.";

            return StringifyContainerContent(containers);
        }

        public string StringifyGroupContent(string groupName)
        {
            var inventories = GridBlocksHelper.Get(GTS).GetGroupBlocks(groupName).Where(t => t.HasInventory).ToList();
            if (inventories.Count == 0)
                return string.Format("There are no blocks in group {0} with inventory.", groupName);

            return StringifyContainerContent(inventories);
        }

        public string StringifyContainerContent(List<IMyTerminalBlock> inventories)
        {
            // Get items in inventories
            var itemsInDestinyInventory = CargoHelper.GetItemsInInventories(inventories);

            // Build a string with the items
            var itemsString = string.Empty;
            foreach (var item in itemsInDestinyInventory.Values)
            {
                itemsString += item.ItemName + " - " + item.Quantity + "\n";
            }
            return itemsString;
        }

        public string StringifyContainerContent(string containerName, Dictionary<string, int> componentDesiredQuantities)
        {
            // Get containers
            var containers = GridBlocksHelper.Prefixed(GTS, containerName).GetCargoContainers();
            if (containers.Count == 0)
                return "Container not found.";

            // Get items in inventories
            var itemsInDestinyInventory = CargoHelper.GetItemsInInventories(containers);

            // Build a string with the items
            var itemsString = string.Empty;
            if (componentDesiredQuantities == null || componentDesiredQuantities.Count == 0)
            {
                foreach (var item in itemsInDestinyInventory.Values)
                {
                    itemsString += item.ItemName + " - " + item.Quantity + "\n";
                }
                return itemsString;
            }

            foreach (var desired in componentDesiredQuantities)
            {
                var quantity = itemsInDestinyInventory.ContainsKey(desired.Key) ? itemsInDestinyInventory[desired.Key].Quantity : 0;
                if (quantity == 0 && desired.Value == 0)
                    continue;
                var percentage = getPercentage(quantity, desired.Value);

                itemsString += desired.Key + " - " + quantity + "(" + percentage + "%) " + (percentage < 100 ? "<=========" : string.Empty) + "\n";
            }

            return itemsString;
        }

        private int getPercentage(int quantity, int desired)
        {
            if (desired == 0)
                return 100;
            return (int)Math.Round((decimal)quantity / desired * 100);
        }

        public void PrintResultsOnLcd(string lcdName, string results, string title = "=================================", int timer = 0)
        {
            //Get the lcd(s)
            var lcds = GridBlocksHelper.Prefixed(GTS, lcdName).GetLcdsPrefixed();
            if (lcds.Count == 0)
                throw new Exception(string.Format("No lcd found with name starting with {0}", lcdName));

            // Print the message on the lcd(s)
            LcdOutputHelper.ShowResultWithProgress(lcds, results, title, timer);
        }
    }

    #endregion SpaceEngineers
}