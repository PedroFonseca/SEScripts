using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRageMath;

namespace SEScripts._3rd_party
{
    class WhipsAutoDoor : Skeleton
    {
        /*            
        /// Whip's Auto Door/Airlock Script v18 - 9/24/2016 ///     
        /// PUBLIC RELEASE ///           
        _______________________________________________________________________            
        ///DESCRIPTION///     

            This script will close opened doors after 3 seconds (default).      
            The duration that a door is allowed to be open can be modified lower      
            down in the code (line 62).   

            This script also supports an INFINITE number of airlock systems.          
        _______________________________________________________________________           
        ///TIMER SETUP///         

            <REQUIRED>: Make a timer and set it to run this script and run itself every 1 second   

            (Optional): If you want the program to be more precise, make the timer   
            "Trigger Now" itself   
        _______________________________________________________________________            
        ///AUTO DOOR CLOSER///        

            The script will fetch ALL doors on the grid and automatically close any   
            door that has been open for 3 seconds (15 seconds for hangar doors).   
            Doors can also be excluded from this feature.     

        Excluding Doors:         
            * Add the tag "[Excluded]" to the front or rear of the door(s) name.        
        _______________________________________________________________________            
        ///AIRLOCKS///            

            This script supports the optional feature of simple airlock systems.    
            Airlock systems are composed of AT LEAST one Interior Door AND one Exterior Door.    
            The airlock status light does NOT affect the functionality of the doors    
            so if you don't have space for one, don't fret :)     

        Airlock system names should follow these patterns:     

            * Interior Airlock Doors: "[Prefix] Airlock Interior"     

            * Exterior Airlock Doors: "[Prefix] Airlock Exterior"     

            * Airlock Status Lights: "[Prefix] Airlock Light"     

            You can make the [Prefix] what ever you wish, but in order for doors in an airlock     
            system to be linked by the script, they MUST have the same prefix.   
        _____________________________________________________________________      

        If you have any questions, comments, or concerns, feel free to leave a comment on             
        the workshop page: http://steamcommunity.com/sharedfiles/filedetails/?id=416932930            
        - Whiplash141   :)     
        _____________________________________________________________________        
        */

        //-------------------------------------------------------------------  
        //================== CONFIGURABLE VARIABLES ======================  
        //-------------------------------------------------------------------  

        //Main runtime variables  
        bool enableAutoDoorCloser = true;
        bool enableAirlockSystem = true;
        bool ignoreAllHangarDoors = true;

        //Door open duration (in seconds)   
        const double regularDoorOpenDuration = 3;
        const double hangarDoorOpenDuration = 15;

        //Door exclusion string   
        const string doorExcludeString = "[Excluded]";

        //Airlock Light Settings   
        Color alarmColor = new Color(255, 40, 40); //color of alarm light           
        Color regularColor = new Color(80, 160, 255); //color of regular light   
        float alarmBlinkLength = 50f;  //alarm blink length in %   
        float regularBlinkLength = 100f; //regular blink length in %   
        float blinkInterval = .8f; // blink interval in seconds   


        //-------------------------------------------------------------------  
        //=========== Don't touch anything below here! <3 ==================  
        //-------------------------------------------------------------------  

        Dictionary<IMyTerminalBlock, double> dictDoors = new Dictionary<IMyTerminalBlock, double>();
        List<IMyTerminalBlock> allDoors = new List<IMyTerminalBlock>();
        double timeElapsed;
        Color lightColor;
        float lightBlinkLength;

        void Main()
        {
            timeElapsed += Runtime.TimeSinceLastRun.TotalSeconds;
            if (timeElapsed >= 0.2)
            {
                Echo("WMI Auto Door and Airlock System Active... " + RunningSymbol());

                GridTerminalSystem.GetBlocksOfType<IMyDoor>(allDoors); //get all doors  

                if (enableAutoDoorCloser)
                {
                    AutoDoors(); //controls auto door closing  
                }

                if (enableAirlockSystem)
                {
                    Airlocks(); //controls airlock system  
                }

                timeElapsed = 0; //reset time count  
                timeSymbol++;
            }
        }

