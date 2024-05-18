using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MorePlanters.Patches
{
    internal class SaveStatePatch
    {
        [HarmonyPatch(typeof(SaveState), "SaveToFile")]
        [HarmonyPostfix]
        static void SavePlanterExtensions() {
            MorePlantersPlugin.SaveData(SaveState.instance.metadata.worldName);
        }
    }
}
