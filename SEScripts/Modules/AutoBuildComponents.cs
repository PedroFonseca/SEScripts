using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.ModAPI;
using SEScripts.Helpers;
using VRage.Game;

namespace SEScripts.Modules
{
    #region Usings

    using GridBlocksHelper = Helpers.GridBlocksHelper;
    using InventoryHelper = Helpers.InventoryHelper;
    using CargoHelper = Helpers.CargoHelper;
    using ComponentHelper = Helpers.ComponentHelper;

    #endregion Usings
    #region SpaceEngineers
    public class AutoBuildComponents
    {
        // Insert here how much of each component you want to build
        public static Dictionary<string, int> DefaultComponentDesiredQuantities = new Dictionary<string, int>
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

        private IMyGridTerminalSystem GTS { get; set; }

        public AutoBuildComponents()
        {
        }

        private AutoBuildComponents(IMyGridTerminalSystem gts)
        {
            GTS = gts;
        }

        public static AutoBuildComponents Get(IMyGridTerminalSystem gts)
        {
            return new AutoBuildComponents(gts);
        }

        public string BuildComponentsToQuota(string gridPrefix, Dictionary<string, int> desiredComponents = null, string mainAssemblerName = null)
        {
            // Get all containers in grid
            var gridContainers = GridBlocksHelper.Get(GTS).GetCargoContainers(gridPrefix);
            var containerInventories = gridContainers.Select(container => InventoryHelper.GetInventories(container).First()).ToList();
            var gridAssemblies = GridBlocksHelper.Get(GTS).GetBlocksOfTypeByName<IMyAssembler>(gridPrefix);
            if (gridAssemblies.Count == 0)
                throw new Exception(string.Format("Assembler with name starting with {0} not found.", gridPrefix));
            var resultAssemblyInventories = gridAssemblies.Select(container => InventoryHelper.GetInventories(container).Last()).ToList();
            var components = CargoHelper.GetItemsInInventories(containerInventories.Concat(resultAssemblyInventories)).Select(t=>t.Value).Where(t => t.IsComponent);

            var debug = "";
            var desiredComponentQuantities = desiredComponents ?? DefaultComponentDesiredQuantities;
            var componentsToBuild = desiredComponentQuantities.ToDictionary(desired => desired.Key, desired =>
            {
                var existing = components.FirstOrDefault(comp => comp.ItemName == desired.Key)?.AccurateQuantity ?? 0;
                return desired.Value - existing;
            });

            // Actually add to queue the missing items
            var mainAssembler = gridAssemblies.Find(ass => ass.Name == mainAssemblerName) ?? gridAssemblies[0];
            if (mainAssembler != null && mainAssembler.IsQueueEmpty)
            {
                // Only add items when queue is empty
                foreach (var componentToBuild in componentsToBuild)
                {
                    if(componentToBuild.Value > 0){
                        try {
                            mainAssembler.AddQueueItem(ComponentHelper.GetBlueprintDefinition(componentToBuild.Key), componentToBuild.Value); 
                        } catch(Exception e) {
                            debug += "Unable to parse blueprint id \n"+componentToBuild.Key;
                        }
                    }
                    
                }
            }
            return debug;
        }
    }

    #endregion SpaceEngineers
}
