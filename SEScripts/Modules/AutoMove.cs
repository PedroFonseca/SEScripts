using System;
using System.Linq;
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
using SEScripts.Helpers;


//  Auto move components features:
//  1) Automatically move up components from anywhere in the grid into the specified container (up to a configured ammount)
// Exemple:
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

    public string MoveAll(List<string> components, string destinyContainerName, List<string> exceptionList)
    {
        //====================================== Move components ==========================
        // Move components from one container into another
        var helper = GridBlocksHelper.WithExceptions(GTS, exceptionList);
        var origin = helper.GetCargoContainersWithExceptions().Concat(
                        helper.GetAssemblers()).ToList();
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
}


////public AutoMoveComponents.Config AutoMoveComponentsToWelderConfig = new AutoMoveComponents.Config
////{
////    // The name of the main container with components (components will be fetched from here)
////    OriginContainerName = "Components Container",
////    // The name of the containter where you want to move the components
////    DestinyContainerName = "Welder Container",
////    // Insert here how much of each component you want to move into the destiny container
////    ComponentDesiredQuantities = new Dictionary<string, int>
////    {
////        { "SteelPlate",       1000 },
////        { "InteriorPlate",     500 },
////        { "Motor",             500 },
////        { "Construction",      500 },
////        { "SmallTube",         300 },
////        { "LargeTube",         250 },
////        { "Computer",          200 },
////        { "MetalGrid",         200 },
////        { "BulletproofGlass",   30 },
////        { "Display",            20 },
////        { "Girder",             10 },
////        { "Detector",            0 },
////        { "Explosives",          0 },
////        { "GravityGenerator",    0 },
////        { "Medical",             0 },
////        { "PowerCell",           0 },
////        { "RadioCommunication",  0 },
////        { "Reactor",             0 },
////        { "SolarCell",           0 },
////        { "SuperConductor",      0 },
////        { "Thrust",              0 }
////    }
////};

//public void Main(string argument, UpdateType updateSource)
//    {
//        // Execute script to automatically move components up to a certain quota to one connected container
//        var autoMoveResult = AutoMoveComponents.Run(GridTerminalSystem, AutoMoveComponentsToWelderConfig);

//        //Get the lcd(s) to show status and errors named debugger (if it doesn't exist we can't show errors/exceptions).
//        var debuggerLcds = GridBlocksHelper.Get(GTS, "auto move welder 1").GetLcdsPrefixed();
//        if (debuggerLcds.Count > 0)
//        {
//            LcdOutputHelper.ShowResultWithProgress(debuggerLcds, autoMoveResult);
//        }
//    }

//    public class AutoMoveComponents
//    {
//        public class Config
//        {
//            public string OriginContainerName { get; set; }
//            public string DestinyContainerName { get; set; }
//            public Dictionary<string, int> ComponentDesiredQuantities { get; set; }

//            public Config Clone(string destinyContainer)
//            {
//                return new Config
//                {
//                    OriginContainerName = OriginContainerName,
//                    DestinyContainerName = destinyContainer,
//                    ComponentDesiredQuantities = ComponentDesiredQuantities
//                };
//            }
//        }

//        public static string Run(IMyGridTerminalSystem gts, Config config)
//        {
//            //====================================== Move components ==========================
//            // Move components from one container into another
//            var origin = new GetComponentsHelper(gts, config.OriginContainerName).GetCargoContainers();
//            if (origin.Count == 0)
//                return string.Format("Could not find any container with the name {0}.", config.OriginContainerName);
//            else if (origin.Count > 1)
//                return string.Format("Multiple containers were found with name {0}. Make sure you have only one.", config.OriginContainerName);

//            var destiny = new GetComponentsHelper(gts, config.DestinyContainerName).GetCargoContainers();
//            if (destiny.Count > 1)
//                return string.Format("Multiple containers were found with name {0}. Make sure you have only one.", config.DestinyContainerName);
//            else if (destiny.Count == 0)
//            {
//                return string.Format("Container with name {0} not found on grid.", config.DestinyContainerName);
//            }

