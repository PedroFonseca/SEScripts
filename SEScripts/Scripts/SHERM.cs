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
    #region Usings

    using ShowContainerContents = Modules.ShowContainerContents;
    using GridBlocksHelper = Helpers.GridBlocksHelper;
    using LcdOutputHelper = Helpers.LcdOutputHelper;

    #endregion Usings

    public class SHERM : Skeleton
    {
        #region SpaceEngineers

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

        #endregion SpaceEngineers
    }
}