        void AutoDoors()
        {
            int autoDoorCount = 0;

            foreach (IMyDoor thisDoor in allDoors)
            {
                if (thisDoor.CustomName.Contains(doorExcludeString)) //removes excluded doors   
                {
                    continue;
                }

                if (thisDoor is IMyAirtightHangarDoor && ignoreAllHangarDoors)
                {
                    continue;
                }

                autoDoorCount++;

                if (dictDoors.ContainsKey(thisDoor)) //checks dict for said door   
                {
                    if (thisDoor.OpenRatio == 0)
                    {
                        dictDoors.Remove(thisDoor); //ignores door if closed   
                    }
                    else
                    {
                        double doorCount;
                        dictDoors.TryGetValue(thisDoor, out doorCount); //pulls current time count   
                        dictDoors.Remove(thisDoor); //removes old time count   

                        double relevantOpenDuration = regularDoorOpenDuration;
                        if (thisDoor is IMyAirtightHangarDoor) //checks if this is a hangar door  
                        {
                            relevantOpenDuration = hangarDoorOpenDuration;
                        }

                        if (doorCount + timeElapsed < relevantOpenDuration) //check if door is allowed to stay open  
                        {
                            dictDoors.Add(thisDoor, doorCount + timeElapsed); //adds new time count   
                        }
                        else
                        {
                            thisDoor.ApplyAction("Open_Off"); //closes door if duration has past   
                        }
                    }
                }
                else
                {
                    if (thisDoor.OpenRatio > 0) //if door isnt in dict, we add it at a time count of zero   
                    {
                        dictDoors.Add(thisDoor, 0);
                    }
                }
            }

            Echo($"\n===Automatic Doors===\nManaged Doors: {autoDoorCount}");
        }

        void Airlocks()
        {
            Echo("\n===Airlock Systems===");

            List<IMyTerminalBlock> allLights = new List<IMyTerminalBlock>();
            List<IMyTerminalBlock> airlockDoors = new List<IMyTerminalBlock>();
            List<string> airlockNames = new List<string>();
            List<string> airlockNamesNoDupes = new List<string>();
            string currentAirlock;
            bool isInteriorClosed;
            bool isExteriorClosed;

            GridTerminalSystem.GetBlocksOfType<IMyLightingBlock>(allLights);

            //Fetch all airlock doors; this assumes proper airlock setup   
            for (int i = 0; i < allDoors.Count; i++)
            {
                var thisDoor = allDoors[i];
                if (thisDoor.CustomName.ToLower().Contains("airlock interior"))//lists all allDoors with proper name   
                {
                    airlockDoors.Add(thisDoor);
                }
            }

            //assign airlock doors to interior or exterior   
            for (int i = 0; i < airlockDoors.Count; i++)
            {
                string thisName = airlockDoors[i].CustomName.ToLower();
                thisName = thisName.Replace("airlock interior", ""); //remove airlock tag   

                if (thisName.Contains(doorExcludeString.ToLower()))
                {
                    thisName = thisName.Replace(doorExcludeString.ToLower(), ""); //remove door exclusion string   
                }

                thisName = thisName.Replace(" ", ""); //remove spaces   

                if (!(airlockNames.Contains(thisName)))
                {
                    airlockNames.Add(thisName);//adds name to string list   
                }
            }

            if (airlockNames.Count == 0)
            {
                Echo("No airlock groups found");
            }

            //Evaluate each unique airlock name   
            for (int i = 0; i < airlockNames.Count; i++)
            {
                List<IMyTerminalBlock> airlockInteriorList = new List<IMyTerminalBlock>();
                List<IMyTerminalBlock> airlockExteriorList = new List<IMyTerminalBlock>();
                List<IMyTerminalBlock> airlockLightList = new List<IMyTerminalBlock>();

                //sort through all doors   
                for (int j = 0; j < allDoors.Count; j++)
                {
                    var thisDoor = allDoors[j];
                    string thisDoorName = thisDoor.CustomName.Replace(" ", "").ToLower();
                    if (thisDoorName.Contains(airlockNames[i]))
                    {
                        if (thisDoorName.Contains("airlockinterior"))
                        {
                            airlockInteriorList.Add(thisDoor);
                        }
                        else if (thisDoorName.Contains("airlockexterior"))
                        {
                            airlockExteriorList.Add(thisDoor);
                        }
                    }
                }

                //sort through all lights   
                for (int j = 0; j < allLights.Count; j++)
                {
                    var thisLight = allLights[j];
                    string thisLightName = thisLight.CustomName.Replace(" ", "").ToLower();
                    if (thisLightName.Contains(airlockNames[i]) && thisLightName.Contains("airlocklight"))
                    {
                        airlockLightList.Add(thisLight);
                    }
                }

                //Start checking airlock status     
                if (airlockInteriorList.Count != 0 && airlockExteriorList.Count != 0) //if we have both door types      
                {
                    //fetch name of airlock group and write to console   
                    currentAirlock = airlockNames[i];                                                                                                                                                /*W=h*I/p=l/A.s/H/1/4/1/*/
                    Echo("Airlock Group '" + currentAirlock + "' found");

                    //we assume the airlocks are closed until proven otherwise          
                    isInteriorClosed = true;
                    isExteriorClosed = true;

                    //Door Interior Check      
                    for (int j = 0; j < airlockInteriorList.Count; j++)
                    {
                        var airlockInterior = airlockInteriorList[j] as IMyDoor;

                        if (airlockInterior.OpenRatio > 0)
                        {
                            Lock(airlockExteriorList);
                            LightColorChanger(true, airlockLightList);
                            isInteriorClosed = false;
                            break;
                            //if any doors yield false, bool will persist until comparison      
                        }
                    }

                    //Door Exterior Check             
                    for (int j = 0; j < airlockExteriorList.Count; j++)
                    {
                        var airlockExterior = airlockExteriorList[j] as IMyDoor;

                        if (airlockExterior.OpenRatio > 0)
                        {
                            Lock(airlockInteriorList);
                            LightColorChanger(true, airlockLightList);
                            isExteriorClosed = false;
                            break;
                        }
                    }

                    //if all Interior & Exterior doors closed   
                    if (isInteriorClosed && isExteriorClosed)
                    {
                        LightColorChanger(false, airlockLightList);
                    }

                    //if all Interior doors closed   
                    if (isInteriorClosed)
                    {
                        Unlock(airlockExteriorList);
                    }

                    //if all Exterior doors closed       
                    if (isExteriorClosed)
                    {
                        Unlock(airlockInteriorList);
                    }
                }
            }
        }