//            // Get inventories of both origin and destiny containers
//            var originInventory = origin[0].GetInventory(0);
//            var destinyInventory = destiny[0].GetInventory(0);

//            // Get items on destiny container inventory
//            var itemsInOriginInventory = GetItemsInInventory(originInventory);
//            var itemsInDestinyInventory = GetItemsInInventory(destinyInventory);

//            // Move components into destiny container
//            var debugMessage = string.Empty;
//            foreach (var component in config.ComponentDesiredQuantities)
//            {
//                if (component.Value <= 0 || !itemsInOriginInventory.ContainsKey(component.Key))
//                    continue;

//                // Calculate the quantity to move
//                var quantityToMove = component.Value;
//                if (itemsInDestinyInventory.ContainsKey(component.Key))
//                {
//                    quantityToMove = component.Value - itemsInDestinyInventory[component.Key].Quantity;
//                }

//                // Move items
//                if (quantityToMove > 0)
//                {
//                    var destinyIndex = itemsInDestinyInventory.ContainsKey(component.Key) ? itemsInDestinyInventory[component.Key].Index :
//                                        destinyInventory..GetItems().Count;
//                    debugMessage += "Moving " + quantityToMove + " of " + component.Key + "from pos " + itemsInOriginInventory[component.Key].Index + " to " + destinyIndex + "\n";
//                    originInventory.TransferItemTo(destinyInventory,
//                        itemsInOriginInventory[component.Key].Index,
//                        destinyIndex,
//                        true, new VRage.MyFixedPoint { RawValue = quantityToMove * 1000000 });
//                }
//            }

//            return debugMessage;
//        }

//        //public static Dictionary<string, ItemContent> GetItemsInInventory(IMyInventory inventory)
//        //{
//        //    List<MyInventoryItem> items = new List<MyInventoryItem>();
//        //    inventory.GetItems(items);
//        //    var itemsDic = new Dictionary<string, ItemContent>();
//        //    for (var i = 0; i < items.Count; i++)
//        //    {
//        //        var item = items[i];
//        //        var name = item.Content.SubtypeName;
//        //        var quantity = (int)(item.Amount.RawValue / 1000000);
//        //        if (!itemsDic.ContainsKey(name))
//        //        {
//        //            //Add item do return structure
//        //            itemsDic.Add(name, new ItemContent
//        //            {
//        //                ItemName = name,
//        //                Quantity = (int)(item.Amount.RawValue / 1000000),
//        //                Index = i
//        //            });
//        //        }
//        //        else
//        //        {
//        //            //There are multiple stacks of the item, so we should stack them together
//        //            inventory.TransferItemTo(inventory, i, itemsDic[name].Index, true);
//        //            itemsDic[name].Quantity += quantity;
//        //        }
//        //    }
//        //    return itemsDic;
//        //}

//        //public class ItemContent
//        //{
//        //    public string ItemName { get; set; }
//        //    public int Quantity { get; set; }
//        //    public int Index { get; set; }
//        //}
//    }

//    //public class GetComponentsHelper
//    //{
//    //    public string Prefix { get; private set; }
//    //    private IMyGridTerminalSystem GTS { get; set; }
//    //    public GetComponentsHelper() { }
//    //    public GetComponentsHelper(IMyGridTerminalSystem gts, string prefix)
//    //    {
//    //        Prefix = prefix;
//    //        GTS = gts;
//    //    }

//    //    private bool NameStartsWithPrefix(IMyTerminalBlock block)
//    //    {
//    //        if (string.IsNullOrEmpty(Prefix))
//    //            return true;
//    //        return block.CustomName.StartsWith(Prefix) && block.IsFunctional;
//    //    }

//    //    private bool NameEqualsPrefix(IMyTerminalBlock block)
//    //    {
//    //        if (string.IsNullOrEmpty(Prefix))
//    //            return true;
//    //        return block.CustomName.Equals(Prefix) && block.IsFunctional;
//    //    }

