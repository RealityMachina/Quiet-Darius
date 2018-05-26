using System;
using System.Reflection;
using Harmony;
using BattleTech;

namespace ShutUpDarius
{
    public static class BeQuietDarius
    {
        public static int MinPilotReq = 2;
        public static void Init()
        {
            var harmony = HarmonyInstance.Create("Battletech.realitymachina.ShutUpDarius");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        public static void OnNotificationDismissed()
        {
        }
    }

    [HarmonyPatch(typeof(BattleTech.SimGameState), "ShowMechWarriorTrainingNotif")]
    public static class BattleTech_Pilot_ShowMechWarriorTrainingNotif_Patch
    {
        static bool Prefix(SimGameState __instance)
        {
            //no easy way to avoid incompatibilites here: we're completely overriding the logic so we can do what we need to do
            return false;
        }
    }

    [HarmonyPatch(typeof(BattleTech.SimGameState), "ShowMechWarriorTrainingNotif")]
    public static class BattleTech_Pilot_ShowMechWarriorTrainingNotif_ReplacementMethod
    {
        static void Postfix(SimGameState __instance)
        {
            int pilotsToTrain = 0; //set this to 0 every time we use this
            foreach (Pilot curPilot in __instance.PilotRoster)
            {
                bool canTrain = false;
                int[] source = new int[]
                {
                        curPilot.Gunnery,
                        curPilot.Tactics,
                        curPilot.Guts,
                        curPilot.Piloting
                };
                for(int i = 0; i < source.Length && !canTrain; i++ )
                {
                    if(source[i] < 10) //exclude max ranked skills
                    {
                        if (curPilot.UnspentXP > __instance.GetLevelCost(source[i] + 1))
                        {

                            canTrain = true; //exit out of this loop, moves on to next pilot
                        }
                    }
                }

                if(canTrain)
                {
                    pilotsToTrain++;
                }
 
            }
            if (pilotsToTrain >= BeQuietDarius.MinPilotReq)
            {
                __instance.GetInterruptQueue().QueuePauseNotification("MechWarrior Training Required", string.Format("Our MechWarriors are gaining in experience and need your guidance, {0}. If you head to the Barracks, you can direct their training.", __instance.Commander.Callsign), __instance.GetCrewPortrait(SimGameCrew.Crew_Darius), null, new Action(BeQuietDarius.OnNotificationDismissed), "Continue", null, null);
            }
        }

    }
}
