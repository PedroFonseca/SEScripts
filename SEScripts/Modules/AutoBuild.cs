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
using VRage.Game;

public class AutoBuild : Skeleton
{
    //======================================== INSTRUCTIONS =========================== 
    //  You need one trigger and one programable block to run this scripts. Setup instructions:
    //      a) This script should be inside a programable block.
    //      b) Place a Timer block component and set it up to run every second with the following 
    //          actions in this order:
    //              - Run Programmable block (default) 
    //              - Timer block (Start) 
    //      c) Execute Trigger Now action on the Timer block to start the script
    //=================================================================================

    //  Auto build components features:
    //  1) Automatically build components in assigned assemblers (up to a configured ammount)
    //======================================== INSTRUCTIONS =========================== 
    //  1. Set bellow the wanted quantities for each item
    //  2. Set the name "AutoBuilder" to the lcd's where you want to see results
    //=================================================================================

    // The name of the assembler(s) that the script uses to build the components (assemblers with name that starts with this string will be used)
    public static string AssemblerName = "AutoBuilder";
    // Name of the lcd to display the information's where you want to display build information (set text to public)
    public string LcdName = "Lcd AutoBuilder";
    // Insert here how much of each component you want to build
    public static Dictionary<string, int> ComponentDesiredQuantities = new Dictionary<string, int>
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


    public void Main(string argument)
    {
        // Execute script to automatically move components up to a certain cota to one connected container
        //var autoMoveResult = AutoMoveComponents.Run(GridTerminalSystem, AutoMoveComponentsToWelderConfig);

        ////Get the lcd(s) to show status and errors named debugger (if it doesn't exist we can't show errors/exceptions).
        //var debuggerLcds = new GetComponentsHelper(GridTerminalSystem, "auto move welder 1").GetLcdsPrefixed();
        //if (debuggerLcds.Count > 0)
        //{
        //    LcdOutputHelper.ShowResultWithProgress(debuggerLcds, autoMoveResult);
        //}

        // Get script assemblers 
        var assemblers = new GetComponentsHelper(GridTerminalSystem, AssemblerName).GetAssemblers();
        if (assemblers.Count == 0)
            throw new Exception(string.Format("Assembler with name starting with {0} not found.", AssemblerName));

        // Get the components that need's building
        var currentShortages = GetComponentsToBuild(GridTerminalSystem, assemblers);
        PrintComponentsToBuildOnLcd(LcdName, StringifyComponentsToBuild(currentShortages));

        //((IMyAssembler)assemblers[0]).AddQueueItem(ComponentHelper.GetBlueprintDefinition(ComponentHelper.Component.MetalGrid), (decimal)1);
    }

    public static Dictionary<string, int> GetComponentsToBuild(IMyGridTerminalSystem GridTerminalSystem, List<IMyTerminalBlock> assemblers)
    {
        // Get items in assembler inventories
        var itemsInAssemblerInventory = CargoHelper.GetItemsInInventories(assemblers, 1);

        var result = new Dictionary<string, int>();
        foreach (var desired in ComponentDesiredQuantities)
        {
            if (desired.Value <= 0)
                continue;
            var currentQuantity = itemsInAssemblerInventory.ContainsKey(desired.Key) ? itemsInAssemblerInventory[desired.Key].Quantity : 0;
            if (currentQuantity >= desired.Value)
                continue;
            result.Add(desired.Key, desired.Value - currentQuantity);
        }
        return result;
    }

    public string StringifyComponentsToBuild(Dictionary<string, int> components)
    {
        var result = string.Empty;
        foreach (var comp in components)
        {
            result += comp.Key + " - " + comp.Value + "\n";
        }
        return result;
    }

    public void PrintComponentsToBuildOnLcd(string lcdName, string results)
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
            var items = inventory.GetItems();
            var itemsDic = new Dictionary<string, ItemContentAdvanced>();
            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];
                var name = item.Content.SubtypeName;
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

    public static class ComponentHelper
    {
        public enum Component
        {
            ComputerComponent, ConstructionComponent, DetectorComponent,
            Display, GirderComponent, GravityGeneratorComponent, MetalGrid, MedicalComponent,
            LargeTube, InteriorPlate, Missile200mm, MotorComponent, NATO_5p56x45mmMagazine,
            SmallTube, RadioCommunicationComponent, PowerCell, ThrustComponent
        };

        public static MyDefinitionId GetBlueprintDefinition(Component comp)
        {
            var id = "MyObjectBuilder_BlueprintDefinition/" + comp.ToString();
            MyDefinitionId result;
            if (!MyDefinitionId.TryParse(id, out result))
                throw new Exception("Unable to parse blueprint id " + id);
            return result;
        }
    }
}