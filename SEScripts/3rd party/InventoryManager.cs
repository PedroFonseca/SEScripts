using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game;
using VRage.Game.ModAPI.Ingame;

namespace SEScripts._3rd_party
{
    public class InventoryManager : Skeleton
    {
        // Isy's Inventory Manager
        // ===================
        // Version: 2.4.0
        // Date: 2019-03-17

        //  =======================================================================================
        //                                                                            --- Configuration ---
        //  =======================================================================================

        // --- Sorting ---
        // =======================================================================================

        // Define the keyword a cargo container has to contain in order to be recognized as a container of the given type.
        const string oreContainerKeyword = "Ores";
        const string ingotContainerKeyword = "Ingots";
        const string componentContainerKeyword = "Components";
        const string toolContainerKeyword = "Tools";
        const string ammoContainerKeyword = "Ammo";
        const string bottleContainerKeyword = "Bottles";

        // Keyword an inventory has to contain to be skipped by the sorting (= no items will be taken out)
        const string lockedContainerKeyword = "Locked";

        // Keyword for connectors to disable sorting of a grid, that is docked to that connector.
        // This also disables the usage of refineries, arc furnaces and assemblers on that grid.
        // Special containers, reactors and O2/H2 generators will still be filled.
        string noSortingKeyword = "[No Sorting]";

        // Balance items between containers of the same type? This will result in an equal amount of all items in all containers of that type.
        bool balanceTypeContainers = false;

        // Show a fill level in the container's name?
        bool showFillLevel = true;

        // Auto assign new containers if a type is full or not present?
        bool autoAssignContainers = true;

        // Auto assign tool, ammo and bottle containers as one?
        bool toolsAmmoBottlesInOne = true;

        // Fill bottles before storing them in the bottle container?
        bool fillBottles = true;


        // --- Autocrafting ---
        // =======================================================================================

        // Enable autocrafting or autodisassembling (disassembling will disassemble everything above the wanted amounts)
        // All assemblers will be used. To use one manually, add the manualMachineKeyword to it (by default: "!manual")
        bool enableAutocrafting = true;
        bool enableAutodisassembling = false;

        // A LCD with the keyword "Autocrafting" is required where you can set the wanted amount!
        // This has multi LCD support. Just append numbers after the keyword, like: "LCD Autocrafting 1", "LCD Autocrafting 2", ..
        string autocraftingKeyword = "Autocrafting";

        // Add the header to every screen when using multiple autocrafting LCDs?
        bool headerOnEveryScreen = false;

        // Margins for assembling or disassembling items in percent based on the wanted amount (default: 0 = exact value).
        // Examples:
        // assembleMargin = 5 with a wanted amount of 100 items will only produce new items, if less than 95 are available.
        // disassembleMargin = 10 with a wanted amount of 1000 items will only disassemble items if more than 1100 are available.
        double assembleMargin = 0;
        double disassembleMargin = 0;

        // To hide certain items from the LCD, simply set their wanted amount to a negative value (e.g.: -1 or -500). These items will be moved
        // to the custom data of the first autocrafting LCD. To let them reappear on the LCD again, remove the entry from the custom data.

        // Sort the assembler queue based on the most needed components?
        bool sortAssemblerQueue = true;

        // Autocraft ingots in survival kits if stone is present?
        bool enableIngotCrafting = true;


        // --- Special Loadout Containers ---
        // =======================================================================================

        // Keyword an inventory has to contain to be filled with a special loadout (see in it's custom data after you renamed it!)
        // Special containers will be filled with your wanted amount of items and never be drained by the auto sorting!
        const string specialContainerKeyword = "Special";

        // Are special containers allowed to 'steal' items from other special containers with a lower priority?
        bool allowSpecialSteal = true;


        // --- Ore Balancing ---
        // =======================================================================================

        // By enabling ore balancing, the script will manage the filling with ores and the queue order of all refinieres.
        // To still use a refinery manually, add the manualMachineKeyword to it (by default: "!manual")
        bool enableOreBalancing = true;

        // TODO: helptext!

        // Value ore priority
        // This will move iron, nickel and cobalt ores to basic refineries if there are more valuable ores, only large refineries can process.
        // If the valuable ore is processed, the other ores are spread to all refineries again.
        bool enableValueOrePriority = true;

        // Process iron, nickel and cobalt ONLY in basic refineries and NOT in large refineries? (this overrides value ore priority mode!)
        bool enableBasicOreSpecialization = false;

        // If a large refinery contains this keyword, it acts like an basic refinery, is used for the two modes above and will only
        // process iron, nickel and coblat.
        string actBasicKeyword = "!basic";

        // Process stone in refineries?
        bool enableStoneProcessing = true;

        // Sort the refinery queue based on the most needed ingots?
        bool sortRefiningQueue = true;


        // --- Ice Balancing ---
        // =======================================================================================

        // Enable balancing of ice in O2/H2 generators?
        // All O2/H2 generators will be used. To use one manually, add the manualMachineKeyword to it (by default: "!manual")
        bool enableIceBalancing = true;

        // Put ice into O2/H2 generators that are turned off? (default: false)
        bool fillOfflineGenerators = false;

        // Ice fill level in percent in order to be able to fill bottles? (default: 95)
        // Note: O2/H2 generators will pull more ice automatically if value is below 60%
        double iceFillLevelPercentage = 95;


        // --- Uranium Balancing ---
        // =======================================================================================

        // Enable balancing of uranium in reactors? (Note: conveyors of reactors are turned off to stop them from pulling more)
        // All reactors will be used. To use one manually, add the manualMachineKeyword to it (by default: "!manual")
        bool enableUraniumBalancing = true;

        // Put uranium into reactors that are turned off? (default: false)
        bool fillOfflineReactors = false;

        // Amount of uranium in each reactor? (default: 100 for large grid reactors, 25 for small grid reactors)
        double uraniumAmountLargeGrid = 100;
        double uraniumAmountSmallGrid = 25;


        // --- Assembler Cleanup ---
        // =======================================================================================

        // This cleans up assemblers, if their inventory is too full and puts the contents back into a cargo container.
        bool enableAssemblerCleanup = true;

        // Set fill level in percent when the assembler should be cleaned up
        double assemblerFillLevelPercentage = 50;


        // --- Internal item sorting ---
        // =======================================================================================

        // Sort the items inside all containers?
        // Note, that this could cause inventory desync issues in multiplayer, so that items are invisible
        // or can't be taken out. Use at your own risk!
        bool enableInternalSorting = true;

        // Internal sorting pattern. Always combine one of each category, e.g.: 'Ad' for descending item amount (from highest to lowest)
        // 1. Quantifier:
        // A = amount
        // N = name
        // T = type (alphabetical)
        // X = type (number of items)

        // 2. Direction:
        // a = ascending
        // d = descending

        string sortingPattern = "Na";

        // Internal sorting can also be set per inventory. Just use '(sort:PATTERN)' in the block's name.
        // Example: Small Cargo Container 3 (sort:Ad)
        // Note: Using this method, internal sorting will always be activated for this container, even if the main switch is turned off!


        // --- LCD panels ---
        // =======================================================================================

        // To display the main script informations, add the following keyword to any LCD name (default: !IIM).
        // You can enable or disable specific informations on the LCD by editing its custom data.
        string mainLCDKeyword = "!IIM";

        // To display current item amounts of different types, add the following keyword to any LCD name
        // and follow the on screen instructions.
        string inventoryLCDKeyword = "!inventory";

        // To display all current warnings and problems, add the following keyword to any LCD name (default: !warnings).
        string warningsLCDKeyword = "!warnings";

        // To display the script performance (PB terminal output), add the following keyword to any LCD name (default: !performance).
        string performanceLCDKeyword = "!performance";


        // --- Settings for enthusiasts ---
        // =======================================================================================

        // Script cycle time in seconds (default: 8).
        double scriptExecutionTime = 8;

        // Script mode: "ship", "station" or blank for autodetect
        string scriptMode = "";

        // Protect type containers when docking to another grid running the script?
        bool protectTypeContainers = true;

        // If you want to use a machine manually, append the keyword to it.
        // This works for assemblers, refineries, reactors and O2/H2 generators
        string manualMachineKeyword = "!manual";

        // Ore yield dictionary for sorting the refining queue
        // This dictionary is directly gotten from the game files and means 1 ore of a kind results in so many ingots after refining.
        Dictionary<string, double> oreYieldDict = new Dictionary<string, double>()
{
    { "Cobalt", 0.3 },
    { "Gold", 0.01 },
    { "Iron", 0.7 },
    { "Magnesium", 0.007 },
    { "Nickel", 0.4 },
    { "Platinum", 0.005 },
    { "Silicon", 0.7 },
    { "Silver", 0.1 },
    { "Stone", 0.014 },
    { "Uranium", 0.01 }
};

        //  =======================================================================================
        //                                                                      --- End of Configuration ---
        //                                                        Don't change anything beyond this point!
        //  =======================================================================================


