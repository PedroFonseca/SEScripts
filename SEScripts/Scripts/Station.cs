using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System;
using VRage;
using VRage.Collections;
using VRage.Game.Components;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game;
using VRageMath;
using SEScripts.Helpers;
using VRage.Scripting;
using VRage.Game.GUI.TextPanel;

namespace SEScripts.Station
{
    #region Usings
    using GridBlocksHelper = Helpers.GridBlocksHelper;
    using LcdOutputHelper = Helpers.LcdOutputHelper;
    using AutoBuildComponents = Modules.AutoBuildComponents;

    #endregion Usings
    public partial class Program : Skeleton
    {
        #region SpaceEngineers

        public string gridPrefix = "{Station}";
        public string lcdName = "{Station} LCD Ucat";
        public static Dictionary<string, int> componentDesiredQuantities = new Dictionary<string, int>
        {
            { "SteelPlate",       10000 },
            { "InteriorPlate",     5000 },
            { "Motor",             1000 },
            { "Construction",      1000 },
            { "SmallTube",         1000 },
            { "LargeTube",         500 },
            { "Computer",          500 },
            { "MetalGrid",         200 },
            { "BulletproofGlass",   30 },
            { "Display",            20 },
            { "Girder",             10 },
            { "Detector",           20 },
            { "Explosives",          0 },
            { "GravityGenerator",    0 },
            { "Medical",             0 },
            { "PowerCell",           100 },
            { "RadioCommunication",  0 },
            { "Reactor",             0 },
            { "SolarCell",           0 },
            { "SuperConductor",      0 },
            { "Thrust",              0 }
        };
        // Timmer is used to show something different every iteration
        public static int t = 0;
        // 
        public readonly string[] timerChar = new string[] { "\\", "|", "/", "-" };

        public Program()
        {
            Echo("Script ready to be launched..\n");
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
        }

        public void Main(string argument, UpdateType updateSource)
        {
            // Move timmer
            t = t == 3 ? 0 : t + 1;
            var tc = timerChar[t];

            Echo("Running Ucat manager " + tc);
            IMyTextSurface mesurface0 = Me.GetSurface(0);
            mesurface0.ContentType = ContentType.TEXT_AND_IMAGE;
            mesurface0.FontSize = 2;
            mesurface0.Alignment = VRage.Game.GUI.TextPanel.TextAlignment.LEFT;
            mesurface0.WriteText("Ucat scripting " + tc+'\n');


            // main logic
            var autoBuilderDebug = AutoBuildComponents.Get(GridTerminalSystem).BuildComponentsToQuota(gridPrefix, componentDesiredQuantities, "{Station} MainAssembler");
            mesurface0.WriteText("Auto Builder: " + (autoBuilderDebug.Length == 0 ? "OK": "NOK")+'\n', true);

            // Debug panel
            var debugLcd = GridBlocksHelper.Get(GridTerminalSystem).GetBlocksOfTypeByName<IMyTextPanel>(lcdName).First();
            LcdOutputHelper.ShowMessageOnLcd(debugLcd, new LcdMessage("Running Ucat manager " + tc, Color.Blue));
            if (autoBuilderDebug != null && autoBuilderDebug.Length > 0)
            {
                LcdOutputHelper.ShowMessageOnLcd(debugLcd, new LcdMessage("Auto Builder result: \n" + autoBuilderDebug, Color.Red), true);
            }
        }

        #endregion
    }
}
