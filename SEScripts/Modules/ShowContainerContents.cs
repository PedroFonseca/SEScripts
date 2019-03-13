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

public class ShowContainerContents : Skeleton
{
    // This scripts shows in one or more lcd panels the contents of one or more cargo containers.

    //======================================== INSTRUCTIONS =========================== 
    //  You need one trigger and one programable block to run this script continuously. Setup instructions:
    //      a) This script should be inside a programable block.
    //      b) Place a Timer block component and set it up to run every second with the following 
    //          actions in this order:
    //              - Run Programmable block (default) 
    //              - Timer block (Start) 
    //      c) Execute Trigger Now action on the Timer block to start the script
    //=================================================================================

    // Name of the container to show the contents
    public string ContainerName = "Small Cargo Container 1 ";
    // Name of the lcd to display the information's where you want to display the contents of destiny container (set text to public)
    public string LcdName = "S.HERM LCD Airlock";
    // The header of text to be shown in the lcd screens (should identify what you are displaying)
    public string Header = "Contents in Cargo Container";
    // Insert here how much of each component you would like to have in destiny container (to show a percentage)
    // Tips: This can be null if you don't want a percentage. 
    public Dictionary<string, int> ComponentDesiredQuantities = new Dictionary<string, int>
        {
            { "SteelPlate",       1000 },
            { "InteriorPlate",     500 },
            { "Motor",             500 },
            { "Construction",      500 },
            { "SmallTube",         300 },
            { "LargeTube",         250 },
            { "Computer",          200 },
            { "MetalGrid",         200 },
            { "BulletproofGlass",   30 },
            { "Display",            20 },
            { "Girder",             10 },
            { "Detector",            0 },
            { "Explosives",          0 },
            { "GravityGenerator",    0 },
            { "Medical",             0 },
            { "PowerCell",           0 },
            { "RadioCommunication",  0 },
            { "Reactor",             0 },
            { "SolarCell",           0 },
            { "SuperConductor",      0 },
            { "Thrust",              0 }
        };

    // Hint: names don't have to be unique. You can count components inside two containers xpto1 and xpto2 if in the name you type xpto

    public void Main(string argument)
    {
        PrintResultsOnLcd(LcdName, GetContainerContent(ContainerName));
    }

    public string GetContainerContent(string containerName)
    {
        // Get containers 
        var containers = new GetComponentsHelper(GridTerminalSystem, ContainerName).GetCargoContainers();
        if (containers.Count == 0)
            return "Container not found.";

        // Get items in inventories
        var itemsInDestinyInventory = CargoHelper.GetItemsInInventories(containers);

        // Build a string with the items
        var itemsString = string.Empty;
        if (ComponentDesiredQuantities == null || ComponentDesiredQuantities.Count == 0)
        {
            foreach (var item in itemsInDestinyInventory.Values)
            {
                itemsString += item.ItemName + " - " + item.Quantity + "\n";
            }
            return itemsString;
        }
        
        foreach (var desired in ComponentDesiredQuantities)
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

    public void PrintResultsOnLcd(string lcdName, string results)
    {
        //Get the lcd(s)
        var lcds = new GetComponentsHelper(GridTerminalSystem, lcdName).GetLcdsPrefixed();
        if (lcds.Count == 0)
            throw new Exception(string.Format("No lcd found with name starting with {0}", lcdName));

        // Print the message on the lcd(s)
        LcdOutputHelper.ShowResultWithProgress(lcds, results);
    }

    public class GetComponentsHelper
    {
        public string Prefix { get; private set; }
        private IMyGridTerminalSystem GTS { get; set; }
        public GetComponentsHelper() { }
        public GetComponentsHelper(IMyGridTerminalSystem gts, string prefix)
        {
            Prefix = prefix;
            GTS = gts;
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

        public List<IMyTerminalBlock> GetCargoContainers()
        {
            var aux = new List<IMyTerminalBlock>();
            GTS.GetBlocksOfType<IMyCargoContainer>(aux, NameStartsWithPrefix);
            return aux;
        }
    }

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

        public static void ShowResultWithProgress(List<IMyTextPanel> lcds, string message)
        {
            if (lcds == null || lcds.Count == 0)
                return;

            message = "=================================\n" + message + "\n  " + getTimmerChar();

            var msg = new LcdMessage(message, Color.White);
            foreach (var lcd in lcds)
            {
                ShowMessageOnLcd(lcd, msg);
            }
        }

        // Timmer is used to show something different every iteration
        static int timmer = 0;
        private static string getTimmerChar()
        {
            // Move timmer
            timmer++;

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

    public static class CargoHelper
    {
        public static Dictionary<string, ItemContent> GetItemsInInventories(List<IMyTerminalBlock> inventoryBlocks)
        {
            if (inventoryBlocks.Count == 0)
                return new Dictionary<string, ItemContent>();

            var result = new Dictionary<string, ItemContent>();
            foreach (var inventory in inventoryBlocks)
            {
                foreach (var item in GetItemsInInventory(inventory.GetInventory(0)))
                {
                    if (!result.ContainsKey(item.Key)) {
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
}
