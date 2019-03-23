
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game.ModAPI.Ingame;
using VRageMath;

namespace SEScripts.Grids
{
    public class StarBasea : Skeleton
    {
        
	// File: StarBase.cs

        // Timmer is used to show something different every iteration
        public static int timer = 0;

        public void Main(string argument, UpdateType updateSource)
        {
            var debug = string.Empty;

            // Move timmer
            timer++;

            // Show contents of ores container
            ShowContainerContents.Get(GridTerminalSystem)
                .PrintContentsWithSubtype("SB LCD Ores", "SB - Ores", "=== StartBase Ore / Ingot ===", timer);
        }

	// File: ShowContainerContents.cs

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

	// File: CargoHelper.cs

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



	// File: GridBlocksHelper.cs

    public class GridBlocksHelper
    {
        public string Prefix { get; private set; }
        public List<string> ExceptionList { get; private set; }
        private IMyGridTerminalSystem GTS { get; set; }

        public GridBlocksHelper()
        {
        }

        private GridBlocksHelper(IMyGridTerminalSystem gts, string prefix, List<string> exceptionList)
        {
            Prefix = prefix;
            ExceptionList = exceptionList;
            GTS = gts;
        }

        public static GridBlocksHelper Get(IMyGridTerminalSystem gts)
        {
            return new GridBlocksHelper(gts, string.Empty, null);
        }

        public static GridBlocksHelper Prefixed(IMyGridTerminalSystem gts, string prefix)
        {
            return new GridBlocksHelper(gts, prefix, null);
        }

        public static GridBlocksHelper WithExceptions(IMyGridTerminalSystem gts, List<string> exceptionList)
        {
            return new GridBlocksHelper(gts, string.Empty, exceptionList);
        }

        private bool NameStartsWithPrefix(IMyTerminalBlock block)
        {
            if (string.IsNullOrEmpty(Prefix))
                return true;
            return block.CustomName.StartsWith(Prefix) && block.IsFunctional;
        }

        private bool NameEqualsPrefix(IMyTerminalBlock block)
        {
            if (string.IsNullOrEmpty(Prefix))
                return true;
            return block.CustomName.Equals(Prefix) && block.IsFunctional;
        }

        private bool NameIsNotException(IMyTerminalBlock block)
        {
            if (ExceptionList == null || ExceptionList.Count == 0)
                return true;

            foreach (var name in ExceptionList)
            {
                if (block.CustomName.Equals(name) && block.IsFunctional)
                {
                    return true;
                }
            }
            return false;
        }

        private List<T> ConvertToListOf<T>(List<IMyTerminalBlock> list) where T : class
        {
            var result = new List<T>();
            foreach (var elem in list)
            {
                result.Add(elem as T);
            }
            return result;
        }

        private List<T> GetBlocksOfTypeStartsWithPrefix<T>() where T : class
        {
            var aux = new List<IMyTerminalBlock>();
            GTS.GetBlocksOfType<T>(aux, NameStartsWithPrefix);
            return ConvertToListOf<T>(aux);
        }

        private List<T> GetBlocksOfTypeByName<T>() where T : class
        {
            var aux = new List<IMyTerminalBlock>();
            GTS.GetBlocksOfType<T>(aux, NameStartsWithPrefix);
            return ConvertToListOf<T>(aux);
        }

        public List<IMyTextPanel> GetLcdsPrefixed()
        {
            return GetBlocksOfTypeStartsWithPrefix<IMyTextPanel>();
        }

        public IMyMotorStator GetRotor()
        {
            var aux = new List<IMyTerminalBlock>();
            GTS.GetBlocksOfType<IMyMotorStator>(aux, NameStartsWithPrefix);

            if (aux == null || aux.Count == 0)
            {
                throw new NullReferenceException(string.Format("Could not find any rotor with name starting by {0}.", Prefix));
            }
            else if (aux.Count > 1)
            {
                throw new NullReferenceException(string.Format("Multiple rotors were found with name starting by {0}. Make sure you have only one.", Prefix));
            }
            return aux[0] as IMyMotorStator;
        }

        public List<IMyTerminalBlock> GetSolarPanels()
        {
            var aux = new List<IMyTerminalBlock>();
            GTS.GetBlocksOfType<IMySolarPanel>(aux, NameStartsWithPrefix);
            if (aux == null || aux.Count == 0)
            {
                throw new NullReferenceException(string.Format("Could not find any solar panel with name starting by {0}.", Prefix));
            }
            return aux;
        }

