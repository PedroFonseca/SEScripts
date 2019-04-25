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

namespace SEScripts.Scripts
{
    #region Usings

    using GridCargo = Modules.GridCargo;
    using LcdOutputHelper = Helpers.LcdOutputHelper;
    using GridBlocksHelper = Helpers.GridBlocksHelper;
    using InventoryHelper = Helpers.InventoryHelper;
    using CargoHelper = Helpers.CargoHelper;

    #endregion Usings
    public partial class Program : Skeleton
    {
        #region SpaceEngineers

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
        }

        #endregion
    }
}