//    //    private List<T> ConvertToListOf<T>(List<IMyTerminalBlock> list) where T : class
//    //    {
//    //        var result = new List<T>();
//    //        foreach (var elem in list)
//    //        {
//    //            result.Add(elem as T);
//    //        }
//    //        return result;
//    //    }

//    //    private List<T> GetBlocksOfTypeStartsWithPrefix<T>() where T : class
//    //    {
//    //        var aux = new List<IMyTerminalBlock>();
//    //        GTS.GetBlocksOfType<T>(aux, NameStartsWithPrefix);
//    //        return ConvertToListOf<T>(aux);
//    //    }

//    //    private List<T> GetBlocksOfTypeByName<T>() where T : class
//    //    {
//    //        var aux = new List<IMyTerminalBlock>();
//    //        GTS.GetBlocksOfType<T>(aux, NameStartsWithPrefix);
//    //        return ConvertToListOf<T>(aux);
//    //    }

//    //    public List<IMyTextPanel> GetLcdsPrefixed()
//    //    {
//    //        return GetBlocksOfTypeStartsWithPrefix<IMyTextPanel>();
//    //    }

//    //    public IMyMotorStator GetRotor()
//    //    {
//    //        var aux = new List<IMyTerminalBlock>();
//    //        GTS.GetBlocksOfType<IMyMotorStator>(aux, NameStartsWithPrefix);

//    //        if (aux == null || aux.Count == 0)
//    //        {
//    //            throw new NullReferenceException(string.Format("Could not find any rotor with name starting by {0}.", Prefix));
//    //        }
//    //        else if (aux.Count > 1)
//    //        {
//    //            throw new NullReferenceException(string.Format("Multiple rotors were found with name starting by {0}. Make sure you have only one.", Prefix));
//    //        }
//    //        return aux[0] as IMyMotorStator;
//    //    }

//    //    public List<IMyTerminalBlock> GetSolarPanels()
//    //    {
//    //        var aux = new List<IMyTerminalBlock>();
//    //        GTS.GetBlocksOfType<IMySolarPanel>(aux, NameStartsWithPrefix);
//    //        if (aux == null || aux.Count == 0)
//    //        {
//    //            throw new NullReferenceException(string.Format("Could not find any solar panel with name starting by {0}.", Prefix));
//    //        }
//    //        return aux;
//    //    }

//    //    public List<IMyTerminalBlock> GetBatteries()
//    //    {
//    //        var aux = new List<IMyTerminalBlock>();
//    //        GTS.GetBlocksOfType<IMyBatteryBlock>(aux, NameStartsWithPrefix);
//    //        return aux;
//    //    }

//    //    public List<IMyTerminalBlock> GetReactors()
//    //    {
//    //        var aux = new List<IMyTerminalBlock>();
//    //        GTS.GetBlocksOfType<IMyReactor>(aux, NameStartsWithPrefix);
//    //        return aux;
//    //    }

//    //    public List<IMyTerminalBlock> GetCargoContainers()
//    //    {
//    //        var aux = new List<IMyTerminalBlock>();
//    //        GTS.GetBlocksOfType<IMyCargoContainer>(aux, NameStartsWithPrefix);
//    //        return aux;
//    //    }

//    //    public List<IMyTerminalBlock> GetAssemblers()
//    //    {
//    //        var aux = new List<IMyTerminalBlock>();
//    //        GTS.GetBlocksOfType<IMyAssembler>(aux, NameStartsWithPrefix);
//    //        return aux;
//    //    }

//    //    public List<IMyTerminalBlock> GetRefineries()
//    //    {
//    //        var aux = new List<IMyTerminalBlock>();
//    //        GTS.GetBlocksOfType<IMyRefinery>(aux, NameStartsWithPrefix);
//    //        return aux;
//    //    }
//    //}