        public List<IMyTerminalBlock> GetBatteries()
        {
            var aux = new List<IMyTerminalBlock>();
            GTS.GetBlocksOfType<IMyBatteryBlock>(aux, NameStartsWithPrefix);
            return aux;
        }

        public List<IMyTerminalBlock> GetReactors()
        {
            var aux = new List<IMyTerminalBlock>();
            GTS.GetBlocksOfType<IMyReactor>(aux, NameStartsWithPrefix);
            return aux;
        }

        public List<IMyTerminalBlock> GetCargoContainers()
        {
            var aux = new List<IMyTerminalBlock>();
            GTS.GetBlocksOfType<IMyCargoContainer>(aux, NameStartsWithPrefix);
            return aux;
        }

        public List<IMyTerminalBlock> GetCargoContainersWithException()
        {
            var aux = new List<IMyTerminalBlock>();
            GTS.GetBlocksOfType<IMyCargoContainer>(aux, NameIsNotException);
            return aux;
        }

        public List<IMyTerminalBlock> GetAssemblers()
        {
            var aux = new List<IMyTerminalBlock>();
            GTS.GetBlocksOfType<IMyAssembler>(aux, NameStartsWithPrefix);
            return aux;
        }

        public List<IMyTerminalBlock> GetRefineries()
        {
            var aux = new List<IMyTerminalBlock>();
            GTS.GetBlocksOfType<IMyRefinery>(aux, NameStartsWithPrefix);
            return aux;
        }

        public List<IMyTerminalBlock> GetGroupBlocks(string groupName)
        {
            var aux = new List<IMyTerminalBlock>();
            GTS.GetBlockGroupWithName(groupName).GetBlocks(aux);
            return aux;
        }
    }



	// File: LcdOutputHelper.cs

    public static class LcdOutputHelper
    {
        public static void ShowResult(IMyTextPanel lcd, string message)
        {
            ShowMessageOnLcd(lcd, new LcdMessage(message, Color.White));
        }

        public static void ShowResult(List<IMyTextPanel> lcds, string message)
        {
            if (lcds == null || lcds.Count == 0)
                return;
            var msg = new LcdMessage(message, Color.White);
            foreach (var lcd in lcds)
            {
                ShowMessageOnLcd(lcd, msg);
            }
        }

        public static void ShowResultWithProgress(List<IMyTextPanel> lcds, string message, string title = "=================================", int timer = 0)
        {
            if (lcds == null || lcds.Count == 0)
                return;

            message = title + "\n" + message + "\n  " + getTimmerChar(timer);

            var msg = new LcdMessage(message, Color.White);
            foreach (var lcd in lcds)
            {
                ShowMessageOnLcd(lcd, msg);
            }
        }

        private static string getTimmerChar(int timmer)
        {
            switch (timmer)
            {
                case 1: return "\\";
                case 2: return "|";
                case 3: return "/";
                default: timmer = 0; return "-";
            }
        }

        public static void ShowMessageOnLcd(IMyTextPanel lcd, LcdMessage message)
        {
            if (lcd == null) return;

            lcd.WritePublicText(message.Text);
            lcd.ShowPublicTextOnScreen();
            lcd.SetValue<Color>("FontColor", message.FontColor);
            lcd.SetValue<Color>("BackgroundColor", message.BackgroundColor);
            lcd.SetValueFloat("FontSize", message.FontSize);
        }

        // Doesn't work
        public static void ShowMessagesOnLcd(IMyTextPanel lcd, List<LcdMessage> messages)
        {
            if (lcd == null) return;

            foreach (var message in messages)
            {
                lcd.SetValue<Color>("FontColor", message.FontColor);
                lcd.SetValue<Color>("BackgroundColor", message.BackgroundColor);
                lcd.SetValueFloat("FontSize", message.FontSize);
                lcd.WritePublicText(message.Text, true);
            }
            lcd.ShowPublicTextOnScreen();
        }

        public struct LcdMessage
        {
            public string Text { get; set; }
            public Color FontColor { get; set; }
            public Color BackgroundColor { get; set; }
            public float FontSize { get; set; }

            public LcdMessage(string text)
            {
                Text = text;
                FontColor = Color.White;
                BackgroundColor = Color.Black;
                FontSize = 1.1f;
            }

            public LcdMessage(string text, Color fontColor)
            {
                Text = text;
                FontColor = fontColor;
                BackgroundColor = Color.Black;
                FontSize = 1.1f;
            }
        }
    }




    }
}