        void Lock(List<IMyTerminalBlock> lock_door_list)
        {
            //locks all doors with the inputed list            
            foreach (IMyDoor lock_door in lock_door_list)
            {
                if (lock_door.Open)
                    lock_door.ApplyAction("Open_Off");

                if (lock_door.OpenRatio == 0 && lock_door.GetValue<bool>("OnOff") == true)
                    lock_door.ApplyAction("OnOff_Off");
            }
        }

        void Unlock(List<IMyTerminalBlock> unlock_door_list)
        {
            //unlocks all doors with inputed list              
            foreach (IMyDoor unlock_door in unlock_door_list)
            {
                if (unlock_door.GetValue<bool>("OnOff") == false)
                    unlock_door.ApplyAction("OnOff_On");
            }
        }

        void LightColorChanger(bool alarm, List<IMyTerminalBlock> listLights)
        {
            //applies our status colors to the airlock lights          
            if (alarm)
            {
                lightColor = alarmColor;
                lightBlinkLength = alarmBlinkLength;
            }
            else
            {
                lightColor = regularColor;
                lightBlinkLength = regularBlinkLength;
            }

            for (int i = 0; i < listLights.Count; i++)
            {
                var thisLight = listLights[i] as IMyLightingBlock;
                thisLight.SetValue("Color", lightColor);
                thisLight.SetValue("Blink Lenght", lightBlinkLength);
                thisLight.SetValue("Blink Interval", blinkInterval);
            }
        }

        //Whip's Running Symbol Method v5  
        int timeSymbol = 0;

        string RunningSymbol()
        {
            string strRunningSymbol = "";

            if (timeSymbol < 1)
                strRunningSymbol = "|";
            else if (timeSymbol < 2)
                strRunningSymbol = "/";
            else if (timeSymbol < 3)
                strRunningSymbol = "--";
            else if (timeSymbol < 4)
                strRunningSymbol = "\\";
            else
            {
                timeSymbol = 0;
                strRunningSymbol = "|";
            }

            return strRunningSymbol;
        }
    }
}
