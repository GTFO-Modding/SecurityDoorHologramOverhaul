using HarmonyLib;
using LevelGeneration;
using System;
using System.Collections.Generic;
using System.Text;

namespace SecurityDoorHologramOverhaul.Injects
{
    [HarmonyPatch(typeof(LG_SecurityDoor))]
    internal static class Inject_LG_SecDoor
    {
        [HarmonyPostfix]
        [HarmonyWrapSafe]
        [HarmonyPatch(nameof(LG_SecurityDoor.Setup))]
        private static void Post_Setup(LG_SecurityDoor __instance)
        {
            DoorHologramManager.SecurityDoorSpawned(__instance);
        }
    }
}
