using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game;
using VRage.Game.ModAPI.Ingame;

namespace SEScripts.Helpers
{
#region SpaceEngineers
    public static class ComponentHelper
    {
        public enum Component
        {
            ComputerComponent, ConstructionComponent, DetectorComponent,
            Display, GirderComponent, GravityGeneratorComponent, MetalGrid, MedicalComponent,
            LargeTube, InteriorPlate, Missile200mm, MotorComponent, NATO_5p56x45mmMagazine,
            SmallTube, RadioCommunicationComponent, PowerCell, ThrustComponent
        };

        public static MyDefinitionId GetBlueprintDefinition(string itemName)
        {
            var id = "MyObjectBuilder_BlueprintDefinition/" + ComponentToBlueprint(itemName);
            MyDefinitionId result;
            if (!MyDefinitionId.TryParse(id, out result))
                throw new Exception("Unable to parse blueprint id " + id);
            return result;
        }
        public static MyDefinitionId GetBlueprintDefinition(Component comp)
        {
            return GetBlueprintDefinition(comp.ToString());
        }

        /// <summary>
        /// Converts the component Subtype to the blueprint Subtype 
        /// </summary>
        /// <param name="componet">The component to convert from</param>
        /// <returns>Blueprint Subtype</returns>
        public static string ComponentToBlueprint(string componentName)
        {
            if (componentName == "Computer")
            {
                return "ComputerComponent";
            }
            else if (componentName == "Girder")
            {
                return "GirderComponent";
            }
            else if (componentName == "Construction")
            {
                return "ConstructionComponent";
            }
            else
            {
                return componentName;
            }
        }
    }
#endregion SpaceEngineers
}