        List<IMyTerminalBlock> Ǿ = new List<IMyTerminalBlock>();
        List<IMyTerminalBlock> ǿ = new List<IMyTerminalBlock>();
        List<IMyTerminalBlock> Ȁ = new List<IMyTerminalBlock>();
        List<IMyTerminalBlock> ȁ = new List<IMyTerminalBlock>();
        List<IMyShipConnector> Ȃ = new List<IMyShipConnector>();
        List<IMyRefinery> ȃ = new List<IMyRefinery>();
        List<IMyRefinery> Ȅ = new List<IMyRefinery>();
        List<IMyTerminalBlock> ȅ = new List<IMyTerminalBlock>();
        List<IMyTerminalBlock> Ȇ = new List<IMyTerminalBlock>();
        List<IMyAssembler> ȇ = new List<IMyAssembler>();
        List<IMyAssembler> Ȉ = new List<IMyAssembler>();
        List<IMyAssembler> ȉ = new List<IMyAssembler>();
        List<IMyGasGenerator> ȋ = new List<IMyGasGenerator>();
        List<IMyReactor> Ȍ = new List<IMyReactor>();
        List<IMyTextPanel> Ɋ = new List<IMyTextPanel>();
        List<string> ȭ = new List<string>();
        HashSet<IMyCubeGrid> Ȯ = new HashSet<IMyCubeGrid>();
        List<IMyTerminalBlock> ȯ = new List<IMyTerminalBlock>();
        List<IMyTerminalBlock> Ȱ = new List<IMyTerminalBlock>();
        List<IMyTerminalBlock> ȱ = new List<IMyTerminalBlock>();
        List<IMyTerminalBlock> Ȳ = new List<IMyTerminalBlock>();
        List<IMyTerminalBlock> ȳ = new List<IMyTerminalBlock>();
        List<IMyTerminalBlock> ȴ = new List<IMyTerminalBlock>();
        List<IMyTerminalBlock> ȵ = new List<IMyTerminalBlock>();
        string[] ȶ ={oreContainerKeyword,ingotContainerKeyword,componentContainerKeyword,toolContainerKeyword,ammoContainerKeyword,bottleContainerKeyword};
        string ȷ = specialContainerKeyword.ToLower();
        string ȸ = lockedContainerKeyword.ToLower();
        string ȹ = "";
        bool Ⱥ = false;
        IMyTerminalBlock Ȼ;
        int ȼ = 0;
        int Ƚ = 0;
        int Ⱦ = 0;
        int ȿ = 0;
        int ɀ = 0;
        string Ɂ = "";
        string[] ɂ ={"/","-","\\","|"};
        int Ƀ = 0;
        List<IMyTextPanel> Ʉ = new List<IMyTextPanel>();
        string[] Ʌ ={"showWarnings=true","showContainerStats=true","showManagedBlocks=true","showLastAction=true"};
        string Ģ;
        HashSet<string> Ɇ = new HashSet<string>(); HashSet<string> ɇ = new HashSet<string>(); List<IMyTextPanel> Ɉ = new List<IMyTextPanel>(); int ɉ = 0; int ɋ = 0; int Ȭ = 0; int ȍ = 1; bool ț = true; bool Ȏ = true; bool ȏ = true;
        string Ȑ = "itemID;blueprintID";
        Dictionary<string, string> ȑ = new Dictionary<string, string>(){{"oreContainer", oreContainerKeyword},{"ingotContainer",ingotContainerKeyword},{"componentContainer",componentContainerKeyword},{"toolContainer",
toolContainerKeyword},{"ammoContainer",ammoContainerKeyword},{"bottleContainer",bottleContainerKeyword},{"lockedContainer",
lockedContainerKeyword},{"specialContainer",specialContainerKeyword},{"oreBalancing","true"},{"iceBalancing","true"},{"uraniumBalancing",
"true"}}; string Ȓ = "Isy's Autocrafting"; string ȓ = "Remove a line to show this item on the LCD again!"; char[] Ȕ = { '=', '>', '<' };
        IMyAssembler ȕ; MyDefinitionId Ȗ; MyDefinitionId ȗ; List<String> Ș = new List<string>{"Uranium","Silicon","Silver","Gold","Platinum",
"Magnesium","Iron","Nickel","Cobalt","Scrap"}; List<String> ș = new List<string> { "Uranium", "Silver", "Gold", "Platinum", }; List<String> Ț =
           new List<string> { "Iron", "Nickel", "Cobalt", "Magnesium", "Silicon" }; const string Ȝ = "MyObjectBuilder_"; const string Ȫ = "Ore";
        const string ȝ = "Ingot"; const string Ȟ = "Component"; const string ȟ = "AmmoMagazine"; const string Ƞ = "OxygenContainerObject"; const
                    string ȡ = "GasContainerObject"; const string Ȣ = "PhysicalGunObject"; const string ȣ = Ȝ + "BlueprintDefinition/"; SortedSet<
                               MyDefinitionId> Ȥ = new SortedSet<MyDefinitionId>(new Ŷ()); SortedSet<string> ȥ = new SortedSet<string>(); SortedSet<string> Ȧ = new SortedSet<
                                       string>(); SortedSet<string> ȧ = new SortedSet<string>(); SortedSet<string> Ȩ = new SortedSet<string>(); SortedSet<string> ȩ = new
                                                SortedSet<string>(); SortedSet<string> ȫ = new SortedSet<string>(); SortedSet<string> Ȋ = new SortedSet<string>(); Dictionary<
                                                         MyDefinitionId, double> ǽ = new Dictionary<MyDefinitionId, double>(); Dictionary<MyDefinitionId, double> Ǆ = new Dictionary<MyDefinitionId,
                                                               double>(); Dictionary<MyDefinitionId, MyDefinitionId> Ǉ = new Dictionary<MyDefinitionId, MyDefinitionId>(); Dictionary<MyDefinitionId
                                                                      , MyDefinitionId> ǈ = new Dictionary<MyDefinitionId, MyDefinitionId>(); Dictionary<string, MyDefinitionId> ǉ = new Dictionary<
                                                                            string, MyDefinitionId>(); Dictionary<string, string> Ǌ = new Dictionary<string, string>(); bool ǋ = false; string ǌ = "station_mode;\n";
        string Ǎ = "ship_mode;\n"; string ǎ = "[PROTECTED] "; string Ǐ = ""; string ǐ = ""; string Ǒ = ""; int ǒ = 3; double Ǔ = 0; List<IMyTextPanel> ǔ = new
                             List<IMyTextPanel>(); string[] Ǖ ={"","Find new items","Create item lists","Name correction","Assign containers",
"Fill special containers","Sort items","Container balancing","Internal sorting","Add fill level to names","Get global item amount",
"Get assembler queue","Autocrafting","Sort assembler queue","Clean up assemblers","Learn unknown blueprints","Ore balancing","Ice balancing",
"Uranium balancing"};
        void Program()
        {
            Echo("Script ready to be launched..\n");
            assembleMargin /= 100;
            disassembleMargin /= 100;
            ɉ = (int)Math.Floor(scriptExecutionTime / 8.9);
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
        }
        void Main(string ǖ)
        {
            try
            {
                Ŀ("", true); if (ț)
                {
                    if (DateTime.Now.Second
% 2 == 0) { Echo("Initializing script.\n"); }
                    else { Echo("Initializing script..\n"); }
                    Echo("Getting base grid.."); Ɓ(Me.CubeGrid);
                    Echo("Getting inventory blocks.."); ǃ(); Echo("Loading saved items.."); if (!ơ())
                    {
                        Echo("-> No assembler found!"); Echo(
"-> Can't check saved blueprints.."); Echo("Restarting.."); return;
                    }
                    Echo("Checking blueprints.."); foreach (var Ö in Ȥ) { Ɠ(Ö); }
                    Echo("Checking type containers.."
); Ǳ(); Echo("Setting script mode.."); if (scriptMode == "station") { ǋ = true; } else if (ſ.IsStatic && scriptMode != "ship") { ǋ = true; }
                    Me.
CustomData = (ǋ ? ǌ : Ǎ) + Me.CustomData.Replace(ǌ, "").Replace(Ǎ, ""); ț = false; Runtime.UpdateFrequency = UpdateFrequency.Update10; return;
                }
                if (ǖ
!= "") { Ǒ = ǖ.ToLower(); ȍ = 1; ǐ = ""; ǒ = 3; }
                if (ɋ < ɉ) { ɋ++; return; }
                var Ǘ = new List<IMyTerminalBlock>(); GridTerminalSystem.GetBlocks(Ǘ);
                int ǘ = Ǘ.Count; if (ȍ == 1 || ǘ != Ȭ) { ǃ(); Ȭ = ǘ; }
                if (ȏ) { Ĥ(); ȏ = false; return; }
                if (Ȏ) { Ļ(); Ġ(); ģ(); Ȏ = false; return; }
                ɋ = 0; ȏ = true; Ȏ = true; if (!ǋ) ƽ
(); if (ǚ(Ǒ)) return; if (ȍ == 1) { Ɵ(); }
                if (ȍ == 2) { ƞ(); }
                if (ȍ == 3) { ǭ(); }
                if (ȍ == 4) { if (autoAssignContainers) Ǵ(); }
                if (ȍ == 5)
                {
                    if (ȵ.Count != 0)
                        ɼ();
                }
                if (ȍ == 6)
                {
                    Ǣ(Ȫ, ȯ, oreContainerKeyword); Ǣ(ȝ, Ȱ, ingotContainerKeyword); Ǣ(Ȟ, ȱ, componentContainerKeyword); Ǣ(Ȣ, Ȳ,
               toolContainerKeyword); Ǣ(ȟ, ȳ, ammoContainerKeyword); Ǣ(Ƞ, ȴ, bottleContainerKeyword); Ǣ(ȡ, ȴ, bottleContainerKeyword);
                }
                if (ȍ == 7)
                {
                    if (
balanceTypeContainers) ʢ();
                }
                if (ȍ == 8) { ʇ(); }
                if (ȍ == 9) { ʀ(ȁ); ʀ(ȵ); }
                if (ȍ == 10) { Ʋ(); }
                if (ȍ == 11) { if (enableAutocrafting || enableAutodisassembling) ƻ(); }
                if (
ȍ == 12) { if (enableAutocrafting || enableAutodisassembling) ʒ(); }
                if (ȍ == 13) { if (sortAssemblerQueue) ɸ(); }
                if (ȍ == 14)
                {
                    if (
enableAssemblerCleanup) ɵ(assemblerFillLevelPercentage); if (enableIngotCrafting) ʜ();
                }
                if (ȍ == 15) { Ɯ(); }
                if (ȍ == 16)
                {
                    if (enableOreBalancing)
                    {
                        Î(
"oreBalancing", "true"); ɣ();
                    }
                    else if (!enableOreBalancing && Ë("oreBalancing") == "true")
                    {
                        Î("oreBalancing", "false"); foreach (IMyRefinery Ǚ in
ȃ) { Ǚ.UseConveyorSystem = true; }
                    }
                }
                if (ȍ == 17)
                {
                    if (enableIceBalancing) { Î("iceBalancing", "true"); Ē(); }
                    else if (!enableIceBalancing
&& Ë("iceBalancing") == "true") { Î("iceBalancing", "false"); foreach (IMyGasGenerator ė in ȋ) { ė.UseConveyorSystem = true; } }
                }
                if (ȍ ==
18)
                {
                    if (enableUraniumBalancing) { Î("uraniumBalancing", "true"); ù(); }
                    else if (!enableUraniumBalancing && Ë("uraniumBalancing") ==
"true") { Î("uraniumBalancing", "false"); foreach (IMyReactor þ in Ȍ) { þ.UseConveyorSystem = true; } }
                }
                Ŀ(Ǖ[ȍ]); H(); if (ȍ >= 18)
                {
                    ȍ = 1; Ɇ = new
HashSet<string>(ɇ); ɇ.Clear(); if (Ɇ.Count == 0) Ģ = null;
                }
                else { ȍ++; }
                Ƀ = Ƀ >= 3 ? 0 : Ƀ + 1;
            }
            catch (Exception e)
            {
                string D = e + " \n\n"; D +=
"The error occured while executing the following script step:\n" + Ǖ[ȍ] + " (ID: " + ȍ + ")"; Ļ(D); throw new Exception(D);
            }
        }
        bool ǚ(string ǖ)
        {
            if (ǖ.Contains("pauseThisPB"))
            {
                Echo(
"Script execution paused!\n"); var Ǜ = ǖ.Split(';'); if (Ǜ.Length == 3)
                {
                    Echo("Found:"); Echo("'" + Ǜ[1] + "'"); Echo("on grid:"); Echo("'" + Ǜ[2] + "'"); Echo(
"also running the script.\n"); Echo("Type container protection: " + (protectTypeContainers ? "ON" : "OFF") + "\n"); Echo(
"Everything else is managed by the other script.");
                }
                return true;
            }
            bool ǜ = true; bool ǝ = true; bool ǅ = false; if (ǖ != "reset" && ǖ != "msg")
            {
                if (!ǖ.Contains(" on") && !ǖ.Contains(" off")
&& !ǖ.Contains(" toggle")) return false; if (ǖ.Contains(" off")) ǝ = false; if (ǖ.Contains(" toggle")) ǅ = true;
            }
            if (ǖ == "reset")
            {
                ư();
                return true;
            }
            else if (ǖ == "msg") { }
            else if (ǖ.Contains("assigncontainers"))
            {
                Ǐ = "Auto container assigment"; if (ǅ) ǝ = !
autoAssignContainers; autoAssignContainers = ǝ;
            }
            else if (ǖ.Contains("fillbottles")) { Ǐ = "Bottle filling"; if (ǅ) ǝ = !fillBottles; fillBottles = ǝ; }
            else
if (ǖ.Contains("autocrafting")) { Ǐ = "Autocrafting"; if (ǅ) ǝ = !enableAutocrafting; enableAutocrafting = ǝ; }
            else if (ǖ.Contains(
"autodisassembling")) { Ǐ = "Auto disassembling"; if (ǅ) ǝ = !enableAutodisassembling; enableAutodisassembling = ǝ; }
            else if (ǖ.Contains(
"sortassemblerqueue")) { Ǐ = "Assembler queue sorting"; if (ǅ) ǝ = !sortAssemblerQueue; sortAssemblerQueue = ǝ; }
            else if (ǖ.Contains("assemblercleanup"))
            {
                Ǐ = "Assembler cleanup"; if (ǅ) ǝ = !enableAssemblerCleanup; enableAssemblerCleanup = ǝ;
            }
            else if (ǖ.Contains("orebalancing"))
            {
                Ǐ =
"Ore balancing"; if (ǅ) ǝ = !enableOreBalancing; enableOreBalancing = ǝ;
            }
            else if (ǖ.Contains("arcpriority"))
            {
                Ǐ = "Arc priority mode"; if (ǅ) ǝ = !
enableValueOrePriority; enableValueOrePriority = ǝ;
            }
            else if (ǖ.Contains("arcspecialization"))
            {
                Ǐ = "Arc specialization mode"; if (ǅ) ǝ = !
enableBasicOreSpecialization; enableBasicOreSpecialization = ǝ;
            }
            else if (ǖ.Contains("stoneprocessing"))
            {
                Ǐ = "Stone processing"; if (ǅ) ǝ = !
enableStoneProcessing; enableStoneProcessing = ǝ;
            }
            else if (ǖ.Contains("sortrefiningqueue"))
            {
                Ǐ = "Refining queue sorting"; if (ǅ) ǝ = !sortRefiningQueue;
                sortRefiningQueue = ǝ;
            }
            else if (ǖ.Contains("icebalancing")) { Ǐ = "Ice balancing"; if (ǅ) ǝ = !enableIceBalancing; enableIceBalancing = ǝ; }
            else if (ǖ.
Contains("uraniumbalancing")) { Ǐ = "Uranium balancing"; if (ǅ) ǝ = !enableUraniumBalancing; enableUraniumBalancing = ǝ; }
            else if (ǖ.Contains(
"internalsorting")) { Ǐ = "Internal sorting"; if (ǅ) ǝ = !enableInternalSorting; enableInternalSorting = ǝ; }
            else if (ǖ.Contains("containerbalancing"))
            { Ǐ = "Container balancing"; if (ǅ) ǝ = !balanceTypeContainers; balanceTypeContainers = ǝ; }
            else { ǜ = false; }
            if (ǜ)
            {
                if (ǐ == "") ǐ = Ǐ +
" temporarily " + (ǝ ? "enabled" : "disabled") + "!\n"; Echo(ǐ); Echo("Continuing in " + ǒ + " seconds.."); Ǒ = "msg"; if (ǒ == 0)
                {
                    scriptExecutionTime = Ǔ; Ǔ = 0
; Ǐ = ""; ǐ = ""; Ǒ = ""; ǒ = 3;
                }
                else { if (Ǔ == 0) { Ǔ = scriptExecutionTime; scriptExecutionTime = 1000; } ǒ -= 1; }
            }
            return ǜ;
        }
        void ƽ()
        {
            List<
IMyProgrammableBlock> ǁ = new List<IMyProgrammableBlock>(); GridTerminalSystem.GetBlocksOfType(ǁ, Ɔ => Ɔ != Me); if (Ǒ == "pauseThisPB" || Ǒ == "")
            {
                Ǒ = "";
                foreach (var ƾ in ǁ)
                {
                    if (ƾ.CustomData.Contains(ǌ) || (ƾ.CustomData.Contains(Ǎ) && Í(ƾ) < Í(Me)))
                    {
                        Ǒ = "pauseThisPB;" + ƾ.CustomName + ";" + ƾ.
CubeGrid.CustomName; foreach (var Ò in ȁ) { if (protectTypeContainers && !Ò.CustomName.Contains(ǎ)) Ò.CustomName = ǎ + Ò.CustomName; }
                        return;
                    }
                }
                if (Ǒ == "") { foreach (var Ò in ȁ) { Ò.CustomName = Ò.CustomName.Replace(ǎ, ""); } }
            }
        }
        void ƿ(IMyCubeGrid Ƃ, bool Ƌ = false)
        {
            Ȯ.Add(Ƃ);
            List<IMyShipConnector> ǀ = new List<IMyShipConnector>(); List<IMyMotorStator> ƃ = new List<IMyMotorStator>(); List<IMyPistonBase> Ƅ =
                    new List<IMyPistonBase>(); GridTerminalSystem.GetBlocksOfType<IMyShipConnector>(ǀ, ê => ê.CubeGrid == Ƃ && ê.Status ==
                      MyShipConnectorStatus.Connected); GridTerminalSystem.GetBlocksOfType<IMyMotorStator>(ƃ, ƅ => ƅ.CubeGrid == Ƃ && ƅ.IsAttached); GridTerminalSystem.
                               GetBlocksOfType<IMyPistonBase>(Ƅ, Ɔ => Ɔ.CubeGrid == Ƃ && Ɔ.IsAttached); foreach (var ǂ in ǀ)
            {
                if (!Ȯ.Contains(ǂ.OtherConnector.CubeGrid))
                {
                    ƿ(ǂ.
OtherConnector.CubeGrid);
                }
            }
            foreach (var Ƈ in ƃ) { if (!Ȯ.Contains(Ƈ.TopGrid)) { ƿ(Ƈ.TopGrid); } }
            foreach (var ƈ in Ƅ)
            {
                if (!Ȯ.Contains(ƈ.TopGrid)
) { ƿ(ƈ.TopGrid); }
            }
        }
        void ǃ()
        {
            Ɗ(ſ, true); GridTerminalSystem.SearchBlocksOfName(oreContainerKeyword, ȯ, ê => ê.HasInventory && Ɖ.
Contains(ê.CubeGrid)); GridTerminalSystem.SearchBlocksOfName(ingotContainerKeyword, Ȱ, ê => ê.HasInventory && Ɖ.Contains(ê.CubeGrid));
            GridTerminalSystem.SearchBlocksOfName(componentContainerKeyword, ȱ, ê => ê.HasInventory && Ɖ.Contains(ê.CubeGrid)); GridTerminalSystem.
                   SearchBlocksOfName(toolContainerKeyword, Ȳ, ê => ê.HasInventory && Ɖ.Contains(ê.CubeGrid)); GridTerminalSystem.SearchBlocksOfName(
                          ammoContainerKeyword, ȳ, ê => ê.HasInventory && Ɖ.Contains(ê.CubeGrid)); GridTerminalSystem.SearchBlocksOfName(bottleContainerKeyword, ȴ, ê => ê.
                                   HasInventory && Ɖ.Contains(ê.CubeGrid)); foreach (var Ò in ȵ) { if (!Ò.CustomName.ToLower().Contains(ȷ)) Ò.CustomData = ""; }
            GridTerminalSystem
.SearchBlocksOfName(specialContainerKeyword, ȵ, ê => ê.HasInventory); ǯ(ȯ); ǯ(Ȱ); ǯ(ȱ); ǯ(Ȳ); ǯ(ȳ); ǯ(ȴ); ǯ(ȵ); ȁ.Clear(); ȁ.AddRange(
ȯ); ȁ.AddRange(Ȱ); ȁ.AddRange(ȱ); ȁ.AddRange(Ȳ); ȁ.AddRange(ȳ); ȁ.AddRange(ȴ); Ȯ.Clear(); GridTerminalSystem.GetBlocksOfType<
IMyShipConnector>(Ȃ, ê => Ɖ.Contains(ê.CubeGrid) && ê.CustomName.Contains(noSortingKeyword)); if (Ȃ.Count > 0)
            {
                foreach (var ǂ in Ȃ)
                {
                    if (ǂ.Status !=
MyShipConnectorStatus.Connected) continue; Ȯ.Add(ǂ.CubeGrid); ƿ(ǂ.OtherConnector.CubeGrid); Ȯ.Remove(ǂ.CubeGrid);
                }
            }
            GridTerminalSystem.
GetBlocksOfType<IMyRefinery>(ȃ, ƅ => !Ȯ.Contains(ƅ.CubeGrid) && !ƅ.CustomName.ToLower().Contains(ȷ) && !ƅ.CustomName.Contains(
manualMachineKeyword) && ƅ.IsWorking); GridTerminalSystem.GetBlocksOfType<IMyRefinery>(Ȅ, ƅ => !Ȯ.Contains(ƅ.CubeGrid) && !ƅ.CustomName.ToLower().
Contains(ȸ)); ȅ.Clear(); Ȇ.Clear(); foreach (IMyRefinery Ǚ in ȃ)
            {
                Ǚ.UseConveyorSystem = false; if (Ǚ.BlockDefinition.SubtypeId ==
"Blast Furnace" || Ǚ.CustomName.ToLower().Contains(actBasicKeyword.ToLower())) { Ȇ.Add(Ǚ); }
                else { ȅ.Add(Ǚ); }
            }
            GridTerminalSystem.
GetBlocksOfType<IMyAssembler>(ȇ, Ĉ => !Ȯ.Contains(Ĉ.CubeGrid) && !Ĉ.CustomName.Contains(manualMachineKeyword) && Ĉ.IsWorking);
            GridTerminalSystem.GetBlocksOfType<IMyAssembler>(Ȉ, Ĉ => !Ȯ.Contains(Ĉ.CubeGrid) && !Ĉ.CustomName.ToLower().Contains(ȸ)); GridTerminalSystem.
                  GetBlocksOfType<IMyAssembler>(ȉ, Ǯ => !Ȯ.Contains(Ǯ.CubeGrid) && Ǯ.BlockDefinition.SubtypeId.Contains("Survival") && !Ǯ.CustomName.Contains(
                   manualMachineKeyword) && Ǯ.IsWorking); GridTerminalSystem.GetBlocksOfType<IMyGasGenerator>(ȋ, ê => !ê.CustomName.ToLower().Contains(ȷ) && !ê.
                       CustomName.ToLower().Contains(manualMachineKeyword) && ê.IsFunctional); if (!fillOfflineGenerators) { ȋ.RemoveAll(ê => !ê.Enabled); }
            GridTerminalSystem.GetBlocksOfType<IMyReactor>(Ȍ, ê => !ê.CustomName.ToLower().Contains(ȷ) && !ê.CustomName.ToLower().Contains(
             manualMachineKeyword) && ê.IsFunctional); if (!fillOfflineReactors) { Ȍ.RemoveAll(ê => !ê.Enabled); }
            GridTerminalSystem.GetBlocksOfType<
IMyTerminalBlock>(Ǿ, u => u.HasInventory && !Ȯ.Contains(u.CubeGrid) && !u.CustomName.Contains(ǎ) && !u.GetType().ToString().Contains("Weapons"));
            GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(Ȁ, u => u.HasInventory && !Ȯ.Contains(u.CubeGrid) && u.InventoryCount == 1 && !u.CustomName.
             ToLower().Contains(ȷ) && !u.CustomName.ToLower().Contains(ȸ) && !u.CustomName.Contains(ǎ) && !u.GetType().ToString().Contains(
             "Reactor") && !u.GetType().ToString().Contains("MyCockpit") && !u.GetType().ToString().Contains("Weapons")); if (fillBottles)
            {
                Ȁ.Sort((Ĉ
, í) => í.BlockDefinition.TypeIdString.Contains("Oxygen").CompareTo(Ĉ.BlockDefinition.TypeIdString.Contains("Oxygen")));
            }
        }
        void ǯ(List<IMyTerminalBlock> ǰ) { if (ǰ.Count >= 2) ǰ.Sort((Ĉ, í) => Í(Ĉ).CompareTo(Í(í))); }
        void Ǳ()
        {
            bool ǲ = false; if (
oreContainerKeyword != Ë("oreContainer")) { ǲ = true; }
            else if (ingotContainerKeyword != Ë("ingotContainer")) { ǲ = true; }
            else if (
componentContainerKeyword != Ë("componentContainer")) { ǲ = true; }
            else if (toolContainerKeyword != Ë("toolContainer")) { ǲ = true; }
            else if (
ammoContainerKeyword != Ë("ammoContainer")) { ǲ = true; }
            else if (bottleContainerKeyword != Ë("bottleContainer")) { ǲ = true; }
            else if (
lockedContainerKeyword != Ë("lockedContainer")) { ǲ = true; }
            else if (specialContainerKeyword != Ë("specialContainer")) { ǲ = true; }
            if (ǲ)
            {
                List<
IMyTerminalBlock> ǳ = new List<IMyTerminalBlock>(); GridTerminalSystem.GetBlocks(ǳ); foreach (var Ã in ǳ)
                {
                    if (Ã.CustomName.Contains(Ë(
"oreContainer"))) { Ã.CustomName = Ã.CustomName.Replace(Ë("oreContainer"), oreContainerKeyword); }
                    if (Ã.CustomName.Contains(Ë(
"ingotContainer"))) { Ã.CustomName = Ã.CustomName.Replace(Ë("ingotContainer"), ingotContainerKeyword); }
                    if (Ã.CustomName.Contains(Ë(
"componentContainer"))) { Ã.CustomName = Ã.CustomName.Replace(Ë("componentContainer"), componentContainerKeyword); }
                    if (Ã.CustomName.Contains(Ë(
"toolContainer"))) { Ã.CustomName = Ã.CustomName.Replace(Ë("toolContainer"), toolContainerKeyword); }
                    if (Ã.CustomName.Contains(Ë(
"ammoContainer"))) { Ã.CustomName = Ã.CustomName.Replace(Ë("ammoContainer"), ammoContainerKeyword); }
                    if (Ã.CustomName.Contains(Ë(
"bottleContainer"))) { Ã.CustomName = Ã.CustomName.Replace(Ë("bottleContainer"), bottleContainerKeyword); }
                    if (Ã.CustomName.Contains(Ë(
"lockedContainer"))) { Ã.CustomName = Ã.CustomName.Replace(Ë("lockedContainer"), lockedContainerKeyword); }
                    if (Ã.CustomName.Contains(Ë(
"specialContainer"))) { Ã.CustomName = Ã.CustomName.Replace(Ë("specialContainer"), specialContainerKeyword); }
                }
                Î("oreContainer",
oreContainerKeyword); Î("ingotContainer", ingotContainerKeyword); Î("componentContainer", componentContainerKeyword); Î("toolContainer",
toolContainerKeyword); Î("ammoContainer", ammoContainerKeyword); Î("bottleContainer", bottleContainerKeyword); Î("lockedContainer",
lockedContainerKeyword); Î("specialContainer", specialContainerKeyword);
            }
        }
        void Ǵ()
        {
            List<IMyCargoContainer> ǵ = new List<IMyCargoContainer>();
            GridTerminalSystem.GetBlocksOfType<IMyCargoContainer>(ǵ, ê => Ɖ.Contains(ê.CubeGrid) && !ê.CustomName.ToLower().Contains(ȷ) && !ê.CustomName.
             ToLower().Contains(ȸ)); IMyCargoContainer Ƕ = ǵ.Find(ê => ê.CustomName.Contains(componentContainerKeyword)); if (Ƕ != null)
            {
                var Ƿ = Ƕ.
GetInventory(); ǵ.RemoveAll(ê => !ê.GetInventory().IsConnectedTo(Ƿ));
            }
            foreach (var Ò in ǵ)
            {
                bool Ǹ = false; foreach (string ǹ in ȶ)
                {
                    if (Ò.
CustomName.Contains(ǹ)) Ǹ = true;
                }
                if (!Ǹ)
                {
                    bool Ǻ = false; string ǻ = Ò.CustomName; string Ǽ = ""; if (ȯ.Count == 0 || ȹ == "Ore")
                    {
                        Ò.CustomName = Ò.
CustomName + " " + oreContainerKeyword; ȯ.Add(Ò); Ǽ = "Ores";
                    }
                    else if (Ȱ.Count == 0 || ȹ == "Ingot")
                    {
                        Ò.CustomName = Ò.CustomName + " " +
ingotContainerKeyword; Ȱ.Add(Ò); Ǽ = "Ingots";
                    }
                    else if (ȱ.Count == 0 || ȹ == "Component")
                    {
                        Ò.CustomName = Ò.CustomName + " " + componentContainerKeyword; ȱ.Add(
Ò); Ǽ = "Components";
                    }
                    else if (Ȳ.Count == 0 || ȹ == "PhysicalGunObject")
                    {
                        if (toolsAmmoBottlesInOne) { Ǻ = true; }
                        else
                        {
                            Ò.CustomName = Ò.
CustomName + " " + toolContainerKeyword; Ȳ.Add(Ò); Ǽ = "Tools";
                        }
                    }
                    else if (ȳ.Count == 0 || ȹ == "AmmoMagazine")
                    {
                        if (toolsAmmoBottlesInOne) { Ǻ = true; }
                        else { Ò.CustomName = Ò.CustomName + " " + ammoContainerKeyword; ȳ.Add(Ò); Ǽ = "Ammo"; }
                    }
                    else if (ȴ.Count == 0 || ȹ == "OxygenContainerObject" ||
ȹ == "GasContainerObject")
                    {
                        if (toolsAmmoBottlesInOne) { Ǻ = true; }
                        else
                        {
                            Ò.CustomName = Ò.CustomName + " " + bottleContainerKeyword; ȴ.
Add(Ò); Ǽ = "Bottles";
                        }
                    }
                    if (Ǻ)
                    {
                        Ò.CustomName = Ò.CustomName + " " + toolContainerKeyword + " " + ammoContainerKeyword + " " +
bottleContainerKeyword; Ȳ.Add(Ò); ȳ.Add(Ò); ȴ.Add(Ò); Ǽ = "Tools, Ammo and Bottles";
                    }
                    if (Ǽ != "")
                    {
                        Ɂ = "Assigned '" + ǻ + "' as a new container for type '" + Ǽ +
"'.";
                    }
                    ȹ = "";
                }
            }
        }
        void ǭ()
        {
            foreach (var Ò in Ǿ)
            {
                string ǡ = Ò.CustomName; string Ǟ = ǡ.ToLower(); List<string> ǟ = new List<string>(); if (Ǟ.
Contains(oreContainerKeyword.ToLower()) && !ǡ.Contains(oreContainerKeyword)) ǟ.Add(oreContainerKeyword); if (Ǟ.Contains(
ingotContainerKeyword.ToLower()) && !ǡ.Contains(ingotContainerKeyword)) ǟ.Add(ingotContainerKeyword); if (Ǟ.Contains(componentContainerKeyword.
ToLower()) && !ǡ.Contains(componentContainerKeyword)) ǟ.Add(componentContainerKeyword); if (Ǟ.Contains(toolContainerKeyword.ToLower(
)) && !ǡ.Contains(toolContainerKeyword)) ǟ.Add(toolContainerKeyword); if (Ǟ.Contains(ammoContainerKeyword.ToLower()) && !ǡ.
Contains(ammoContainerKeyword)) ǟ.Add(ammoContainerKeyword); if (Ǟ.Contains(bottleContainerKeyword.ToLower()) && !ǡ.Contains(
 bottleContainerKeyword)) ǟ.Add(bottleContainerKeyword); if (Ǟ.Contains(lockedContainerKeyword.ToLower()) && !ǡ.Contains(lockedContainerKeyword)) ǟ.
       Add(lockedContainerKeyword); if (Ǟ.Contains(specialContainerKeyword.ToLower()) && !ǡ.Contains(specialContainerKeyword)) ǟ.Add(
            specialContainerKeyword); if (Ǟ.Contains(noSortingKeyword.ToLower()) && !ǡ.Contains(noSortingKeyword)) ǟ.Add(noSortingKeyword); if (Ǟ.Contains(
                  actBasicKeyword.ToLower()) && !ǡ.Contains(actBasicKeyword)) ǟ.Add(actBasicKeyword); if (Ǟ.Contains(manualMachineKeyword.ToLower()) && !ǡ.
                      Contains(manualMachineKeyword)) ǟ.Add(manualMachineKeyword); if (Ǟ.Contains(autocraftingKeyword.ToLower()) && !ǡ.Contains(
                        autocraftingKeyword)) ǟ.Add(autocraftingKeyword); if (Ǟ.Contains(inventoryLCDKeyword.ToLower()) && !ǡ.Contains(inventoryLCDKeyword)) ǟ.Add(
                              inventoryLCDKeyword); if (Ǟ.Contains(mainLCDKeyword.ToLower()) && !ǡ.Contains(mainLCDKeyword)) ǟ.Add(mainLCDKeyword); if (Ǟ.Contains(
                                    warningsLCDKeyword.ToLower()) && !ǡ.Contains(warningsLCDKeyword)) ǟ.Add(warningsLCDKeyword); if (Ǟ.Contains(performanceLCDKeyword.ToLower()) && !
                                        ǡ.Contains(performanceLCDKeyword)) ǟ.Add(performanceLCDKeyword); if (Ǟ.Contains(" p") && !ǡ.Contains(" P")) ǟ.Add(" P"); if (Ǟ.
                                               Contains(" pmax") && !ǡ.Contains(" PMax")) ǟ.Add(" PMax"); if (Ǟ.Contains(" pmin") && !ǡ.Contains(" PMin")) ǟ.Add(" PMin"); foreach (var F
                                                        in ǟ)
                {
                    Ò.CustomName = System.Text.RegularExpressions.Regex.Replace(Ò.CustomName, F, F, System.Text.RegularExpressions.
              RegexOptions.IgnoreCase); Ɂ = "Corrected name\nof: '" + ǡ + "'\nto: '" + Ò.CustomName + "'";
                }
            }
            List<IMyTextPanel> Ǡ = new List<IMyTextPanel>();
            GridTerminalSystem.GetBlocksOfType(Ǡ); foreach (var K in Ǡ)
            {
                string ǡ = K.CustomName; string Ǟ = ǡ.ToLower(); List<string> ǟ = new List<string>(); if (Ǟ
.Contains(mainLCDKeyword.ToLower()) && !ǡ.Contains(mainLCDKeyword)) ǟ.Add(mainLCDKeyword); if (Ǟ.Contains(inventoryLCDKeyword.
ToLower()) && !ǡ.Contains(inventoryLCDKeyword)) ǟ.Add(inventoryLCDKeyword); if (Ǟ.Contains(warningsLCDKeyword.ToLower()) && !ǡ.
Contains(warningsLCDKeyword)) ǟ.Add(warningsLCDKeyword); foreach (var F in ǟ)
                {
                    K.CustomName = System.Text.RegularExpressions.Regex.
Replace(K.CustomName, F, F, System.Text.RegularExpressions.RegexOptions.IgnoreCase); Ɂ = "Corrected name\nof: '" + ǡ + "'\nto: '" + K.
CustomName + "'";
                }
            }
        }
        void Ǣ(string ǣ, List<IMyTerminalBlock> Ǥ, string ǥ)
        {
            if (Ǥ.Count == 0)
            {
                Ƣ("There are no containers for type '" + ǥ +
"'!\nBuild new ones or add the tag to existing ones!"); ȹ = ǣ; return;
            }
            IMyTerminalBlock U = null; int ǫ = int.MaxValue; string Ǧ =
"\nhas a different owner/faction!\nCan't move items from there!"; foreach (var Ò in Ǥ)
            {
                if (ǣ == Ƞ && Ò.BlockDefinition.TypeIdString.Contains("OxygenTank") && Ò.BlockDefinition.SubtypeId.
Contains("Hydrogen")) { continue; }
                else if (ǣ == ȡ && Ò.BlockDefinition.TypeIdString.Contains("OxygenTank") && !Ò.BlockDefinition.
SubtypeId.Contains("Hydrogen")) { continue; }
                var Â = Ò.GetInventory(0); if ((float)Â.CurrentVolume < (float)Â.MaxVolume * 0.99)
                {
                    int ǧ = Í(Ò);
                    if (ǧ < ǫ) { ǫ = ǧ; U = Ò; }
                }
            }
            if (U == null)
            {
                Ƣ("All containers for type '" + ǥ + "' are full!\nYou should build new cargo containers!"); ȹ = ǣ;
                return;
            }
            IMyTerminalBlock Ǩ = null; if (fillBottles && (ǣ == Ƞ || ǣ == ȡ)) { Ǩ = ǩ(ǣ); }
            foreach (var Ò in Ȁ)
            {
                if (Ò == U || (Ò.CustomName.Contains(ǥ) &&
Í(Ò) <= ǫ) || (ǣ == "Ore" && Ò.GetType().ToString().Contains("MyGasGenerator"))) { continue; }
                if (Ò.CustomName.Contains(ǥ) &&
balanceTypeContainers && !Ò.BlockDefinition.TypeIdString.Contains("OxygenGenerator") && !Ò.BlockDefinition.TypeIdString.Contains("OxygenTank"))
                    continue; if (Ò.GetOwnerFactionTag() != Me.GetOwnerFactionTag()) { Ƣ("'" + Ò.CustomName + "'" + Ǧ); continue; }
                if (Ǩ != null)
                {
                    if (!Ò.
BlockDefinition.TypeIdString.Contains("Oxygen")) { Q(ǣ, Ò, 0, Ǩ, 0); continue; }
                }
                Q(ǣ, Ò, 0, U, 0);
            }
            foreach (var Ǚ in Ȅ)
            {
                if (Ǚ == U || (Ǚ.CustomName.
Contains(ǥ) && Í(Ǚ) <= ǫ)) { continue; }
                if (Ǚ.GetOwnerFactionTag() != Me.GetOwnerFactionTag()) { Ƣ("'" + Ǚ.CustomName + "'" + Ǧ); continue; }
                Q(ǣ, Ǚ, 1
, U, 0);
            }
            foreach (IMyAssembler ĉ in Ȉ)
            {
                if (ĉ.Mode == MyAssemblerMode.Disassembly || ĉ == U || (ĉ.CustomName.Contains(ǥ) && Í(ĉ) <= ǫ))
                {
                    continue;
                }
                if (ĉ.GetOwnerFactionTag() != Me.GetOwnerFactionTag()) { Ƣ("'" + ĉ.CustomName + "'" + Ǧ); continue; }
                if (Ǩ != null)
                {
                    Q(ǣ, ĉ, 1, Ǩ, 0);
                    continue;
                }
                Q(ǣ, ĉ, 1, U, 0);
            }
        }
        IMyTerminalBlock ǩ(string ǣ)
        {
            List<IMyGasTank> Ǫ = new List<IMyGasTank>(); GridTerminalSystem.
GetBlocksOfType<IMyGasTank>(Ǫ, ǆ => Ɖ.Contains(ǆ.CubeGrid) && !ǆ.CustomName.ToLower().Contains(ȷ) && !ǆ.CustomName.ToLower().Contains(ȸ) && ǆ.
IsWorking); if (ǣ == Ƞ) Ǫ.RemoveAll(ǆ => ǆ.BlockDefinition.SubtypeId.Contains("Hydrogen")); if (ǣ == ȡ) Ǫ.RemoveAll(ǆ => !ǆ.BlockDefinition.
SubtypeId.Contains("Hydrogen")); foreach (var Ǭ in Ǫ) { if (Ǭ.FilledRatio > 0) { Ǭ.AutoRefillBottles = true; return Ǭ; } }
            List<IMyGasGenerator>
Ɍ = ȋ; Ɍ.RemoveAll(ʅ => !Ɖ.Contains(ʅ.CubeGrid) || ʅ.Enabled == false); MyDefinitionId Ĕ = MyDefinitionId.Parse(Ȝ + Ȫ + "/Ice"); foreach (
var ʆ in Ɍ) { if (â(Ĕ, ʆ) > 0) { ʆ.AutoRefill = true; return ʆ; } }
            return null;
        }
        void ʇ()
        {
            char ʈ = '0'; char ʉ = '0'; char[] ʊ = { 'A', 'N', 'T', 'X' }
; char[] ʋ = { 'a', 'd' }; if (sortingPattern.Length == 2) { ʈ = sortingPattern[0]; ʉ = sortingPattern[1]; }
            if (enableInternalSorting)
            {
                if (ʈ.
ToString().IndexOfAny(ʊ) < 0 || ʉ.ToString().IndexOfAny(ʋ) < 0)
                {
                    Ƣ("You provided the invalid sorting pattern '" + sortingPattern +
"'!\nCan't sort the inventories!"); return;
                }
                GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(ǿ, u => u.HasInventory && !Ȯ.Contains(u.CubeGrid) && !u.
CustomName.Contains(ǎ) && u.InventoryCount == 1);
            }
            else
            {
                GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(ǿ, u => u.HasInventory && !Ȯ.
Contains(u.CubeGrid) && !u.CustomName.Contains(ǎ) && u.InventoryCount == 1 && u.CustomName.Contains("(sort:"));
            }
            for (var ɪ = ȼ; ɪ < ǿ.Count; ɪ
++)
            {
                if (Ʊ(20)) return; if (ȼ >= ǿ.Count - 1) { ȼ = 0; } else { ȼ++; }
                var Â = ǿ[ɪ].GetInventory(0); var º = new List<MyInventoryItem>(); Â.
GetItems(º); if (º.Count > 200) continue; char ʌ = ʈ; char ʍ = ʉ; string ʎ = System.Text.RegularExpressions.Regex.Match(ǿ[ɪ].CustomName,
@"(\(sort:)(.{2})", System.Text.RegularExpressions.RegexOptions.IgnoreCase).Groups[2].Value; if (ʎ.Length == 2)
                {
                    ʈ = ʎ[0]; ʉ = ʎ[1]; if (ʈ.ToString().
IndexOfAny(ʊ) < 0 || ʉ.ToString().IndexOfAny(ʋ) < 0)
                    {
                        Ƣ("You provided an invalid sorting pattern in\n'" + ǿ[ɪ].CustomName +
"'!\nUsing global pattern!"); ʈ = ʌ; ʉ = ʍ;
                    }
                }
                var ʏ = new List<MyInventoryItem>(); Â.GetItems(ʏ); if (ʈ == 'A')
                {
                    if (ʉ == 'd')
                    {
                        ʏ.Sort((Ĉ, í) => í.Amount.ToIntSafe().
CompareTo(Ĉ.Amount.ToIntSafe()));
                    }
                    else { ʏ.Sort((Ĉ, í) => Ĉ.Amount.ToIntSafe().CompareTo(í.Amount.ToIntSafe())); }
                }
                else if (ʈ == 'N')
                {
                    if (ʉ
== 'd') { ʏ.Sort((Ĉ, í) => í.Type.SubtypeId.ToString().CompareTo(Ĉ.Type.SubtypeId.ToString())); }
                    else
                    {
                        ʏ.Sort((Ĉ, í) => Ĉ.Type.
SubtypeId.ToString().CompareTo(í.Type.SubtypeId.ToString()));
                    }
                }
                else if (ʈ == 'T')
                {
                    if (ʉ == 'd')
                    {
                        ʏ.Sort((Ĉ, í) => í.Type.ToString().
CompareTo(Ĉ.Type.ToString()));
                    }
                    else { ʏ.Sort((Ĉ, í) => Ĉ.Type.ToString().CompareTo(í.Type.ToString())); }
                }
                else if (ʈ == 'X')
                {
                    if (ʉ == 'd')
                    {
                        ʏ.
Sort((Ĉ, í) => (í.Type.TypeId.ToString() + í.Amount.ToIntSafe().ToString(@"000000000")).CompareTo((Ĉ.Type.TypeId.ToString() + Ĉ.
Amount.ToIntSafe().ToString(@"000000000"))));
                    }
                    else
                    {
                        ʏ.Sort((Ĉ, í) => (Ĉ.Type.TypeId.ToString() + Ĉ.Amount.ToIntSafe().ToString(
@"000000000")).CompareTo((í.Type.TypeId.ToString() + í.Amount.ToIntSafe().ToString(@"000000000"))));
                    }
                }
                if (ʏ.SequenceEqual(º, new Ź()))
                    continue; foreach (var F in ʏ)
                {
                    string ɻ = F.ToString(); for (int u = 0; u < º.Count; u++)
                    {
                        if (º[u].ToString() == ɻ)
                        {
                            Â.TransferItemTo(Â, u, º.
Count, false); º.Clear(); Â.GetItems(º); break;
                        }
                    }
                }
                ʈ = ʌ; ʉ = ʍ;
            }
        }
        void ɼ()
        {
            for (var ɪ = Ƚ; ɪ < ȵ.Count; ɪ++)
            {
                if (Runtime.
CurrentInstructionCount > Runtime.MaxInstructionCount * 0.75) { return; }
                if (Ƚ >= ȵ.Count - 1) { Ƚ = 0; } else { Ƚ++; }
                Ñ(ȵ[ɪ]); int ã = 0; if (ȵ[ɪ].BlockDefinition.
SubtypeId.Contains("Assembler")) { IMyAssembler ĉ = ȵ[ɪ] as IMyAssembler; if (ĉ.Mode == MyAssemblerMode.Disassembly) ã = 1; }
                var M = ȵ[ɪ].
CustomData.Split('\n'); foreach (var O in M)
                {
                    if (!O.Contains("=")) continue; MyDefinitionId Ö; double ɽ = 0; var ɾ = O.Split('='); if (ɾ.Length
>= 2)
                    {
                        if (!MyDefinitionId.TryParse(Ȝ + ɾ[0], out Ö)) continue; double.TryParse(ɾ[1], out ɽ); if (ɾ[1].ToLower().Contains("all"))
                        {
                            ɽ =
int.MaxValue; Ç(ȵ[ɪ], int.MaxValue);
                        }
                    }
                    else { continue; }
                    double ɿ = â(Ö, ȵ[ɪ], ã); double ɢ = 0; if (ɽ >= 0) { ɢ = ɽ - ɿ; } else { ɢ = Math.Abs(ɽ) - ɿ; }
                    if
(ɢ >= 1 && ɽ >= 0)
                    {
                        IMyTerminalBlock S = null; if (allowSpecialSteal) { S = ä(Ö, true, ȵ[ɪ]); } else { S = ä(Ö); }
                        if (S != null)
                        {
                            Q(Ö.ToString(), S, 0,
ȵ[ɪ], ã, ɢ, true);
                        }
                    }
                    else if (ɢ < 0) { IMyTerminalBlock U = æ(ȵ[ɪ]); if (U != null) Q(Ö.ToString(), ȵ[ɪ], ã, U, 0, Math.Abs(ɢ), true); }
                }
            }
        }
        void
ʀ(List<IMyTerminalBlock> Ĵ)
        {
            foreach (var Ò in Ĵ)
            {
                string ʁ = Ò.CustomName; string ʂ = ""; var ʃ = System.Text.RegularExpressions.
Regex.Match(ʁ, @"\(\d+\.?\d*\%\)").Value; if (ʃ != "") { ʂ = ʁ.Replace(ʃ, "").TrimEnd(' '); } else { ʂ = ʁ; }
                var Â = Ò.GetInventory(0); string Ř =
((double)Â.CurrentVolume).ų((double)Â.MaxVolume); if (showFillLevel) { ʂ += " (" + Ř + ")"; ʂ = ʂ.Replace("  ", " "); }
                if (ʂ != ʁ) Ò.
CustomName = ʂ;
            }
        }
        string ʄ()
        {
            if (Ɋ.Count > 1)
            {
                string ī = @"(" + autocraftingKeyword + @" *)(\d*)"; Ɋ.Sort((Ĉ, í) => System.Text.RegularExpressions
.Regex.Match(Ĉ.CustomName, ī).Groups[2].Value.CompareTo(System.Text.RegularExpressions.Regex.Match(í.CustomName, ī).Groups[
2].Value));
            }
            string ļ = ""; if (!Ɋ[0].GetPublicText().Contains("Isy's Autocrafting")) { Ɋ[0].FontSize = 0.8f; }
            foreach (var K in Ɋ)
            {
                ļ += K.GetPublicText() + "\n"; K.WritePublicTitle("Craft item manually once to show up here"); K.FontSize = Ɋ[0].FontSize; K.Font =
                         Ɋ[0].Font; K.SetValue<Int64>("alignment", 0); K.ShowPublicTextOnScreen();
            }
            List<string> ğ = ļ.Split('\n').ToList(); List<string> M
= Ɋ[0].CustomData.Split('\n').ToList(); ğ.RemoveAll(O => O.IndexOfAny(Ȕ) <= 0); foreach (var Ú in ȭ)
            {
                bool ʤ = false; foreach (var O
in ğ) { string ʥ = O.Remove(O.IndexOf(" ")); if (ʥ == Ú) { ʤ = true; break; } }
                foreach (var O in M) { if (O == Ú) { ʤ = true; break; } }
                if (!ʤ)
                {
                    MyDefinitionId Ö = Ʃ(Ú); double ʦ = Math.Ceiling(â(Ö)); ļ += "\n" + Ú + " " + ʦ + " = " + ʦ;
                }
            }
            List<string> ʔ = ļ.Split('\n').ToList(); string q = ""; ʔ.
RemoveAll(O => O.IndexOfAny(Ȕ) <= 0); try
            {
                IOrderedEnumerable<string> ʧ; ʧ = ʔ.OrderBy(Ĉ => Ĉ.Substring(0, Ĉ.IndexOf(" "))); foreach (var O in ʧ
)
                {
                    bool ʨ = false; string Ú = O.Remove(O.IndexOf(" ")); string ʩ = O.Replace(Ú, ""); foreach (var F in ȭ) { if (F == Ú) { ʨ = true; break; } }
                    if (
ʨ) q += Ú + ʩ + "\n";
                }
            }
            catch { }
            return q.TrimEnd('\n');
        }
        void ʪ(string ļ)
        {
            if (ļ == "")
            {
                ļ = "Autocrafting error!\n\nNo items for crafting available!\n\nIf you hid all items, check the custom data of the first autocrafting panel and reenable some of them.\n\nOtherwise, store or build new items manually!"
; string ğ = ň(Ɋ[0].FontSize, ļ, Ɋ[0]); Ɋ[0].WritePublicText(ğ); return;
            }
            var h = ļ.Split('\n'); int j = h.Length; int k = 0; float e = Ɋ[0]
.FontSize; ƌ = Ɋ[0].Font == "Monospace" ? true : false; int m = (int)Math.Ceiling(17 / e); int w = 650; float ʫ = 0; foreach (var K in Ɋ)
            {
                w = K.
BlockDefinition.SubtypeName.Contains("Wide") ? 1300 : 650; int o = 0; List<string> q = new List<string>(); if (K == Ɋ[0] || headerOnEveryScreen)
                {
                    string
ʬ = Ȓ; if (headerOnEveryScreen && Ɋ.Count > 1)
                    {
                        ʬ += " " + (Ɋ.IndexOf(K) + 1) + "/" + Ɋ.Count; try { ʬ += " [" + h[k][0] + "-#]"; }
                        catch
                        {
                            ʬ += " [Empty]"
;
                        }
                    }
                    string ʣ = '='.ŭ((int)Math.Ceiling(ƍ(ʬ, e) / ũ('=', e))); q.Add(ʬ); q.Add(ʣ + "\n"); string ʡ = "Component "; string ʐ =
                               "Current | Wanted "; ʫ = ƍ("Wanted ", e); string Ş = ' '.ŭ((int)((w - ƍ(ʡ, e) - ƍ(ʐ, e)) / ũ(' ', e))); q.Add(ʡ + Ş + ʐ + "\n"); o = 5;
                } while ((k < j && o < m) || (K == Ɋ[Ɋ.
Count - 1] && k < j))
                {
                    var O = h[k].Split(' '); O[0] += " "; O[1] = O[1].Replace('$', ' '); string Ş = ' '.ŭ((int)((w - ƍ(O[0], e) - ƍ(O[1], e) - ʫ) / ũ(
              ' ', e))); string ʑ = O[0] + Ş + O[1] + O[2]; q.Add(ʑ); k++; o++;
                }
                if (headerOnEveryScreen && Ɋ.Count > 1) { q[0] = q[0].Replace('#', h[k - 1][0]); }
                K
.WritePublicText(String.Join("\n", q));
            }
        }
        void ʒ()
        {
            Ɋ.Clear(); GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(Ɋ, K => Ɖ.
Contains(K.CubeGrid) && K.CustomName.Contains(autocraftingKeyword)); if (Ɋ.Count == 0) return; if (ȇ.Count == 0)
            {
                Ƣ(
"No assemblers found!\nBuild assemblers to enable autocrafting!"); return;
            }
            Ċ(); List<MyDefinitionId> ʓ = new List<MyDefinitionId>(); var ʔ = ʄ().Split('\n'); string q = ""; foreach (var O in ʔ)
            {
                string Ú = ""; try { Ú = O.Substring(0, O.IndexOf(" ")); } catch { continue; }
                MyDefinitionId Ö = Ʃ(Ú); if (Ö == null) continue; double ʕ = Math.
Ceiling(â(Ö)); string ʖ = O.Substring(O.IndexOfAny(Ȕ) + 1); double ʗ = 0; double.TryParse(System.Text.RegularExpressions.Regex.Replace(ʖ
, @"\D", ""), out ʗ); if (ʖ.Contains("-"))
                {
                    if (!Ɋ[0].CustomData.Contains(ȓ)) Ɋ[0].CustomData = ȓ; Ɋ[0].CustomData += "\n" + Ú; continue;
                }
                double ʘ = Math.Abs(ʗ - ʕ); bool ʙ; MyDefinitionId Ƒ = ƨ(Ö, out ʙ); double ʚ = Ƽ(Ƒ); if (ʕ >= ʗ + ʗ * assembleMargin && ʚ > 0 && !O.Contains("[D:"
                            )) { ɰ(Ƒ); ʚ = 0; Ɂ = "Removed '" + Ö.SubtypeId.ToString() + "' from the assembling queue."; }
                if (ʕ <= ʗ - ʗ * disassembleMargin && ʚ > 0 && O.
Contains("[D:")) { ɰ(Ƒ); ʚ = 0; Ɂ = "Removed '" + Ö.SubtypeId.ToString() + "' from the disassembling queue."; }
                string Õ = ""; if (ʚ > 0 || ʘ > 0)
                {
                    if (
enableAutodisassembling && ʕ > ʗ + ʗ * disassembleMargin) { Õ = "$[D:"; }
                    else if (enableAutocrafting && ʕ < ʗ - ʗ * assembleMargin) { Õ = "$[A:"; }
                    if (Õ != "")
                    {
                        if (ʚ == 0)
                        {
                            Õ +=
"Wait]";
                        }
                        else { Õ += Math.Round(ʚ) + "]"; }
                    }
                }
                if (!ʙ) Õ = "$[NoBP!]"; string Å = ""; if (ʙ && ʖ.Contains("!")) { ʓ.Add(Ƒ); Å = "!"; }
                string ʛ = "$=$ "; if (
ʕ > ʗ) ʛ = "$>$ "; if (ʕ < ʗ) ʛ = "$<$ "; q += Ú + " " + ʕ + Õ + ʛ + ʗ + Å + "\n"; if (Õ.Contains("[D:Wait]")) { ɭ(Ƒ, ʘ); }
                else if (Õ.Contains("[A:Wait]"))
                {
                    ɫ
(Ƒ, ʘ); Ɂ = "Queued " + ʘ + " '" + Ö.SubtypeId.ToString() + "' in the assemblers.";
                }
                else if (Õ.Contains("[NoBP!]") && ʗ > ʕ)
                {
                    Ƣ(
"Unsuccessfully attempted to craft\n'" + Ö.SubtypeId.ToString() + "'\nA valid blueprint for this item couldn't be found!\nQueue it up a few times with no other item in queue\nso that the script can learn its blueprint."
);
                }
            }
            ċ(); ɨ(ʓ); ʪ(q.TrimEnd('\n'));
        }
        void ʜ()
        {
            MyDefinitionId ʝ = MyDefinitionId.Parse(Ȝ + Ȫ + "/Stone"); MyDefinitionId Ƒ =
MyDefinitionId.Parse(ȣ + "StoneOreToIngotBasic"); double ʞ = â(ʝ); if (ʞ > 0)
            {
                double ʟ = Math.Ceiling(ʞ / 500 / ȉ.Count); foreach (var ʠ in ȉ)
                {
                    if (ʠ.
IsQueueEmpty) ʠ.AddQueueItem(Ƒ, ʟ);
                }
            }
        }
        void ʢ()
        {
            if (Ⱦ == 0) ɺ(ȯ, Ȫ); if (Ⱦ == 1) ɺ(Ȱ, ȝ); if (Ⱦ == 2) ɺ(ȱ, Ȟ); if (Ⱦ == 3) ɺ(Ȳ, Ȣ); if (Ⱦ == 4) ɺ(ȳ, ȟ); if (Ⱦ == 5) ɺ(ȴ,
 "ContainerObject"); Ⱦ++; if (Ⱦ > 5) Ⱦ = 0;
        }
        void ɺ(List<IMyTerminalBlock> Ĵ, string è)
        {
            Ĵ.RemoveAll(ê => ê.InventoryCount == 2 || ê.BlockDefinition.
TypeIdString.Contains("OxygenGenerator") || ê.BlockDefinition.TypeIdString.Contains("OxygenTank")); if (Ĵ.Count <= 1) { Ⱦ++; return; }
            Dictionary<MyDefinitionId, double> ɚ = new Dictionary<MyDefinitionId, double>(); foreach (var Ò in Ĵ)
            {
                var º = new List<MyInventoryItem>(); Ò
.GetInventory(0).GetItems(º); foreach (var F in º)
                {
                    if (!F.Type.TypeId.ToString().Contains(è)) continue; MyDefinitionId Ö = F.
Type; if (ɚ.ContainsKey(Ö)) { ɚ[Ö] += (double)F.Amount; } else { ɚ[Ö] = (double)F.Amount; }
                }
            }
            Dictionary<MyDefinitionId, double> ɛ = new
Dictionary<MyDefinitionId, double>(); foreach (var F in ɚ) { ɛ[F.Key] = F.Value / Ĵ.Count; }
            foreach (var Ò in Ĵ)
            {
                if (Ʊ(75)) { Ⱦ--; return; }
                var ɜ =
new List<MyInventoryItem>(); Ò.GetInventory(0).GetItems(ɜ); Dictionary<MyDefinitionId, double> ɝ = new Dictionary<MyDefinitionId,
double>(); foreach (var F in ɜ)
                {
                    MyDefinitionId Ö = F.Type; if (ɝ.ContainsKey(Ö)) { ɝ[Ö] += (double)F.Amount; }
                    else
                    {
                        ɝ[Ö] = (double)F.Amount;
                    }
                }
                double ɞ = 0; foreach (var F in ɚ)
                {
                    ɝ.TryGetValue(F.Key, out ɞ); double ɟ = ɛ[F.Key]; if (ɞ >= ɟ - 1) continue; foreach (var ɠ in Ĵ)
                    {
                        double ɡ = â(F.Key, ɠ); if (ɡ <= ɟ + 1) continue; double ɢ = ɛ[F.Key] - ɞ; if (ɢ > ɡ - ɟ) ɢ = ɡ - ɟ; if (ɢ > 0)
                        {
                            ɞ += Q(F.Key.ToString(), ɠ, 0, Ò, 0, ɢ, true); if (ɞ >=
ɟ - 1 && ɞ <= ɟ + 1) break;
                        }
                    }
                }
            }
            if (!Ʊ(50)) Ⱦ++;
        }
        void ɣ()
        {
            if (ȃ.Count == 0) return; if (Ȇ.Count == 0) Ⱥ = false; if (enableStoneProcessing)
            {
                if (!Ș.
Contains("Stone")) { Ș.Add("Stone"); Ț.Add("Stone"); }
            }
            Ș = ɳ(Ș); ș = ɳ(ș); Ț = ɳ(Ț); if (enableValueOrePriority && Ȇ.Count > 0)
            {
                Ⱥ = false; foreach (
var F in ș) { var Ö = MyDefinitionId.Parse(Ȝ + Ȫ + "/" + F); if (â(Ö) > 0) { Ⱥ = true; break; } }
            }
            if (Ⱥ)
            {
                foreach (var Ǚ in ȅ)
                {
                    IMyTerminalBlock U = æ
(Ǚ, oreContainerKeyword); if (U != null) { foreach (var ɒ in Ț) { Q(ɒ, Ǚ, 0, U, 0); } }
                }
            }
            if (enableBasicOreSpecialization || Ⱥ) { Ɏ(ȅ, ș); }
            else
            { Ɏ(ȅ, Ș); }
            Ɏ(Ȇ, Ț); if (enableBasicOreSpecialization || Ⱥ) { ɓ(ȅ, ș); } else { ɓ(ȅ, Ș); }
            ɓ(Ȇ, Ț); bool ɤ = false; if (!
enableBasicOreSpecialization && !Ⱥ) { ɤ = ɕ(ȅ, Ȇ, Ț); }
            if (!ɤ) { ɕ(Ȇ, ȅ, Ț); }
            ɹ(ȅ); ɹ(Ȇ); if (sortRefiningQueue)
            {
                foreach (IMyRefinery Ǚ in ȃ)
                {
                    var Â = Ǚ.GetInventory(0);
                    var º = new List<MyInventoryItem>(); Â.GetItems(º); if (º.Count < 2) continue; bool ɦ = false; int ə = 0; string ɍ = ""; foreach (var ɔ in Ș)
                    {
                        for (int u = 0; u < º.Count; u++) { if (º[u].Type.SubtypeId == ɔ) { ə = u; ɍ = ɔ; ɦ = true; break; } }
                        if (ɦ) break;
                    }
                    if (ə != 0)
                    {
                        Â.TransferItemTo(Â, ə, 0,
true); Ɂ = "Sorted the refining queue.\n'" + ɍ + "' is now at the front of the queue.";
                    }
                }
            }
        }
        void Ɏ(List<IMyTerminalBlock> ɏ, List<
string> ɐ)
        {
            MyDefinitionId ɑ = new MyDefinitionId(); foreach (var ɒ in ɐ)
            {
                var Ö = MyDefinitionId.Parse(Ȝ + Ȫ + "/" + ɒ); if (â(Ö) > 0)
                {
                    ɑ = Ö; break
;
                }
            }
            if (!ɑ.ToString().Contains(Ȫ)) return; foreach (var Ǚ in ɏ)
            {
                var Â = Ǚ.GetInventory(0); if ((double)Â.CurrentVolume > (double)Â.
MaxVolume * 0.95)
                {
                    var º = new List<MyInventoryItem>(); Â.GetItems(º); foreach (var F in º) { if ((MyDefinitionId)F.Type == ɑ) return; }
                    IMyTerminalBlock U = æ(Ǚ, oreContainerKeyword); if (U != null) { Q("", Ǚ, 0, U, 0); }
                }
            }
        }
        void ɓ(List<IMyTerminalBlock> ɏ, List<string> ɐ)
        {
            var Ĵ = new List<
IMyTerminalBlock>(); Ĵ.AddRange(Ȁ); Ĵ.AddRange(ȵ); IMyTerminalBlock Ȼ = null; foreach (var Ǚ in ɏ)
            {
                var Â = Ǚ.GetInventory(0); if ((double)Â.
CurrentVolume > (double)Â.MaxVolume * 0.75) continue; foreach (var F in ɐ)
                {
                    if ((double)Â.CurrentVolume > (double)Â.MaxVolume * 0.95) return;
                    MyDefinitionId Ö = MyDefinitionId.Parse(Ȝ + Ȫ + "/" + F); IMyTerminalBlock S = null; if (â(Ö) == 0) continue; if (Ȼ != null)
                    {
                        if (Ȼ.GetInventory(0).FindItem
(Ö) != null) { S = Ȼ; }
                    }
                    if (S == null) { foreach (var Ò in Ĵ) { if (Ò.GetInventory(0).FindItem(Ö) != null) { S = Ò; Ȼ = Ò; } } }
                    if (S == null) continue; Q
(Ö.ToString(), S, 0, Ǚ, 0);
                }
                return;
            }
        }
        bool ɕ(List<IMyTerminalBlock> ɖ, List<IMyTerminalBlock> ɗ, List<string> ɐ)
        {
            foreach (var ɘ in ɖ
)
            {
                if ((double)ɘ.GetInventory(0).CurrentVolume > 0.05) continue; foreach (var ɥ in ɗ)
                {
                    if ((double)ɥ.GetInventory(0).CurrentVolume
> 0) { foreach (var ɒ in ɐ) { Q(ɒ, ɥ, 0, ɘ, 0, -0.5); } return true; }
                }
            }
            return false;
        }
        void ɹ(List<IMyTerminalBlock> ɲ)
        {
            if (ɲ.Count < 2)
                return; Dictionary<MyDefinitionId, double> ɚ = new Dictionary<MyDefinitionId, double>(); foreach (var Ò in ɲ)
            {
                var º = new List<
MyInventoryItem>(); Ò.GetInventory(0).GetItems(º); foreach (var F in º)
                {
                    MyDefinitionId Ö = F.Type; if (ɚ.ContainsKey(Ö))
                    {
                        ɚ[Ö] += (double)F.
Amount;
                    }
                    else { ɚ[Ö] = (double)F.Amount; }
                }
            }
            Dictionary<MyDefinitionId, double> ɛ = new Dictionary<MyDefinitionId, double>(); foreach (var F
in ɚ) { ɛ[F.Key] = (int)(F.Value / ɲ.Count); }
            foreach (var Ò in ɲ)
            {
                if (Ʊ(75)) { return; }
                var ɜ = new List<MyInventoryItem>(); Ò.
GetInventory(0).GetItems(ɜ); Dictionary<MyDefinitionId, double> ɝ = new Dictionary<MyDefinitionId, double>(); foreach (var F in ɜ)
                {
                    MyDefinitionId Ö = F.Type; if (ɝ.ContainsKey(Ö)) { ɝ[Ö] += (double)F.Amount; } else { ɝ[Ö] = (double)F.Amount; }
                }
                double ɞ = 0; foreach (var F in ɚ)
                {
                    ɝ.
TryGetValue(F.Key, out ɞ); double ɟ = ɛ[F.Key]; if (ɞ >= ɟ - 1) continue; foreach (var ɠ in ɲ)
                    {
                        double ɡ = â(F.Key, ɠ); if (ɡ <= ɟ + 1) continue; double ɢ = ɛ
[F.Key] - ɞ; if (ɢ > ɡ - ɟ) ɢ = ɡ - ɟ; if (ɢ > 0) { ɞ += Q(F.Key.ToString(), ɠ, 0, Ò, 0, ɢ, true); if (ɞ >= ɟ - 1 && ɞ <= ɟ + 1) break; }
                    }
                }
            }
        }
        List<string> ɳ(List<
string> ɴ)
        {
            ɴ.Sort((Ĉ, í) => (â(MyDefinitionId.Parse(Ȝ + ȝ + "/" + Ĉ)) / ƺ(Ĉ)).CompareTo((â(MyDefinitionId.Parse(Ȝ + ȝ + "/" + í)) / ƺ(í)))); return
                      ɴ;
        }
        void ɵ(double ɶ)
        {
            foreach (var ĉ in ȇ)
            {
                if (ĉ.GetOwnerFactionTag() == Me.GetOwnerFactionTag())
                {
                    var ɷ = ĉ.GetInventory(0); if ((
float)ɷ.CurrentVolume >= (float)ɷ.MaxVolume * (ɶ / 100) || ĉ.Mode == MyAssemblerMode.Disassembly)
                    {
                        IMyTerminalBlock U = æ(ĉ,
ingotContainerKeyword); if (U != null) Q("", ĉ, 0, U, 0);
                    }
                }
            }
        }
        void ɸ()
        {
            foreach (IMyAssembler ĉ in ȇ)
            {
                if (ĉ.Mode == MyAssemblerMode.Disassembly) continue; if (
ĉ.CustomData.Contains("skipQueueSorting")) { ĉ.CustomData = ""; continue; }
                var Õ = new List<MyProductionItem>(); ĉ.GetQueue(Õ); if (
Õ.Count < 2) continue; double ɱ = Double.MaxValue; int ə = 0; string ɍ = ""; for (int u = 0; u < Õ.Count; u++)
                {
                    MyDefinitionId Ö = Ƥ(Õ[u].
BlueprintId); double ɧ = â(Ö); if (ɧ < ɱ) { ɱ = ɧ; ə = u; ɍ = Ö.SubtypeId.ToString(); }
                }
                if (ə != 0)
                {
                    ĉ.MoveQueueItemRequest(Õ[ə].ItemId, 0); Ɂ =
"Sorted the assembling queue.\n'" + ɍ + "' is now at the front of the queue.";
                }
            }
        }
        void ɨ(List<MyDefinitionId> ɩ)
        {
            if (ɩ.Count == 0) return; if (ɩ.Count > 1) ɩ.Sort((Ĉ, í)
=> â(Ƥ(Ĉ)).CompareTo(â(Ƥ(í)))); foreach (var ĉ in ȇ)
            {
                var Õ = new List<MyProductionItem>(); ĉ.GetQueue(Õ); if (Õ.Count < 2) continue;
                foreach (var Ƒ in ɩ)
                {
                    int ɪ = Õ.FindIndex(u => u.BlueprintId == Ƒ); if (ɪ == -1) continue; if (ɪ == 0) { ĉ.CustomData = "skipQueueSorting"; break; }
                    ĉ.
MoveQueueItemRequest(Õ[ɪ].ItemId, 0); ĉ.CustomData = "skipQueueSorting"; Ɂ = "Sorted the assembler queue by priority.\n'" + Ƥ(Ƒ).SubtypeId.ToString()
+ "' is now at the front of the queue."; break;
                }
            }
        }
        void ɫ(MyDefinitionId Ƒ, double G)
        {
            List<IMyAssembler> ɬ = new List<
IMyAssembler>(); foreach (IMyAssembler ĉ in ȇ)
            {
                if (ĉ.Mode == MyAssemblerMode.Disassembly && !ĉ.IsQueueEmpty) continue; if (ĉ.Mode ==
MyAssemblerMode.Disassembly) { ĉ.Mode = MyAssemblerMode.Assembly; }
                if (ĉ.CanUseBlueprint(Ƒ)) { ɬ.Add(ĉ); }
            }
            ɮ(ɬ, Ƒ, G);
        }
        void ɭ(MyDefinitionId Ƒ,
double G)
        {
            List<IMyAssembler> ɬ = new List<IMyAssembler>(); foreach (IMyAssembler ĉ in ȇ)
            {
                if (ĉ.Mode == MyAssemblerMode.Assembly && ĉ.
IsProducing) continue; if (ĉ.Mode == MyAssemblerMode.Assembly) { ĉ.ClearQueue(); ĉ.Mode = MyAssemblerMode.Disassembly; }
                if (ĉ.Mode ==
MyAssemblerMode.Assembly) continue; if (ĉ.CanUseBlueprint(Ƒ)) { ɬ.Add(ĉ); }
            }
            ɮ(ɬ, Ƒ, G);
        }
        void ɮ(List<IMyAssembler> ɬ, MyDefinitionId Ƒ, double G)
        {
            if (ɬ.Count == 0) return; double ɯ = Math.Ceiling(G / ɬ.Count); foreach (IMyAssembler ĉ in ɬ)
            {
                if (ɯ > G) ɯ = Math.Ceiling(G); if (G > 0)
                {
                    ĉ.
InsertQueueItem(0, Ƒ, ɯ); G -= ɯ;
                }
                else { break; }
            }
        }
        void ɰ(MyDefinitionId Ƒ)
        {
            foreach (IMyAssembler ĉ in ȇ)
            {
                var Õ = new List<MyProductionItem>(); ĉ.
GetQueue(Õ); for (int u = 0; u < Õ.Count; u++) { if (Õ[u].BlueprintId == Ƒ) ĉ.RemoveQueueItem(u, Õ[u].Amount); }
            }
        }
        void Ċ()
        {
            foreach (IMyAssembler
ĉ in ȇ)
            {
                ĉ.UseConveyorSystem = true; ĉ.CooperativeMode = false; ĉ.Repeating = false; if (ĉ.Mode == MyAssemblerMode.Disassembly && !ĉ.
                 IsProducing) { ĉ.ClearQueue(); ĉ.Mode = MyAssemblerMode.Assembly; }
            }
        }
        void ċ()
        {
            List<IMyAssembler> Č = new List<IMyAssembler>(ȇ); Č.RemoveAll(Ĉ
=> Ĉ.IsQueueEmpty); if (Č.Count == 0) return; List<IMyAssembler> č = new List<IMyAssembler>(ȇ); č.RemoveAll(Ĉ => !Ĉ.IsQueueEmpty);
            foreach (var Ď in Č)
            {
                if (č.Count == 0) return; var Õ = new List<MyProductionItem>(); Ď.GetQueue(Õ); double ď = (double)Õ[0].Amount; if (ď <=
        100) continue; double Đ = Math.Ceiling(ď / 2); foreach (var đ in č)
                {
                    if (!đ.CanUseBlueprint(Õ[0].BlueprintId)) continue; đ.Mode = Ď.Mode;
                    if (đ.Mode != Ď.Mode) continue; đ.AddQueueItem(Õ[0].BlueprintId, Đ); Ď.RemoveQueueItem(0, Đ); č.Remove(đ); break;
                }
            }
        }
        void Ē()
        {
            if (ȋ.
Count == 0) return; double ē = iceFillLevelPercentage / 100; MyDefinitionId Ĕ = MyDefinitionId.Parse(Ȝ + Ȫ + "/Ice"); string ĕ = Ĕ.ToString();
            double Ė = 0.00037; foreach (IMyGasGenerator ė in ȋ)
            {
                var Â = ė.GetInventory(0); double Ę = â(Ĕ, ė); double ę = Ę * Ė; double Ě = (double)Â.
MaxVolume; if (ę > Ě * (ē + 0.001)) { IMyTerminalBlock U = æ(ė, oreContainerKeyword); if (U != null) { double ā = (ę - Ě * ē) / Ė; Q(ĕ, ė, 0, U, 0, ā); } }
                else if (ę
< Ě * (ē - 0.001)) { IMyTerminalBlock S = ä(Ĕ, true); if (S != null) { double ā = (Ě * ē - ę) / Ė; Q(ĕ, S, 0, ė, 0, ā); } }
            }
            double ě = 0; double Ĝ = 0; foreach
(var ė in ȋ) { ě += â(Ĕ, ė); var Â = ė.GetInventory(0); Ĝ += (double)Â.MaxVolume; }
            double î = (ě * Ė) / Ĝ; foreach (var û in ȋ)
            {
                var X = û.
GetInventory(0); double ï = â(Ĕ, û); double ð = ï * Ė; double ñ = (double)X.MaxVolume; if (ð > ñ * (î + 0.001))
                {
                    foreach (var ò in ȋ)
                    {
                        if (û == ò) continue; var
Á = ò.GetInventory(0); double ó = â(Ĕ, ò); double ô = ó * Ė; double õ = (double)Á.MaxVolume; if (ô < õ * (î - 0.001))
                        {
                            double ö = ((õ * î) - ô) / Ė; if ((
ï - ö) * Ė >= ñ * î && ö > 5) { ï -= Q(ĕ, û, 0, ò, 0, ö); continue; }
                            if ((ï - ö) * Ė < ñ * î && ö > 5) { double ø = (ï * Ė - ñ * î) / Ė; Q(ĕ, û, 0, ò, 0, ø); break; }
                        }
                    }
                }
            }
        }
        void ù
()
        {
            if (Ȍ.Count == 0) return; MyDefinitionId ú = MyDefinitionId.Parse(Ȝ + ȝ + "/Uranium"); string ü = ú.ToString(); double ć = 0; double ý = 0
                           ; foreach (IMyReactor þ in Ȍ)
            {
                þ.UseConveyorSystem = false; double ÿ = â(ú, þ); double Ā = uraniumAmountLargeGrid; if (þ.CubeGrid.
GridSize == 0.5f) Ā = uraniumAmountSmallGrid; ý += Ā; if (ÿ > Ā + 0.05)
                {
                    IMyTerminalBlock U = æ(þ, ingotContainerKeyword); if (U != null)
                    {
                        double ā = ÿ - Ā
; Q(ü, þ, 0, U, 0, ā);
                    }
                }
                else if (ÿ < Ā - 0.05) { IMyTerminalBlock S = ä(ú, true); if (S != null) { double ā = Ā - ÿ; Q(ü, S, 0, þ, 0, ā); } }
                ć += â(ú, þ);
            }
            double Ă = ć / ý; foreach (var ă in Ȍ)
            {
                double Ą = â(ú, ă); double ą = Ă * uraniumAmountLargeGrid; if (ă.CubeGrid.GridSize == 0.5f) ą = Ă *
uraniumAmountSmallGrid; if (Ą > ą + 0.05)
                {
                    foreach (var Ć in Ȍ)
                    {
                        if (ă == Ć) continue; double ĝ = â(ú, Ć); double Ğ = Ă * uraniumAmountLargeGrid; if (Ć.CubeGrid.
GridSize == 0.5f) Ğ = Ă * uraniumAmountSmallGrid; if (ĝ < Ğ - 0.05)
                        {
                            Ą = â(ú, ă); double Ľ = Ğ - ĝ; if (Ą - Ľ >= ą) { Q(ü, ă, 0, Ć, 0, Ľ); continue; }
                            if (Ą - Ľ < ą)
                            {
                                Ľ = Ą - ą
; Q(ü, ă, 0, Ć, 0, Ľ); break;
                            }
                        }
                    }
                }
            }
        }
        string ĭ(float e = 0.65f, int w = 650, bool Į = true, bool į = true, bool İ = true, bool Ĭ = true)
        {
            bool E =
false; string D = ""; D = "Isy's Inventory Manager " + ɂ[Ƀ] + "\n"; D += '='.ŭ((int)Math.Ceiling(ƍ(D, e) / ũ('=', e))) + "\n\n"; if (Į && Ģ != null)
            {
                D
+= "Warning!\n"; D += Ģ + "\n\n";
            }
            if (į)
            {
                D += "Statistics for " + ȁ.Count + " sorted cargo containers:\n\n"; D += ĳ(e, w, ȯ, "Ores"); D += ĳ(e, w
, Ȱ, "Ingots"); D += ĳ(e, w, ȱ, "Components"); D += ĳ(e, w, Ȳ, "Tools"); D += ĳ(e, w, ȳ, "Ammo"); D += ĳ(e, w, ȴ, "Bottles"); D += "\n"; E = true;
            }
            if (İ &&
(ȵ.Count > 0 || ȃ.Count > 0 || ȇ.Count > 0 || ȉ.Count > 0 || ȋ.Count > 0 || Ȍ.Count > 0))
            {
                D += "Managed blocks by type:\n"; int ı = 0; if (ȵ.Count > ı) ı
= ȵ.Count; if (ȃ.Count > ı) ı = ȃ.Count; if (ȇ.Count > ı) ı = ȇ.Count; if (ȋ.Count > ı) ı = ȋ.Count; if (Ȍ.Count > ı) ı = Ȍ.Count; int Ĳ = ı.ToString().
Length; if (ȵ.Count > 0) { D += ' '.ŭ(Ĳ - ȵ.Count.ToString().Length) + ȵ.Count + " Special Containers\n"; }
                if (enableOreBalancing && ȅ.Count > 0)
                {
                    D += ' '.ŭ(Ĳ - ȅ.Count.ToString().Length) + ȅ.Count + " Large Refineries: "; D += "Value Ore Priority " + (enableValueOrePriority ? "ON"
                             : "OFF") + "\n";
                }
                if (enableOreBalancing && Ȇ.Count > 0)
                {
                    D += ' '.ŭ(Ĳ - Ȇ.Count.ToString().Length) + Ȇ.Count + " Basic Refineries: "; D +=
"Basic Ore Specialization " + (enableBasicOreSpecialization ? "ON" : "OFF") + "\n";
                }
                if (ȋ.Count > 0)
                {
                    D += ' '.ŭ(Ĳ - ȋ.Count.ToString().Length) + ȋ.Count +
" O2/H2 Generators: "; D += "Ice Balancing " + (enableIceBalancing ? "ON" : "OFF") + "\n";
                }
                if (Ȍ.Count > 0)
                {
                    D += ' '.ŭ(Ĳ - Ȍ.Count.ToString().Length) + Ȍ.Count +
" Reactors: "; D += "Uranium Balancing " + (enableUraniumBalancing ? "ON" : "OFF") + "\n";
                }
                if (ȇ.Count > 0)
                {
                    D += ' '.ŭ(Ĳ - ȇ.Count.ToString().Length) + ȇ
.Count + " Assemblers: "; D += "Craft " + (enableAutocrafting ? "ON" : "OFF") + " | "; D += "Uncraft " + (enableAutodisassembling ? "ON" :
"OFF") + " | "; D += "Cleanup " + (enableAssemblerCleanup ? "ON" : "OFF") + "\n";
                }
                if (ȉ.Count > 0)
                {
                    D += ' '.ŭ(Ĳ - ȉ.Count.ToString().Length) + ȉ.
Count + " Survival Kits: "; D += "Ingot Crafting " + (enableIngotCrafting ? "ON" : "OFF") + "\n";
                }
                D += "\n"; E = true;
            }
            if (Ĭ && Ɂ != "")
            {
                D +=
"Last Action:\n" + Ɂ; E = true;
            }
            if (!E) { D += "-- No informations to show --"; }
            return D;
        }
        string ĳ(float e, int w, List<IMyTerminalBlock> Ĵ, string Ù)
        {
            double ĵ = 0, Ķ = 0; foreach (var Ò in Ĵ) { var Â = Ò.GetInventory(0); ĵ += (double)Â.CurrentVolume; Ķ += (double)Â.MaxVolume; }
            string ķ = Ĵ
.Count + "x " + Ù + ":"; string ĸ = ĵ.Ű(); string Ĺ = Ķ.Ű(); string ĺ = ƪ(e, w, ķ, ĵ, Ķ, ĸ, Ĺ); return ĺ;
        }
        void Ļ(string ļ = null)
        {
            Ʉ.Clear();
            GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(Ʉ, z => z.CustomName.Contains(mainLCDKeyword)); if (Ʉ.Count == 0) return; foreach (var K in Ʉ)
            {
                bool
Į = J(K, "showWarnings"); bool į = J(K, "showContainerStats"); bool İ = J(K, "showManagedBlocks"); bool Ĭ = J(K, "showLastAction"); float
e = K.FontSize; int w = K.BlockDefinition.SubtypeName.Contains("Wide") ? 1300 : 650; ƌ = K.Font == "Monospace" ? true : false; string D = "";
                if (ļ != null) { D = ļ; } else { D = ĭ(e, w, Į, į, İ, Ĭ); }
                string ğ = ň(e, D, K); K.WritePublicTitle("Isy's Inventory Manager"); K.WritePublicText(
ğ, false); K.SetValue<Int64>("alignment", 0); K.ShowPublicTextOnScreen();
            }
        }
        void Ġ()
        {
            Ɉ.Clear(); GridTerminalSystem.
GetBlocksOfType<IMyTextPanel>(Ɉ, z => z.CustomName.Contains(warningsLCDKeyword)); if (Ɉ.Count == 0) return; foreach (var K in Ɉ)
            {
                if (K.
GetPublicTitle() != "Isy's Inventory Manager Warnings") { K.FontSize = 0.5f; }
                float e = K.FontSize; ƌ = K.Font == "Monospace" ? true : false; string D =
"Isy's Inventory Manager Warnings\n"; D += '='.ŭ((int)Math.Ceiling(ƍ(D, e) / ũ('=', e))) + "\n\n"; if (Ɇ.Count == 0) { D += "- No problems detected -"; }
                else
                {
                    int ġ = 1; foreach (
var Ģ in Ɇ) { D += ġ + ". " + Ģ.Replace("\n", " ") + "\n"; ġ++; }
                }
                string ğ = ň(e, D, K); K.WritePublicTitle(
"Isy's Inventory Manager Warnings"); K.WritePublicText(ğ, false); K.SetValue<Int64>("alignment", 0); K.ShowPublicTextOnScreen();
            }
        }
        void ģ()
        {
            ǔ.Clear();
            GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(ǔ, z => z.CustomName.Contains(performanceLCDKeyword)); if (ǔ.Count == 0) return; foreach (var K in
                     ǔ)
            {
                if (K.GetPublicTitle() != "Isy's Inventory Manager Performance") { K.FontSize = 0.5f; }
                float e = K.FontSize; ƌ = K.Font ==
"Monospace" ? true : false; string D = "Isy's Inventory Manager Performance\n"; D += '='.ŭ((int)Math.Ceiling(ƍ(D, e) / ũ('=', e))) + "\n\n"; D += Ţ;
                string ğ = ň(e, D, K); K.WritePublicTitle("Isy's Inventory Manager Performance"); K.WritePublicText(ğ, false); K.SetValue<Int64>(
                        "alignment", 0); K.ShowPublicTextOnScreen();
            }
        }
        void Ĥ()
        {
            var ĥ = new List<IMyTextPanel>(); GridTerminalSystem.GetBlocksOfType<IMyTextPanel
>(ĥ, z => z.CustomName.Contains(inventoryLCDKeyword)); List<IMyTextPanel> Ħ = ĥ.Where(z => !z.CustomName.Contains(
inventoryLCDKeyword + ":")).ToList(); if (ĥ.Count == 0) return; HashSet<string> ħ = new HashSet<string>(); foreach (var K in ĥ)
            {
                ħ.Add(System.Text.
RegularExpressions.Regex.Match(K.CustomName, inventoryLCDKeyword + @":\D+").Value);
            }
            ħ.RemoveWhere(Ĩ => Ĩ == ""); List<string> ĩ = ħ.ToList(); for (int
u = ȿ; u < ĩ.Count; u++)
            {
                if (Ʊ(10)) return; ȿ++; List<IMyTextPanel> Ī = new List<IMyTextPanel>(); GridTerminalSystem.GetBlocksOfType<
     IMyTextPanel>(Ī, z => z.CustomName.Contains(ĩ[u])); string ī = inventoryLCDKeyword + @":\w+"; Ī.Sort((Ĉ, í) => System.Text.RegularExpressions.
              Regex.Match(Ĉ.CustomName, ī).Value.CompareTo(System.Text.RegularExpressions.Regex.Match(í.CustomName, ī).Value)); string D = v(Ī[0
                 ]); float e = Ī[0].FontSize; string f = Ī[0].Font; if (!Ī[0].CustomData.ToLower().Contains("noscroll")) { D = ň(e / Ī.Count, D, Ī[0], 2); }
                var h = D.Split('\n'); int j = h.Length; int k = 0; int m = (int)Math.Ceiling(17 / e); foreach (var K in Ī)
                {
                    K.FontSize = e; K.Font = f; K.
WritePublicTitle("Items for LCD Group: " + ĩ[u].Replace(inventoryLCDKeyword + ":", "")); K.SetValue<Int64>("alignment", 2); K.
ShowPublicTextOnScreen(); int o = 0; string q = ""; while (k < j && o < m) { q += h[k] + "\n"; k++; o++; }
                    K.WritePublicText(q);
                }
            }
            for (int u = ɀ; u < Ħ.Count; u++)
            {
                if (Ʊ(10))
                    return; ɀ++; string D = v(Ħ[u]); float e = Ħ[u].FontSize; if (!Ħ[u].CustomData.ToLower().Contains("noscroll")) { D = ň(e, D, Ħ[u], 0); }
                Ħ[u].
WritePublicTitle("Inventory Items"); Ħ[u].WritePublicText(D, false); Ħ[u].SetValue<Int64>("alignment", 2); Ħ[u].ShowPublicTextOnScreen();
            }
            ȿ = 0
; ɀ = 0;
        }
        string v(IMyTextPanel K)
        {
            float e = K.FontSize; int w = K.BlockDefinition.SubtypeName.Contains("Wide") ? 1300 : 650; ƌ = K.Font
== "Monospace" ? true : false; string D = ""; if (K.CustomData.Length < 1)
            {
                D = "Put an item or type name in the custom data.\n\n" +
"Examples:\nComponent\nIngot\nSteelPlate\nInteriorPlate\n\n" + "Optionally, add a max amount for the bars as a 2nd parameter.\n\n" + "Example:\nIngot 100000\n\n" +
"At last, add any of these modifiers.\n" + "There are 5 modifiers at this point:\n\n" + "'noHeading' to hide the heading\n" +
"'singleLine' to force one line per item\n" + "'noBar' to hide the bars\n" + "'noScroll' to prevent the text from scrolling\n" +
"'hideEmpty' to hide items that have an amount of 0\n\n" + "Examples:\nComponent 100000 noBar\nSteelPlate noHeading noBar hideEmpty\n\n" +
"To display multiple different items, use a new line for every item!\n\n" + "Hint: One 'noScroll' modifier per screen is enough!\n\n"; K.FontSize = 0.6f;
            }
            else
            {
                var M = K.CustomData.Split('\n').ToList()
; M.RemoveAll(z => z.Length <= 1); foreach (var O in M)
                {
                    var ª = O.Split(' '); double Z = 100000; bool A = false; bool N = false; bool B =
false; bool C = false; if (ª.Length >= 2) { try { Z = Convert.ToDouble(ª[1]); } catch { Z = 100000; } }
                    if (O.ToLower().Contains("noheading")) A = true
; if (O.ToLower().Contains("nobar")) N = true; if (O.ToLower().Contains("hideempty")) B = true; if (O.ToLower().Contains("singleline"
)) C = true; D += µ(e, w, ª[0], Z, A, N, B, C);
                }
            }
            return D.TrimStart('\n');
        }
        string µ(float e, int w, string À, double Z, bool A = false, bool
N = false, bool B = false, bool C = false)
        {
            string D = ""; bool E = false; foreach (var F in Ȥ)
            {
                if (F.ToString().ToLower().Contains(À.
ToLower()))
                {
                    if (D == "" && !A) { D = "\n" + "Items containing '" + char.ToUpper(À[0]) + À.Substring(1).ToLower() + "'\n\n"; }
                    double G = â(F); if (G ==
0 && B) continue; D += ƪ(e, w, F.SubtypeId.ToString(), G, Z, G.Ų(), Z.Ų(), N, C); E = true;
                }
            }
            if (!E)
            {
                D = "\nError!\n\n"; D +=
"No items containing '" + À + "' found!\nCheck the custom data of this LCD and enter a valid type or item name!\n";
            }
            return D;
        }
        void H()
        {
            if (ľ == 99)
            {
                ľ =
0;
            }
            else { ľ++; }
            Echo("Isy's Inventory Manager " + ɂ[Ƀ] + "\n====================\n"); if (Ģ != null) { Echo("Warning!\n" + Ģ + "\n"); }
            if (Ʉ
.Count == 0)
            {
                Echo("Hint:\nBuild a LCD and add the main LCD\nkeyword '" + mainLCDKeyword +
     "' to its name to get\nmore informations about your base\nand the current script actions.\n");
            }
            string D = ""; D += "Script is running in " + (ǋ ? "station" : "ship") + " mode\n\n"; D += "Task: " + Ǖ[ȍ] + "\n"; D += "Script step: " + ȍ +
" / " + (Ǖ.Length - 1) + "\n\n"; Ţ = D + Ţ; if (Ȯ.Count > 0)
            {
                Ţ += "Excluded grids:\n============\n\n"; foreach (var I in Ȯ)
                {
                    Ţ += I.CustomName + "\n"
;
                }
            }
            Echo(Ţ);
        }
        bool J(IMyTextPanel K, string L)
        {
            Y(K); var M = K.CustomData.Replace(" ", "").Split('\n'); foreach (var O in M)
            {
                if (O.
Contains(L + "=")) { try { return Convert.ToBoolean(O.Replace(L + "=", "")); } catch { return true; } }
            }
            return true;
        }
        void Y(IMyTextPanel K)
        {
            var
M = K.CustomData.Split('\n'); if (M.Length != Ʌ.Length)
            {
                string P = ""; foreach (var F in Ʌ) { P += F + "\n"; }
                K.CustomData = P.TrimEnd('\n')
; K.FontSize = 0.5f;
            }
        }
        double Q(string R, IMyTerminalBlock S, int T, IMyTerminalBlock U, int V, double G = -1, bool W = false)
        {
            var X = S.
GetInventory(T); var º = new List<MyInventoryItem>(); X.GetItems(º); if (º.Count == 0) return 0; var Á = U.GetInventory(V); if ((double)Á.
CurrentVolume > (double)Á.MaxVolume * 0.99) return 0; double ë = 0; MyDefinitionId Ö = new MyDefinitionId(); MyDefinitionId Ø = new MyDefinitionId(
); string Ù = ""; string Ú = ""; bool Û = false; string Ü = ""; if (G == -0.5) Ü = "halfInventory"; if (G == -1) Ü = "completeInventory"; for (var u =
º.Count - 1; u >= 0; u--)
            {
                Ö = º[u].Type; if (W ? Ö.ToString() == R : Ö.ToString().Contains(R))
                {
                    if (Ü != "" && Ö != Ø) ë = 0; Ø = Ö; Ù = Ö.TypeId.ToString
().Replace(Ȝ, ""); Ú = Ö.SubtypeId.ToString(); Û = true; if (!X.CanTransferItemTo(Á, Ö))
                    {
                        Ƣ("'" + Ú +
"' couldn't be transferred\nfrom '" + S.CustomName + "'\nto '" + U.CustomName + "'\nThere's no conveyor line between them or the conveyor type was too small!");
                        return 0;
                    }
                    double Ý = (double)º[u].Amount; double Þ = 0; if (Ü == "completeInventory") { X.TransferItemTo(Á, u, null, true); }
                    else if (Ü ==
"halfInventory") { double ß = Math.Ceiling((double)º[u].Amount / 2); X.TransferItemTo(Á, u, null, true, (VRage.MyFixedPoint)ß); }
                    else
                    {
                        if (!Ù.
Contains(ȝ)) G = Math.Ceiling(G); X.TransferItemTo(Á, u, null, true, (VRage.MyFixedPoint)G);
                    }
                    º.Clear(); X.GetItems(º); try
                    {
                        if ((
MyDefinitionId)º[u].Type == Ö) { Þ = (double)º[u].Amount; }
                    }
                    catch { Þ = 0; }
                    double à = Ý - Þ; ë += à; G -= à; if (G <= 0 && Ü == "") break;
                }
            }
            if (!Û) return 0; if (ë > 0)
            {
                string á = Math.Round(ë, 2) + " " + Ú + " " + Ù; Ɂ = "Moved: " + á + "\nfrom: '" + S.CustomName + "'\nto: '" + U.CustomName + "'";
            }
            else
            {
                string á = Math.
Round(G, 2) + " " + R.Replace(Ȝ, ""); if (Ü == "completeInventory") á = "all items"; if (Ü == "halfInventory") á = "half of the items"; Ƣ(
"Couldn't move '" + á + "'\nfrom '" + S.CustomName + "'\nto '" + U.CustomName + "'\nCheck conveyor connection and owner/faction!");
            }
            return ë;
        }
        double
â(MyDefinitionId Ö, IMyTerminalBlock Ã, int ã = 0)
        { return (double)Ã.GetInventory(ã).GetItemAmount(Ö); ; }
        IMyTerminalBlock ä(
MyDefinitionId Ö, bool å = false, IMyTerminalBlock S = null)
        {
            try { if (Ȼ.GetInventory(0).FindItem(Ö) != null) { return Ȼ; } } catch { }
            foreach (var Ò in
Ȁ)
            {
                if (Ö.SubtypeId.ToString() == "Ice" && Ò.GetType().ToString().Contains("MyGasGenerator")) continue; if (Ò.GetInventory(0).
                    FindItem(Ö) != null) { Ȼ = Ò; return Ò; }
            }
            if (å)
            {
                foreach (var Ò in ȵ)
                {
                    if (S != null) { if (Í(Ò) <= Í(S)) continue; }
                    if (Ò.GetInventory(0).FindItem(Ö)
!= null) { Ȼ = Ò; return Ò; }
                }
            }
            return null;
        }
        IMyTerminalBlock æ(IMyTerminalBlock ç, string è = "")
        {
            List<IMyCargoContainer> é = new List<
IMyCargoContainer>(); GridTerminalSystem.GetBlocksOfType<IMyCargoContainer>(é, ê => ê.CubeGrid == ç.CubeGrid && !ê.CustomName.ToLower().Contains(
ȷ) && !ê.CustomName.ToLower().Contains(ȸ) && !ê.CustomName.Contains(ǎ)); if (è != "") { é.RemoveAll(ê => !ê.CustomName.Contains(è)); }
            if (é.Count == 0)
            {
                GridTerminalSystem.GetBlocksOfType<IMyCargoContainer>(é, ê => !ê.CustomName.ToLower().Contains(ȷ) && !ê.
  CustomName.ToLower().Contains(ȸ) && !ê.CustomName.Contains(ǎ));
            }
            if (é.Count == 0)
            {
                Ƣ("'" + ç.CustomName +
"'\nhas no containers to move its items!"); return null;
            }
            else
            {
                IMyTerminalBlock ì = null; foreach (var Ò in é)
                {
                    if (Ò == ç) continue; var Â = Ò.GetInventory(0); if ((float)Â.
CurrentVolume < (float)Â.MaxVolume * 0.99) { ì = Ò; break; }
                }
                if (ì == null)
                {
                    Ƣ("'" + ç.CustomName +
"'\nhas no empty containers on its grid!\nCan't move its items!"); return null;
                }
                else { return ì; }
            }
        }
        int Í(IMyTerminalBlock Ã)
        {
            string Ä = System.Text.RegularExpressions.Regex.Match(Ã.
CustomName, @"p\d+|pmax|pmin", System.Text.RegularExpressions.RegexOptions.IgnoreCase).Value.ToLower().Replace("p", ""); int Å = 0; bool
Æ = true; if (Ä == "max") { Å = int.MinValue; } else if (Ä == "min") { Å = int.MaxValue; } else { Æ = Int32.TryParse(Ä, out Å); }
            if (!Æ)
            {
                Int32.
TryParse(Ã.EntityId.ToString().Substring(0, 4), out Å);
            }
            return Å;
        }
        void Ç(IMyTerminalBlock Ã, int È)
        {
            string É = System.Text.
RegularExpressions.Regex.Match(Ã.CustomName, @"p\d+|pmax|pmin", System.Text.RegularExpressions.RegexOptions.IgnoreCase).Value; string Ê = ""; if
(È == int.MaxValue) { Ê = "PMax"; }
            else if (È == int.MinValue) { Ê = "PMin"; } else { Ê = "P" + È; }
            if (É == Ê) { return; }
            else if (É != "")
            {
                Ã.CustomName
= Ã.CustomName.Replace(É, Ê);
            }
            else { Ã.CustomName = Ã.CustomName + " " + Ê; }
        }
        string Ë(string L)
        {
            Ð(); var Ì = Storage.Split('\n');
            foreach (var O in Ì) { if (O.Contains(L)) { return O.Replace(L + "=", ""); } }
            return "";
        }
        void Î(string L, string È = "")
        {
            Ð(); var Ì = Storage.
Split('\n'); string Ï = ""; foreach (var O in Ì) { if (O.Contains(L)) { Ï += L + "=" + È + "\n"; } else { Ï += O + "\n"; } }
            Storage = Ï.TrimEnd('\n');
        }
        void
Ð()
        {
            var Ì = Storage.Split('\n'); if (Ì.Length != ȑ.Count)
            {
                string Ï = ""; foreach (var F in ȑ) { Ï += F.Key + "=" + F.Value + "\n"; }
                Storage = Ï.
TrimEnd('\n');
            }
        }
        void Ñ(IMyTerminalBlock Ò)
        {
            foreach (var Ó in Ǌ.Keys.ToList()) { Ǌ[Ó] = "0"; }
            List<string> Ô = Ò.CustomData.Replace(" ",
"").TrimEnd('\n').Split('\n').ToList(); Ô.RemoveAll(O => !O.Contains("=") || O.Length < 8); bool d = false; foreach (var O in Ô)
            {
                var Ŗ
= O.Split('='); if (!Ǌ.ContainsKey(Ŗ[0])) { MyDefinitionId Ö; if (MyDefinitionId.TryParse(Ȝ + Ŗ[0], out Ö)) { Ơ(Ö); d = true; } }
                Ǌ[Ŗ[0]] = Ŗ
[1];
            }
            if (d) ơ(); List<string> Ɲ = new List<string>{"Special Container modes:","",
"Positive number: stores wanted amount, removes excess (e.g.: 100)","Negative number: doesn't store items, only removes excess (e.g.: -100)",
"Keyword 'all': stores all items of that subtype (like a type container)",""}; foreach (var F in Ǌ) { Ɲ.Add(F.Key + "=" + F.Value); }
            Ò.CustomData = string.Join("\n", Ɲ);
        }
        void ƞ()
        {
            ȭ.Clear(); ȭ.AddRange(ȧ); ȭ.
AddRange(Ȩ); ȭ.AddRange(ȩ); ȭ.AddRange(ȫ); ȭ.AddRange(Ȋ); Ǌ.Clear(); foreach (var F in ȧ) { Ǌ[Ȟ + "/" + F] = "0"; }
            foreach (var F in ȥ)
            {
                Ǌ[Ȫ + "/" +
F] = "0";
            }
            foreach (var F in Ȧ) { Ǌ[ȝ + "/" + F] = "0"; }
            foreach (var F in Ȩ) { Ǌ[ȟ + "/" + F] = "0"; }
            foreach (var F in ȩ) { Ǌ[Ƞ + "/" + F] = "0"; }
            foreach (var F in ȫ) { Ǌ[ȡ + "/" + F] = "0"; }
            foreach (var F in Ȋ) { Ǌ[Ȣ + "/" + F] = "0"; }
        }
        void Ɵ()
        {
            foreach (var Â in Ǿ)
            {
                if (Ʊ(75)) return; var º = new
List<MyInventoryItem>(); Â.GetInventory(0).GetItems(º); foreach (var F in º)
                {
                    MyDefinitionId Ö = F.Type; if (Ȥ.Contains(Ö)) continue;
                    string Ù = Ö.TypeId.ToString().Replace(Ȝ, ""); string Ú = Ö.SubtypeId.ToString(); Ɂ = "Found new item!\n" + Ú + " (" + Ù + ")"; if (Ù == Ȫ)
                    {
                        ȥ.Add(Ú
); Ƹ(Ú); if (!Ú.Contains("Ice") && !Ú.Contains("Stone"))
                        {
                            if (ȅ.Count > 0)
                            {
                                if (ȅ[0].GetInventory(0).CanItemsBeAdded(1, Ö))
                                {
                                    if (!Ș.
Contains(Ú)) Ș.Add(Ú); if (Ȇ.Count > 0) { if (Ȇ[0].GetInventory(0).CanItemsBeAdded(1, Ö)) Ț.Add(Ú); }
                                    if (!ș.Contains(Ú) && !Ț.Contains(Ú)) ș.
Add(Ú);
                                }
                            }
                        }
                    }
                    else if (Ù == ȝ) { Ȧ.Add(Ú); }
                    else if (Ù == Ȟ) { ȧ.Add(Ú); }
                    else if (Ù == ȟ) { Ȩ.Add(Ú); }
                    else if (Ù == Ƞ) { ȩ.Add(Ú); }
                    else if (Ù == ȡ)
                    {
                        ȫ.
Add(Ú);
                    }
                    else if (Ù == Ȣ) { Ȋ.Add(Ú); }
                    Ơ(Ö); Ɠ(Ö);
                }
            }
        }
        void Ơ(MyDefinitionId Ö)
        {
            Y(); if (Me.CustomData.Contains(Ö.ToString())) return;
            var M = Me.CustomData.Split('\n').ToList(); M.RemoveAll(O => !O.Contains(";")); M.Add(Ö + ";noBP"); Me.CustomData = String.Join("\n", M
                     ); Ƨ(Ö);
        }
        bool ơ()
        {
            Y(); var M = Me.CustomData.Split('\n'); foreach (var O in M)
            {
                var Ƙ = O.Split(';'); if (Ƙ.Length < 2) continue;
                MyDefinitionId Ö; if (!MyDefinitionId.TryParse(Ƙ[0], out Ö)) continue; MyDefinitionId Ƒ; if (MyDefinitionId.TryParse(Ƙ[1], out Ƒ))
                {
                    if (ȇ.Count
== 0) return false; if (ƒ(Ƒ)) { Ʀ(Ö, Ƒ); } else { ƙ(Ö); continue; }
                }
                string Ù = Ö.TypeId.ToString().Replace(Ȝ, ""); string Ú = Ö.SubtypeId.
ToString(); if (Ù == Ȫ)
                {
                    ȥ.Add(Ú); Ƹ(Ú); if (!Ú.Contains("Ice") && !Ú.Contains("Stone"))
                    {
                        if (ȅ.Count > 0)
                        {
                            if (ȅ[0].GetInventory(0).
CanItemsBeAdded(1, Ö))
                            {
                                if (!Ș.Contains(Ú)) Ș.Add(Ú); if (Ȇ.Count > 0) { if (Ȇ[0].GetInventory(0).CanItemsBeAdded(1, Ö)) Ț.Add(Ú); }
                                if (!ș.Contains(Ú)
&& !Ț.Contains(Ú)) ș.Add(Ú);
                            }
                        }
                    }
                }
                else if (Ù == ȝ) { Ȧ.Add(Ú); }
                else if (Ù == Ȟ) { ȧ.Add(Ú); }
                else if (Ù == ȟ) { Ȩ.Add(Ú); }
                else if (Ù == Ƞ)
                {
                    ȩ.Add(
Ú);
                }
                else if (Ù == ȡ) { ȫ.Add(Ú); } else if (Ù == Ȣ) { Ȋ.Add(Ú); }
                Ƨ(Ö);
            }
            return true;
        }
        void Y()
        {
            if (!Me.CustomData.Contains(Ȑ))
            {
                Me.
CustomData = (ǋ ? ǌ : Ǎ) + Ȑ;
            }
        }
        void Ɯ()
        {
            string Ɛ = "Learning Blueprint.. (" + lockedContainerKeyword + ", " + manualMachineKeyword + ") "; try
            {
                var º =
new List<MyInventoryItem>(); ȕ.GetInventory(1).GetItems(º); var Õ = new List<MyProductionItem>(); ȕ.GetQueue(Õ); ȕ.CustomName = ȕ.
CustomName.Replace(Ɛ, ""); MyDefinitionId Ƒ = Õ[0].BlueprintId; MyDefinitionId Ö = º[0].Type; if (º.Count == 1 && Õ.Count == 1 && ȕ.Mode ==
MyAssemblerMode.Assembly && Ƒ == Ȗ && Ö == ȗ)
                {
                    ȕ.ClearQueue(); ȕ = null; Ȗ = new MyDefinitionId(); ȗ = new MyDefinitionId(); Ɂ = "Learned new Blueprint!\n'"
+ Ƒ.ToString().Replace(Ȝ, "") + "'\nproduces: '" + Ö.ToString().Replace(Ȝ, "") + "'"; Ʀ(Ö, Ƒ); Ɨ(Ö, Ƒ); return;
                }
            }
            catch { }
            ȕ = null; Ȗ = new
MyDefinitionId(); ȗ = new MyDefinitionId(); foreach (var ĉ in ȇ)
            {
                var º = new List<MyInventoryItem>(); ĉ.GetInventory(1).GetItems(º); var Õ = new
List<MyProductionItem>(); ĉ.GetQueue(Õ); if (º.Count == 1 && Õ.Count == 1 && ĉ.Mode == MyAssemblerMode.Assembly)
                {
                    MyDefinitionId Ƒ = Õ[0].
BlueprintId; if (ƥ(Ƒ)) continue; MyDefinitionId Ö = º[0].Type; if (!Ȥ.Contains(Ö)) continue; ĉ.CustomName = Ɛ + ĉ.CustomName; ȕ = ĉ; Ȗ = Ƒ; ȗ = Ö; return;
                }
            }
        }
        bool ƒ(MyDefinitionId Ƒ)
        {
            try { foreach (var ĉ in ȇ) { if (ĉ.CanUseBlueprint(Ƒ)) return true; } } catch { return false; }
            return false
;
        }
        void Ɠ(MyDefinitionId Ö)
        {
            if (ȇ.Count == 0) return; if (Ö.TypeId.ToString() == Ȝ + Ȫ || Ö.TypeId.ToString() == Ȝ + ȝ) return;
            MyDefinitionId Ƒ; bool ƚ = Ǉ.TryGetValue(Ö, out Ƒ); if (ƚ) ƚ = ƒ(Ƒ); if (!ƚ)
            {
                string[] Ɣ = { "", "Component", "Magazine" }; bool ƕ = false; for (int u = 0; u < Ɣ.
Length; u++)
                {
                    string Ɩ = ȣ + Ö.SubtypeId.ToString().Replace("Item", "") + Ɣ[u]; MyDefinitionId.TryParse(Ɩ, out Ƒ); ƕ = ƒ(Ƒ); if (ƕ)
                    {
                        Ʀ(Ö, Ƒ); Ɨ(Ö
, Ƒ); ƚ = true; return;
                    }
                }
            }
        }
        void Ɨ(MyDefinitionId Ö, MyDefinitionId Ƒ)
        {
            Y(); var M = Me.CustomData.Split('\n'); for (var u = 0; u < M.
Length; u++)
            {
                if (!M[u].Contains(Ö.ToString())) continue; var Ƙ = M[u].Split(';'); M[u] = Ƙ[0] + ";" + Ƒ.ToString(); Me.CustomData = String.
                 Join("\n", M); return;
            }
        }
        void ƙ(MyDefinitionId Ö)
        {
            Y(); var M = Me.CustomData.Split('\n').ToList(); M.RemoveAll(u => u.Contains(Ö.
ToString())); Me.CustomData = String.Join("\n", M);
        }
        void Ƣ(string ļ) { Ɇ.Add(ļ); ɇ.Add(ļ); Ģ = Ɇ.ElementAt(0); }
        void ư()
        {
            Me.CustomData = "";
            foreach (var Ò in ȵ)
            {
                List<string> M = Ò.CustomData.Replace(" ", "").TrimEnd('\n').Split('\n').ToList(); M.RemoveAll(O => !O.Contains(
 "=") || O.Contains("=0")); Ò.CustomData = string.Join("\n", M);
            }
            Echo("Stored items deleted!\n"); if (ȵ.Count > 0) Echo(
"Also deleted itemlists of " + ȵ.Count + " Special containers!\n"); Echo("Please hit 'Recompile'!\n\nScript stopped!");
        }
        bool Ʊ(double ē)
        {
            return Runtime.
CurrentInstructionCount > Runtime.MaxInstructionCount * (ē / 100);
        }
        void Ʋ()
        {
            ǽ.Clear(); List<IMyTerminalBlock> Ƴ = ȇ.ToList<IMyTerminalBlock>(); List<
IMyTerminalBlock> ƴ = ȃ.ToList<IMyTerminalBlock>(); Ƶ(Ǿ, 0); Ƶ(Ƴ, 1); Ƶ(ƴ, 1);
        }
        void Ƶ(List<IMyTerminalBlock> ƶ, int ã)
        {
            foreach (var Ò in ƶ)
            {
                var º =
new List<MyInventoryItem>(); Ò.GetInventory(ã).GetItems(º); foreach (var F in º)
                {
                    MyDefinitionId Ö = F.Type; if (ǽ.ContainsKey(Ö))
                    {
                        ǽ[Ö] += (double)F.Amount;
                    }
                    else { ǽ[Ö] = (double)F.Amount; bool Ʒ; ƨ(Ö, out Ʒ); }
                }
            }
        }
        double â(MyDefinitionId Ö)
        {
            double ƣ; ǽ.
TryGetValue(Ö, out ƣ); return ƣ;
        }
        void Ƹ(string ƹ) { if (!oreYieldDict.ContainsKey(ƹ)) { oreYieldDict[ƹ] = 1; } }
        double ƺ(string ƹ)
        {
            double ƣ; ƹ =
ƹ.Replace(Ȝ + Ȫ + "/", ""); oreYieldDict.TryGetValue(ƹ, out ƣ); return ƣ != 0 ? ƣ : 1;
        }
        void ƻ()
        {
            Ǆ.Clear(); foreach (IMyAssembler ĉ in ȇ)
            {
                var Õ = new List<MyProductionItem>(); ĉ.GetQueue(Õ); if (Õ.Count > 0 && !ĉ.IsProducing)
                {
                    Ƣ("'" + ĉ.CustomName +
"' has a queue but is currently not producing!");
                }
                foreach (var F in Õ)
                {
                    MyDefinitionId Ƒ = F.BlueprintId; if (Ǆ.ContainsKey(Ƒ)) { Ǆ[Ƒ] += (double)F.Amount; }
                    else
                    {
                        Ǆ[Ƒ] = (double)F.
Amount;
                    }
                }
            }
        }
        double Ƽ(MyDefinitionId Ƒ) { double ƣ; Ǆ.TryGetValue(Ƒ, out ƣ); return ƣ; }
        MyDefinitionId ƨ(MyDefinitionId Ö, out bool ƚ)
        {
            MyDefinitionId Ƒ; ƚ = Ǉ.TryGetValue(Ö, out Ƒ); return Ƒ;
        }
        MyDefinitionId Ƥ(MyDefinitionId Ƒ)
        {
            MyDefinitionId Ö; ǈ.TryGetValue(Ƒ, out Ö); return
Ö;
        }
        bool ƥ(MyDefinitionId Ƒ) { return ǈ.ContainsKey(Ƒ); }
        void Ʀ(MyDefinitionId Ö, MyDefinitionId Ƒ) { Ǉ[Ö] = Ƒ; ǈ[Ƒ] = Ö; }
        void Ƨ(
MyDefinitionId Ö)
        { Ȥ.Add(Ö); ǉ[Ö.SubtypeId.ToString()] = Ö; }
        MyDefinitionId Ʃ(string Ú)
        {
            MyDefinitionId Ö = new MyDefinitionId(); ǉ.TryGetValue
(Ú, out Ö); return Ö;
        }
        string ƪ(float e, int w, string ķ, double È, double ƫ, string Ƭ = null, string ƭ = null, bool N = false, bool Ʈ =
false)
        {
            string ĸ = È.ToString(); string Ĺ = ƫ.ToString(); if (Ƭ != null) { ĸ = Ƭ; }
            if (ƭ != null) { Ĺ = ƭ; }
            char Ư = '['; char ƛ = ']'; char Ə = 'I'; char Ŕ =
'.'; float ŗ = ũ(' ', e); string Ř = È.ų(ƫ); Ř = " " + ' '.ŭ((int)((ƍ("99999.9%", e) - ƍ(Ř, e)) / ŗ)) + Ř; string ř = ĸ + " / " + Ĺ; double Ś = 0; if (ƫ > 0)
                Ś = È / ƫ >= 1 ? 1 : È / ƫ; string ś = ķ + " "; string Ŝ = ""; if (Ʈ && !N)
            {
                if (e <= 0.5 || (e <= 1 && w > 650))
                {
                    float ŝ = ƍ(ś, e) + ƍ(ĸ + " /", e); string Ş = ' '.ŭ((
int)((w * 0.4 - ŝ) / ŗ)); ś += Ş + ř; Ş = ' '.ŭ((int)((w * 0.6 - ƍ(ś, e)) / ŗ)); ś += Ş; float ş = ƍ(ś, e) + ũ(Ư, e) + ũ(ƛ, e) + ƍ(Ř, e); string Š = Ə.ŭ((int)((w - ş
) * Ś / ũ(Ə, e))); string š = Ŕ.ŭ((int)((w - ş - ƍ(Š, e)) / ũ(Ŕ, e))); ś += Ư + Š + š + ƛ + Ř;
                }
                else
                {
                    string Ş = ' '.ŭ((int)((w * 0.5 - ƍ(ś, e)) / ŗ)); ś += Ş;
                    float ş = ƍ(ś, e) + ũ(Ư, e) + ũ(ƛ, e) + ƍ(Ř, e); string Š = Ə.ŭ((int)((w - ş) * Ś / ũ(Ə, e))); string š = Ŕ.ŭ((int)((w - ş - ƍ(Š, e)) / ũ(Ŕ, e))); ś += Ư + Š + š + ƛ + Ř
                                                      ;
                }
            }
            else
            {
                if (e <= 0.6 || (e <= 1 && w > 650))
                {
                    float ŝ = ƍ(ś, e) + ƍ(ĸ + " /", e); string Ş = ' '.ŭ((int)((w * 0.5 - ŝ) / ŗ)); ś += Ş + ř; ŝ = ƍ(ś, e) + ƍ(Ř, e); Ş =
                ' '.ŭ((int)((w - ŝ) / ŗ)); ś += Ş + Ř; if (!N)
                    {
                        float ş = ũ(Ư, e) + ũ(ƛ, e); string Š = Ə.ŭ((int)((w - ş) * Ś / ũ(Ə, e))); string š = Ŕ.ŭ((int)((w - ş - ƍ(Š, e
     )) / ũ(Ŕ, e))); Ŝ = Ư + Š + š + ƛ + "\n";
                    }
                }
                else
                {
                    float ŝ = ƍ(ś, e) + ƍ(ř, e); string Ş = ' '.ŭ((int)((w - ŝ) / ŗ)); ś += Ş + ř; if (!N)
                    {
                        float ş = ũ(Ư, e) + ũ(ƛ, e
) + ƍ(Ř, e); string Š = Ə.ŭ((int)((w - ş) * Ś / ũ(Ə, e))); string š = Ŕ.ŭ((int)((w - ş - ƍ(Š, e)) / ũ(Ŕ, e))); Ŝ = Ư + Š + š + ƛ + Ř + "\n";
                    }
                }
            }
            return ś + "\n" + Ŝ
;
        }
        string Ţ = "Performance information is generated.."; Dictionary<string, int> ţ = new Dictionary<string, int>(); List<int> Ť = new
               List<int>(new int[100]); List<double> ť = new List<double>(new double[100]); double Ŧ, ŕ; int ľ = 0; DateTime ŉ; void Ŀ(string ŀ, bool Ł
                          = false)
        {
            if (Ł) { ŉ = DateTime.Now; return; }
            ľ = ľ >= 99 ? 0 : ľ + 1; Ţ = ""; int ł = Runtime.CurrentInstructionCount; if (ł > Ŧ) Ŧ = ł; Ť[ľ] = ł; double Ń =
  Ť.Sum() / Ť.Count; Ţ += "Instructions: " + ł + " / " + Runtime.MaxInstructionCount + "\n"; Ţ += "Max. Instructions: " + Ŧ + " / " + Runtime.
                MaxInstructionCount + "\n"; Ţ += "Avg. Instructions: " + Math.Floor(Ń) + " / " + Runtime.MaxInstructionCount + "\n\n"; double ń = (DateTime.Now - ŉ).
                              TotalMilliseconds; if (ń > ŕ) ŕ = ń; ť[ľ] = ń; double Ņ = ť.Sum() / ť.Count; Ţ += "Last runtime: " + Math.Round(ń, 4) + " ms\n"; Ţ += "Max. runtime: " + Math.Round(ŕ
                                                      , 4) + " ms\n"; Ţ += "Avg. runtime: " + Math.Round(Ņ, 4) + " ms\n\n"; Ţ += "Instructions per Method:\n"; ţ[ŀ] = ł; foreach (var F in ţ.
                                                                        OrderByDescending(u => u.Value)) { Ţ += "- " + F.Key + ": " + F.Value + "\n"; }
            Ţ += "\n";
        }
        DateTime ņ = DateTime.Now; Dictionary<long, List<int>> Ň = new
Dictionary<long, List<int>>(); string ň(float e, string ļ, IMyTextPanel K, int Ŋ = 3, bool ŋ = true, int Ō = 1)
        {
            long ō = K.EntityId; if (!Ň.
ContainsKey(ō)) { Ň[ō] = new List<int> { 1, 3, Ŋ, 0 }; }
            int Ŏ = Ň[ō][0]; int ŏ = Ň[ō][1]; int Ő = Ň[ō][2]; int ő = Ň[ō][3]; var Œ = ļ.TrimEnd('\n').Split(
'\n'); List<string> h = new List<string>(); int œ = (int)Math.Ceiling(17 / e * Ō); int w = K.BlockDefinition.SubtypeName.Contains("Wide") ?
1300 : 650; string ğ = ""; if (K.BlockDefinition.SubtypeName.Contains("Corner"))
            {
                if (K.CubeGrid.GridSize == 0.5)
                {
                    œ = (int)Math.Floor(5 / e
);
                }
                else { œ = (int)Math.Floor(3 / e); }
            }
            foreach (var O in Œ)
            {
                if (ƍ(O, e) <= w) { h.Add(O); }
                else
                {
                    try
                    {
                        string ª = ""; float Ǝ = 0; float ŗ = ũ(' '
, e); var ź = O.Split(' '); string Ż = System.Text.RegularExpressions.Regex.Match(O, @".+(\.|\:)\ ").Value; string ż = ' '.ŭ((int)
Math.Floor(ƍ(Ż, e) / ŗ)); foreach (var Ž in ź) { float ž = ƍ(Ž, e); if (Ǝ + ž > w) { h.Add(ª); ª = ż + Ž + " "; Ǝ = ƍ(ª, e); } else { ª += Ž + " "; Ǝ += ž + ŗ; } }
                        h.Add
(ª);
                    }
                    catch { h.Add(O); }
                }
            }
            if (ŋ)
            {
                if (h.Count > œ)
                {
                    if (DateTime.Now.Second != ő)
                    {
                        ő = DateTime.Now.Second; if (ŏ > 0) ŏ--; if (ŏ <= 0) Ő += Ŏ; if (Ő +
œ - Ŋ >= h.Count && ŏ <= 0) { Ŏ = -1; ŏ = 3; }
                        if (Ő <= Ŋ && ŏ <= 0) { Ŏ = 1; ŏ = 3; }
                    }
                }
                else { Ő = Ŋ; Ŏ = 1; ŏ = 3; }
                Ň[ō][0] = Ŏ; Ň[ō][1] = ŏ; Ň[ō][2] = Ő; Ň[ō][3] = ő;
            }
            else
            {
                Ő
= Ŋ;
            }
            for (var O = 0; O < Ŋ; O++) { ğ += h[O] + "\n"; }
            for (var O = Ő; O < h.Count; O++) { ğ += h[O] + "\n"; }
            return ğ;
        }
        IMyCubeGrid ſ = null; HashSet<
IMyCubeGrid> ƀ = new HashSet<IMyCubeGrid>(); void Ɓ(IMyCubeGrid Ƃ)
        {
            ƀ.Add(Ƃ); List<IMyMotorStator> ƃ = new List<IMyMotorStator>(); List<
IMyPistonBase> Ƅ = new List<IMyPistonBase>(); GridTerminalSystem.GetBlocksOfType<IMyMotorStator>(ƃ, ƅ => ƅ.IsAttached && ƅ.TopGrid == Ƃ && !ƀ.
Contains(ƅ.CubeGrid)); GridTerminalSystem.GetBlocksOfType<IMyPistonBase>(Ƅ, Ɔ => Ɔ.IsAttached && Ɔ.TopGrid == Ƃ && !ƀ.Contains(Ɔ.CubeGrid)
); if (ƃ.Count == 0 && Ƅ.Count == 0) { ſ = Ƃ; return; } else { foreach (var Ƈ in ƃ) { Ɓ(Ƈ.CubeGrid); } foreach (var ƈ in Ƅ) { Ɓ(ƈ.CubeGrid); } }
        }
        HashSet<IMyCubeGrid> Ɖ = new HashSet<IMyCubeGrid>(); void Ɗ(IMyCubeGrid Ƃ, bool Ƌ = false)
        {
            if (Ƌ) Ɖ.Clear(); Ɖ.Add(Ƃ); List<IMyMotorStator
> ƃ = new List<IMyMotorStator>(); List<IMyPistonBase> Ƅ = new List<IMyPistonBase>(); GridTerminalSystem.GetBlocksOfType<
IMyMotorStator>(ƃ, ƅ => ƅ.CubeGrid == Ƃ && ƅ.IsAttached && !Ɖ.Contains(ƅ.TopGrid)); GridTerminalSystem.GetBlocksOfType<IMyPistonBase>(Ƅ, Ɔ => Ɔ.
CubeGrid == Ƃ && Ɔ.IsAttached && !Ɖ.Contains(Ɔ.TopGrid)); foreach (var Ƈ in ƃ) { Ɗ(Ƈ.TopGrid); }
            foreach (var ƈ in Ƅ) { Ɗ(ƈ.TopGrid); }
        }
        bool ƌ =
false; float ƍ(string Ž, float e = 1)
        {
            if (ƌ) return Ž.Length * 25 * e; int ŧ = 0; int Ū = 0; foreach (var Ũ in Ž)
            {
                ū.TryGetValue(Ũ, out Ū); ŧ += Ū + 1
;
            }
            return ŧ * e;
        }
        float ũ(char Ũ, float e = 1) { if (ƌ) return 25 * e; int Ū = 0; ū.TryGetValue(Ũ, out Ū); return (Ū + 1) * e; }
        Dictionary<char,
int> ū = new Dictionary<char, int>(){{' ',8},{'!',8},{'"',10},{'#',19},{'$',20},{'%',24},{'&',20},{'\'',6},{'(',9},{')',9},{'*'
,11},{'+',18},{',',9},{'-',10},{'.',9},{'/',14},{'0',19},{'1',9},{'2',19},{'3',17},{'4',19},{'5',19},{'6',19},{'7',16},{
'8',19},{'9',19},{':',9},{';',9},{'<',18},{'=',18},{'>',18},{'?',16},{'@',25},{'A',21},{'B',21},{'C',19},{'D',21},{'E',18},
{'F',17},{'G',20},{'H',20},{'I',8},{'J',16},{'K',17},{'L',15},{'M',26},{'N',21},{'O',21},{'P',20},{'Q',21},{'R',21},{'S',
21},{'T',17},{'U',20},{'V',20},{'W',31},{'X',19},{'Y',20},{'Z',19},{'[',9},{'\\',12},{']',9},{'^',18},{'_',15},{'`',8},{
'a',17},{'b',17},{'c',16},{'d',17},{'e',17},{'f',9},{'g',17},{'h',17},{'i',8},{'j',8},{'k',17},{'l',8},{'m',27},{'n',17},{
'o',17},{'p',17},{'q',17},{'r',10},{'s',17},{'t',9},{'u',17},{'v',15},{'w',27},{'x',15},{'y',17},{'z',16},{'{',9},{'|',6},{
'}',9},{'~',18},{' ',8},{'¡',8},{'¢',16},{'£',17},{'¤',19},{'¥',19},{'¦',6},{'§',20},{'¨',8},{'©',25},{'ª',10},{'«',15},{
'¬',18},{'­',10},{'®',25},{'¯',8},{'°',12},{'±',18},{'²',11},{'³',11},{'´',8},{'µ',17},{'¶',18},{'·',9},{'¸',8},{'¹',11},{
'º',10},{'»',15},{'¼',27},{'½',29},{'¾',28},{'¿',16},{'À',21},{'Á',21},{'Â',21},{'Ã',21},{'Ä',21},{'Å',21},{'Æ',31},{'Ç',19
},{'È',18},{'É',18},{'Ê',18},{'Ë',18},{'Ì',8},{'Í',8},{'Î',8},{'Ï',8},{'Ð',21},{'Ñ',21},{'Ò',21},{'Ó',21},{'Ô',21},{'Õ',
21},{'Ö',21},{'×',18},{'Ø',21},{'Ù',20},{'Ú',20},{'Û',20},{'Ü',20},{'Ý',17},{'Þ',20},{'ß',19},{'à',17},{'á',17},{'â',17},{
'ã',17},{'ä',17},{'å',17},{'æ',28},{'ç',16},{'è',17},{'é',17},{'ê',17},{'ë',17},{'ì',8},{'í',8},{'î',8},{'ï',8},{'ð',17},{
'ñ',17},{'ò',17},{'ó',17},{'ô',17},{'õ',17},{'ö',17},{'÷',18},{'ø',17},{'ù',17},{'ú',17},{'û',17},{'ü',17},{'ý',17},{'þ',17
},{'ÿ',17},{'Ā',20},{'ā',17},{'Ă',21},{'ă',17},{'Ą',21},{'ą',17},{'Ć',19},{'ć',16},{'Ĉ',19},{'ĉ',16},{'Ċ',19},{'ċ',16},{
'Č',19},{'č',16},{'Ď',21},{'ď',17},{'Đ',21},{'đ',17},{'Ē',18},{'ē',17},{'Ĕ',18},{'ĕ',17},{'Ė',18},{'ė',17},{'Ę',18},{'ę',17
},{'Ě',18},{'ě',17},{'Ĝ',20},{'ĝ',17},{'Ğ',20},{'ğ',17},{'Ġ',20},{'ġ',17},{'Ģ',20},{'ģ',17},{'Ĥ',20},{'ĥ',17},{'Ħ',20},{
'ħ',17},{'Ĩ',8},{'ĩ',8},{'Ī',8},{'ī',8},{'Į',8},{'į',8},{'İ',8},{'ı',8},{'Ĳ',24},{'ĳ',14},{'Ĵ',16},{'ĵ',8},{'Ķ',17},{'ķ',17
},{'Ĺ',15},{'ĺ',8},{'Ļ',15},{'ļ',8},{'Ľ',15},{'ľ',8},{'Ŀ',15},{'ŀ',10},{'Ł',15},{'ł',8},{'Ń',21},{'ń',17},{'Ņ',21},{'ņ',
17},{'Ň',21},{'ň',17},{'ŉ',17},{'Ō',21},{'ō',17},{'Ŏ',21},{'ŏ',17},{'Ő',21},{'ő',17},{'Œ',31},{'œ',28},{'Ŕ',21},{'ŕ',10},{
'Ŗ',21},{'ŗ',10},{'Ř',21},{'ř',10},{'Ś',21},{'ś',17},{'Ŝ',21},{'ŝ',17},{'Ş',21},{'ş',17},{'Š',21},{'š',17},{'Ţ',17},{'ţ',9}
,{'Ť',17},{'ť',9},{'Ŧ',17},{'ŧ',9},{'Ũ',20},{'ũ',17},{'Ū',20},{'ū',17},{'Ŭ',20},{'ŭ',17},{'Ů',20},{'ů',17},{'Ű',20},{'ű',
17},{'Ų',20},{'ų',17},{'Ŵ',31},{'ŵ',27},{'Ŷ',17},{'ŷ',17},{'Ÿ',17},{'Ź',19},{'ź',16},{'Ż',19},{'ż',16},{'Ž',19},{'ž',16},{
'ƒ',19},{'Ș',21},{'ș',17},{'Ț',17},{'ț',9},{'ˆ',8},{'ˇ',8},{'ˉ',6},{'˘',8},{'˙',8},{'˚',8},{'˛',8},{'˜',8},{'˝',8},{'Ё',19}
,{'Ѓ',16},{'Є',18},{'Ѕ',21},{'І',8},{'Ї',8},{'Ј',16},{'Љ',28},{'Њ',21},{'Ќ',19},{'Ў',17},{'Џ',18},{'А',19},{'Б',19},{'В',
19},{'Г',15},{'Д',19},{'Е',18},{'Ж',21},{'З',17},{'И',19},{'Й',19},{'К',17},{'Л',17},{'М',26},{'Н',18},{'О',20},{'П',19},{
'Р',19},{'С',19},{'Т',19},{'У',19},{'Ф',20},{'Х',19},{'Ц',20},{'Ч',16},{'Ш',26},{'Щ',29},{'Ъ',20},{'Ы',24},{'Ь',19},{'Э',18
},{'Ю',27},{'Я',20},{'а',16},{'б',17},{'в',16},{'г',15},{'д',17},{'е',17},{'ж',20},{'з',15},{'и',16},{'й',16},{'к',17},{
'л',15},{'м',25},{'н',16},{'о',16},{'п',16},{'р',17},{'с',16},{'т',14},{'у',17},{'ф',21},{'х',15},{'ц',17},{'ч',15},{'ш',25
},{'щ',27},{'ъ',16},{'ы',20},{'ь',16},{'э',14},{'ю',23},{'я',17},{'ё',17},{'ђ',17},{'ѓ',16},{'є',14},{'ѕ',16},{'і',8},{
'ї',8},{'ј',7},{'љ',22},{'њ',25},{'ћ',17},{'ќ',16},{'ў',17},{'џ',17},{'Ґ',15},{'ґ',13},{'–',15},{'—',31},{'‘',6},{'’',6},{
'‚',6},{'“',12},{'”',12},{'„',12},{'†',20},{'‡',20},{'•',15},{'…',31},{'‰',31},{'‹',8},{'›',8},{'€',19},{'™',30},{'−',18},{
'∙',8},};
    }
    static partial class Ŭ { public static string ŭ(this char Ů, int ů) { if (ů <= 0) { return ""; } return new string(Ů, ů); } }
    static
partial class Ŭ
    {
        public static string Ű(this double È)
        {
            string ű = "kL"; if (È < 1) { È *= 1000; ű = "L"; }
            else if (È >= 1000 && È < 1000000)
            {
                È /= 1000;
                ű = "ML";
            }
            else if (È >= 1000000 && È < 1000000000) { È /= 1000000; ű = "BL"; } else if (È >= 1000000000) { È /= 1000000000; ű = "TL"; }
            return Math.
Round(È, 1) + " " + ű;
        }
    }
    static partial class Ŭ
    {
        public static string Ų(this double È)
        {
            string ű = ""; if (È >= 1000 && È < 1000000)
            {
                È /= 1000; ű =
" k";
            }
            else if (È >= 1000000 && È < 1000000000) { È /= 1000000; ű = " M"; } else if (È >= 1000000000) { È /= 1000000000; ű = " B"; }
            return Math.Round(È,
1) + ű;
        }
    }
    static partial class Ŭ
    {
        public static string ų(this double Ŵ, double ŵ)
        {
            double ē = Math.Round(Ŵ / ŵ * 100, 1); if (ŵ == 0)
            {
                return "0%";
            }
            else { return ē + "%"; }
        }
    }
    class Ŷ : IComparer<MyDefinitionId>
    {
        public int Compare(MyDefinitionId ŷ, MyDefinitionId Ÿ)
        {
            return ŷ.ToString().CompareTo(Ÿ.ToString());
        }
    }
    class Ź : IEqualityComparer<MyInventoryItem>
    {
        public bool Equals(MyInventoryItem ŷ,
MyInventoryItem Ÿ)
        { return ŷ.ToString() == Ÿ.ToString(); }
        public int GetHashCode(MyInventoryItem F) { return F.ToString().GetHashCode(); }
    }
}
