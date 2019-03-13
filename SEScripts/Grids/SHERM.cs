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
    public class SHERM : Skeleton
    {
        // Name of the lcd to display the information's where you want to display the contents of destiny container (set text to public)
        public string LcdName = "S.HERM LCD Airlock";
        // The header of text to be shown in the lcd screens (should identify what you are displaying)
        public string Header = "Contents in Cargo Container";
        // Insert here how much of each component you would like to have in destiny container (to show a percentage)
        // Tips: This can be null if you don't want a percentage. 
        public Dictionary<string, int> ComponentDesiredQuantities = new Dictionary<string, int>
        {
            { "SteelPlate",       10000 },
            { "InteriorPlate",    10000 },
            { "Motor",             2500 },
            { "Construction",      5000 },
            { "SmallTube",         5000 },
            { "LargeTube",         2500 },
            { "Computer",          5000 },
            { "MetalGrid",         1000 },
            { "BulletproofGlass",  1000 },
            { "Display",           2000 },
            { "Girder",            1000 },
            { "Detector",           100 },
            { "Explosives",           0 },
            { "GravityGenerator",     0 },
            { "Medical",            100 },
            { "PowerCell",            0 },
            { "RadioCommunication", 500 },
            { "Reactor",              0 },
            { "SolarCell",            0 },
            { "SuperConductor",       0 },
            { "Thrust",               0 }
        };

        public void Main(string argument, UpdateType updateSource)
        {
            // Show contents of components container
            // LcdName: S.HERM LCD Airlock
            // ContainerName: S.HERM Cargo Components Container
            ShowContainerContents.Get(GridTerminalSystem).PrintContents("S.HERM LCD Airlock", "S.HERM Cargo Components Container", ComponentDesiredQuantities);
        }

        public class ShowContainerContents
        {
            private IMyGridTerminalSystem GTS { get; set; }
            public ShowContainerContents() { }
            private ShowContainerContents(IMyGridTerminalSystem gts)
            {
                GTS = gts;
            }

            public static ShowContainerContents Get(IMyGridTerminalSystem gts)
            {
                return new ShowContainerContents(gts);
            }

            public void PrintContents(string lcdName, string containerName, Dictionary<string, int> componentDesiredQuantities)
            {
                PrintResultsOnLcd(lcdName, StringifyContainerContent(containerName, componentDesiredQuantities));
            }

            public string StringifyContainerContent(string containerName, Dictionary<string, int> componentDesiredQuantities)
            {
                // Get containers 
                var containers = GridBlocksHelper.Get(GTS, containerName).GetCargoContainers();
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

            public void PrintResultsOnLcd(string lcdName, string results)
            {
                //Get the lcd(s)
                var lcds = GridBlocksHelper.Get(GTS, lcdName).GetLcdsPrefixed();
                if (lcds.Count == 0)
                    throw new Exception(string.Format("No lcd found with name starting with {0}", lcdName));

                // Print the message on the lcd(s)
                LcdOutputHelper.ShowResultWithProgress(lcds, results);
            }
        }

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

        public class GridBlocksHelper
        {
            public string Prefix { get; private set; }
            private IMyGridTerminalSystem GTS { get; set; }
            public GridBlocksHelper() { }
            private GridBlocksHelper(IMyGridTerminalSystem gts, string prefix)
            {
                Prefix = prefix;
                GTS = gts;
            }

            public static GridBlocksHelper Get(IMyGridTerminalSystem gts, string prefix)
            {
                return new GridBlocksHelper(gts, prefix);
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
        }
    }
}