//    //public class TerminalBlockHelper
//    //{
//    //    public static IMyTerminalBlock GetBlockByName(List<IMyTerminalBlock> blocks, string blockName)
//    //    {
//    //        foreach (var block in blocks)
//    //        {
//    //            if (block.CustomName == blockName)
//    //            {
//    //                return block;
//    //            }
//    //        }
//    //        return null;
//    //    }

//    //    public static void TurnOn(IMyTerminalBlock block)
//    //    {
//    //        var action = block.GetActionWithName("OnOff_On");
//    //        action.Apply(block);
//    //    }

//    //    public static void TurnOff(IMyTerminalBlock block)
//    //    {
//    //        var action = block.GetActionWithName("OnOff_Off");
//    //        action.Apply(block);
//    //    }
//    //}
//    //public static class LcdOutputHelper
//    //{
//    //    public static void ShowResult(IMyTextPanel lcd, string message)
//    //    {
//    //        ShowMessageOnLcd(lcd, new LcdMessage(message, Color.White));
//    //    }

//    //    public static void ShowResult(List<IMyTextPanel> lcds, string message)
//    //    {
//    //        if (lcds == null || lcds.Count == 0)
//    //            return;
//    //        var msg = new LcdMessage(message, Color.White);
//    //        foreach (var lcd in lcds)
//    //        {
//    //            ShowMessageOnLcd(lcd, msg);
//    //        }
//    //    }

//    //    public static void ShowResultWithProgress(List<IMyTextPanel> lcds, string message)
//    //    {
//    //        if (lcds == null || lcds.Count == 0)
//    //            return;

//    //        message = "=================================\n" + message + "\n  " + getTimmerChar();

//    //        var msg = new LcdMessage(message, Color.White);
//    //        foreach (var lcd in lcds)
//    //        {
//    //            ShowMessageOnLcd(lcd, msg);
//    //        }
//    //    }

//    //    // Timmer is used to show something different every iteration
//    //    static int timmer = 0;
//    //    private static string getTimmerChar()
//    //    {
//    //        // Move timmer
//    //        timmer++;

//    //        switch (timmer)
//    //        {
//    //            case 1: return "\\";
//    //            case 2: return "|";
//    //            case 3: return "/";
//    //            default: timmer = 0; return "-";
//    //        }
//    //    }

//    //    public static void ShowMessageOnLcd(IMyTextPanel lcd, LcdMessage message)
//    //    {
//    //        if (lcd == null) return;

//    //        lcd.WritePublicText(message.Text);
//    //        lcd.ShowPublicTextOnScreen();
//    //        lcd.SetValue<Color>("FontColor", message.FontColor);
//    //        lcd.SetValue<Color>("BackgroundColor", message.BackgroundColor);
//    //        lcd.SetValueFloat("FontSize", message.FontSize);
//    //    }

//    //    // Doesn't work
//    //    public static void ShowMessagesOnLcd(IMyTextPanel lcd, List<LcdMessage> messages)
//    //    {
//    //        if (lcd == null) return;

//    //        foreach (var message in messages)
//    //        {
//    //            lcd.SetValue<Color>("FontColor", message.FontColor);
//    //            lcd.SetValue<Color>("BackgroundColor", message.BackgroundColor);
//    //            lcd.SetValueFloat("FontSize", message.FontSize);
//    //            lcd.WritePublicText(message.Text, true);
//    //        }
//    //        lcd.ShowPublicTextOnScreen();
//    //    }

//    //    public struct LcdMessage
//    //    {
//    //        public string Text { get; set; }
//    //        public Color FontColor { get; set; }
//    //        public Color BackgroundColor { get; set; }
//    //        public float FontSize { get; set; }

//    //        public LcdMessage(string text)
//    //        {
//    //            Text = text;
//    //            FontColor = Color.White;
//    //            BackgroundColor = Color.Black;
//    //            FontSize = 1.1f;
//    //        }

//    //        public LcdMessage(string text, Color fontColor)
//    //        {
//    //            Text = text;
//    //            FontColor = fontColor;
//    //            BackgroundColor = Color.Black;
//    //            FontSize = 1.1f;
//    //        }
//    //    }
//    //}
//}