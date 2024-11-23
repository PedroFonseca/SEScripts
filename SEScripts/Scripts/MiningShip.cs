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
using VRage.Scripting;
using VRage.Game.GUI.TextPanel;

namespace SEScripts.MiningShip
{
    #region Usings
    using GridBlocksHelper = Helpers.GridBlocksHelper;
    using CargoHelper = Helpers.CargoHelper;
    using InventoryHelper = Helpers.InventoryHelper;

    #endregion Usings
    public partial class Program : Skeleton
    {
        #region SpaceEngineers

        public string gridPrefix = "UCATM1>";
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
            var cockpit = GridBlocksHelper.Get(GridTerminalSystem).GetBlocksOfTypeByName<IMyCockpit>(gridPrefix).FirstOrDefault();
            var textOutput = cockpit?.GetSurface(3) ?? Me.GetSurface(0);
            textOutput.ContentType = ContentType.TEXT_AND_IMAGE;
            textOutput.FontSize = 2;
            textOutput.Alignment = VRage.Game.GUI.TextPanel.TextAlignment.LEFT;
            textOutput.WriteText("Calculating " + tc + '\n');

            IMyTextSurface mesurface0 = Me.GetSurface(0);
            mesurface0.ContentType = ContentType.TEXT_AND_IMAGE;
            mesurface0.FontSize = 2;
            mesurface0.Alignment = VRage.Game.GUI.TextPanel.TextAlignment.LEFT;
            mesurface0.WriteText("Calculating " + tc + '\n');

            // Get all containers on the ship
            var gridContainers = GridBlocksHelper.Get(GridTerminalSystem).GetCargoContainers(gridPrefix);
            var containerInventories = gridContainers.Select(container => InventoryHelper.GetInventories(container).First()).ToList();
            var inventoryAggregation = containerInventories.Select(inv => new InventoryCargo(inv)).Aggregate(new InventoryCargo((decimal)0, (decimal)0, (decimal)0), 
                (InventoryCargo agg, InventoryCargo next) => new InventoryCargo(agg.CurrentVolume + next.CurrentVolume, agg.MaxVolume + next.MaxVolume, agg.CurrentMass + next.CurrentMass));
            var percentageFull = inventoryAggregation.CurrentVolume / inventoryAggregation.MaxVolume * 100;

            textOutput.WriteText(Math.Round(percentageFull, 0) + "% Full\n", true);
            textOutput.WriteText("Current Mass " + Math.Round(inventoryAggregation.CurrentMass/1000, 2) + "t \n", true);
            mesurface0.WriteText(Math.Round(percentageFull, 0) + "% Full\n", true);
            mesurface0.WriteText("Current Mass " + Math.Round(inventoryAggregation.CurrentMass / 1000, 2) + "t \n", true);


            IMyTextSurface mesurface1 = Me.GetSurface(1);
            mesurface1.ContentType = ContentType.TEXT_AND_IMAGE;
            mesurface1.FontSize = 6;
            mesurface1.Alignment = VRage.Game.GUI.TextPanel.TextAlignment.CENTER;
            mesurface1.WriteText("\n");
            mesurface1.WriteText(Math.Round(percentageFull, 0) + "% Full\n", true);
            if (percentageFull < (decimal)50)
            {
                textOutput.BackgroundColor = Color.Green;
                mesurface1.BackgroundColor = Color.Green;
            }
            else if (percentageFull < (decimal)90)
            {
                textOutput.BackgroundColor = Color.Orange;
                mesurface1.BackgroundColor = Color.Orange;
            }
            else
            {
                textOutput.BackgroundColor = Color.Red;
                mesurface1.BackgroundColor = Color.Red;
            }
        }

        public struct InventoryCargo
        {
            public decimal CurrentVolume;
            public decimal MaxVolume;
            public decimal CurrentMass;

            public InventoryCargo(decimal CurrentVolume, decimal MaxVolume, decimal CurrentMass)
            {
                this.CurrentVolume = CurrentVolume;
                this.MaxVolume = MaxVolume;
                this.CurrentMass = CurrentMass;
            }
            public InventoryCargo(MyFixedPoint CurrentVolume, MyFixedPoint MaxVolume, MyFixedPoint CurrentMass) {
                this.CurrentVolume = CargoHelper.ConvertFromRawQuantity(CurrentVolume);
                this.MaxVolume = CargoHelper.ConvertFromRawQuantity(MaxVolume);
                this.CurrentMass = CargoHelper.ConvertFromRawQuantity(CurrentMass); 
            }

            public InventoryCargo(IMyInventory inventory)
            {
                CurrentVolume = CargoHelper.ConvertFromRawQuantity(inventory.CurrentVolume);
                MaxVolume = CargoHelper.ConvertFromRawQuantity(inventory.MaxVolume);
                CurrentMass = CargoHelper.ConvertFromRawQuantity(inventory.CurrentMass);
            }
        }

        #endregion
    }
}
