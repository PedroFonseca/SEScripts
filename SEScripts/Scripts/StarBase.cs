using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game.ModAPI.Ingame;
using VRageMath;

namespace SEScripts.Scripts
{
    #region Usings

    using ShowContainerContents = Modules.ShowContainerContents;

    #endregion Usings

    public class StarBase : Skeleton
    {
        #region SpaceEngineers

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

        #endregion SpaceEngineers
    }
}