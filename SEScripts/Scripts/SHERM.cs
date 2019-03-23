﻿using Sandbox.ModAPI.Ingame;
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
        // Insert here how much of each component you would like to have in destiny container (to show a percentage)
        // Tips: This can be null if you don't want a percentage. 
        public Dictionary<string, int> Welder1DesiredQuantities = new Dictionary<string, int>
        {
            { "SteelPlate",         500 },
            { "InteriorPlate",      500 },
            { "Motor",              150 },
            { "Construction",       500 },
            { "SmallTube",          500 },
            { "LargeTube",          100 },
            { "Computer",           500 },
            { "MetalGrid",          100 },
            { "BulletproofGlass",   100 },
            { "Display",            100 },
            { "Girder",             100 },
            { "Detector",           100 },
            { "Explosives",           0 },
            { "GravityGenerator",     0 },
            { "Medical",              0 },
            { "PowerCell",            0 },
            { "RadioCommunication",   0 },
            { "Reactor",              0 },
            { "SolarCell",            0 },
            { "SuperConductor",       0 },
            { "Thrust",               0 }
        };

        public List<string> ores = new List<string>() { "Stone", "Gravel", "Iron", "Nickel", "Magnesium", "Silicon", "Cobalt", "Gold", "Silver", "Uranium", "Platinum", "Ice" };

        public void Main(string argument, UpdateType updateSource)
        {
            var debug = string.Empty;
            //AutoMove.Get(GridTerminalSystem).MoveAll(Welder1DesiredQuantities.Keys.ToList(), "S.HERM Cargo Components Container", new List<string>{ "Welder 1 Cargo Container" });
            AutoMove.Get(GridTerminalSystem).MoveAll(ores, "S.HERM Cargo Ore / Ingot Container", new List<string>() { });
            AutoMove.Get(GridTerminalSystem).MoveAll(new List<string>() { "Ice" }, "S.HERM Cargo Ice Container", new List<string>() { });
            // Fill welder contents
            debug = AutoMove.Get(GridTerminalSystem).MoveToQuota("S.HERM Cargo Components Container", "Welder 1 Cargo Container", Welder1DesiredQuantities);

            // Show contents of components container
            ShowContainerContents.Get(GridTerminalSystem).PrintContents("S.HERM LCD Components", "S.HERM Cargo Components Container", "=== S.HERM Cargo Components ===");

            // Show contents of ores container
            ShowContainerContents.Get(GridTerminalSystem).PrintContents("S.HERM LCD Ores", "S.HERM Cargo Ore / Ingot Container", "=== S.HERM Cargo Ore / Ingot ===");

            // Show contents of welder1 container
            ShowContainerContents.Get(GridTerminalSystem).PrintContents("S.HERM LCD Welder 1", "Welder 1 Cargo Container", "=== Welder 1 contents ===");

            //Debug messages
            var lcds = GridBlocksHelper.Prefixed(GridTerminalSystem, "S.HERM LCD Airlock debug").GetLcdsPrefixed();
            if (lcds.Count == 0)
                throw new Exception(string.Format("No lcd found with name starting with {0}", "S.HERM LCD Airlock debug"));

            // Print the message on the lcd(s)
            LcdOutputHelper.ShowMessageOnLcd(lcds[0], new LcdOutputHelper.LcdMessage("Debug:\n" + debug));
            
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

            public void PrintContents(string lcdName, string containerName, string title)
            {
                PrintResultsOnLcd(lcdName, StringifyContainerContent(containerName), title);
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

                // Get items in inventories
                var itemsInDestinyInventory = CargoHelper.GetItemsInInventories(containers);

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

            public void PrintResultsOnLcd(string lcdName, string results, string title="=================================")
            {
                //Get the lcd(s)
                var lcds = GridBlocksHelper.Prefixed(GTS, lcdName).GetLcdsPrefixed();
                if (lcds.Count == 0)
                    throw new Exception(string.Format("No lcd found with name starting with {0}", lcdName));

                // Print the message on the lcd(s)
                LcdOutputHelper.ShowResultWithProgress(lcds, results, title);
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

            public static void ShowResultWithProgress(List<IMyTextPanel> lcds, string message, string title= "=================================")
            {
                if (lcds == null || lcds.Count == 0)
                    return;

                message = title + "\n" + message + "\n  " + getTimmerChar();

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
            public List<string> ExceptionList { get; private set; }
            private IMyGridTerminalSystem GTS { get; set; }
            public GridBlocksHelper() { }
            private GridBlocksHelper(IMyGridTerminalSystem gts, string prefix, List<string> exceptionList)
            {
                Prefix = prefix;
                ExceptionList = exceptionList;
                GTS = gts;
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
        }

        public class AutoMove
        {
            private IMyGridTerminalSystem GTS { get; set; }
            public AutoMove() { }
            private AutoMove(IMyGridTerminalSystem gts)
            {
                GTS = gts;
            }

            public static AutoMove Get(IMyGridTerminalSystem gts)
            {
                return new AutoMove(gts);
            }

            // Move components from grid to container (except from exception container list)
            public string MoveAll(List<string> components, string destinyContainerName, List<string> exceptionList)
            {
                var origin = GridBlocksHelper.WithExceptions(GTS, exceptionList).GetCargoContainersWithException();
                if (origin.Count == 0)
                    return string.Format("Could not find any container not in exceptions.");

                var destiny = GridBlocksHelper.Prefixed(GTS, destinyContainerName).GetCargoContainers();
                if (destiny.Count > 1)
                    return string.Format("Multiple containers were found with name {0}. Make sure you have only one.", destinyContainerName);
                else if (destiny.Count == 0)
                {
                    return string.Format("Container with name {0} not found on grid.", destinyContainerName);
                }

                // Get inventories of both origin and destiny containers
                var originInventories = origin.Select(t => t.GetInventory(0)).ToList();
                var destinyInventory = destiny[0].GetInventory(0);

                // Get items on destiny container inventory
                var itemsInDestinyInventory = CargoHelper.GetItemsInInventory(destinyInventory);

                var debugMessage = string.Empty;
                foreach (var originInventory in originInventories)
                {
                    var itemsInOriginInventory = CargoHelper.GetItemsInInventory(originInventory);
                    // Move components into destiny container
                    foreach (var component in components)
                    {
                        if (!itemsInOriginInventory.ContainsKey(component))
                        {
                            continue;
                        }
                        var destinyIndex = itemsInDestinyInventory.ContainsKey(component) ? itemsInDestinyInventory[component].Index : destinyInventory.ItemCount;
                        //debugMessage += "Moving " + component + "from " + origin[0].CustomName + " to " + destinyIndex + "\n";
                        originInventory.TransferItemTo(destinyInventory,
                                itemsInOriginInventory[component].Index,
                                destinyIndex,
                                true);
                    }
                }

                return debugMessage;
            }

            public string MoveToQuota(string originContainerName, string destinyContainerName, Dictionary<string, int> componentDesiredQuantities)
            {
                //====================================== Move components ==========================
                // Move components from one container into another
                var origin = GridBlocksHelper.Prefixed(GTS, originContainerName).GetCargoContainers();
                if (origin.Count == 0)
                    return string.Format("Could not find any container with the name {0}.", originContainerName);
                else if (origin.Count > 1)
                    return string.Format("Multiple containers were found with name {0}. Make sure you have only one.", originContainerName);


                var destiny = GridBlocksHelper.Prefixed(GTS, destinyContainerName).GetCargoContainers();
                if (destiny.Count > 1)
                    return string.Format("Multiple containers were found with name {0}. Make sure you have only one.", destinyContainerName);
                else if (destiny.Count == 0)
                {
                    return string.Format("Container with name {0} not found on grid.", destinyContainerName);
                }

                // Get inventories of both origin and destiny containers
                var originInventory = origin[0].GetInventory(0);
                var destinyInventory = destiny[0].GetInventory(0);

                // Get items on destiny container inventory
                var itemsInOriginInventory = CargoHelper.GetItemsInInventory(originInventory);
                var itemsInDestinyInventory = CargoHelper.GetItemsInInventory(destinyInventory);

                // Move components into destiny container
                var debugMessage = string.Empty;
                foreach (var component in componentDesiredQuantities)
                {
                    if (component.Value <= 0 || !itemsInOriginInventory.ContainsKey(component.Key))
                        continue;

                    // Calculate the quantity to move
                    var quantityToMove = component.Value;
                    if (itemsInDestinyInventory.ContainsKey(component.Key))
                    {
                        quantityToMove = component.Value - itemsInDestinyInventory[component.Key].Quantity;
                    }

                    // Move items
                    if (quantityToMove > 0)
                    {
                        var a = (VRage.MyFixedPoint)quantityToMove;
                        var destinyIndex = itemsInDestinyInventory.ContainsKey(component.Key) ? itemsInDestinyInventory[component.Key].Index :
                                            destinyInventory.ItemCount;
                        debugMessage += "Moving " + quantityToMove + " of " + component.Key + "from pos " + itemsInOriginInventory[component.Key].Index + " to " + destinyIndex + "\n";
                        originInventory.TransferItemTo(destinyInventory,
                            itemsInOriginInventory[component.Key].Index,
                            destinyIndex,
                            true,
                            quantityToMove);
                    }
                }

                return debugMessage;
            }
        }
    }
}