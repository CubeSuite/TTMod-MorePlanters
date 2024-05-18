using EquinoxsModUtils;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MorePlanters.Patches
{
    public class PlanterUIPatch
    {
        [HarmonyPatch(typeof(PlanterUI), "OnOpen")]
        [HarmonyPostfix]
        static void ShowPlanterMk3GUI(PlanterUI __instance) {
            MachineInstanceRef<PlanterInstance> instance = (MachineInstanceRef<PlanterInstance>)ModUtils.GetPrivateField("_myMachineRef", __instance);
            PlanterInstance planter = instance.Get();
            if(planter.myDef.displayName == MorePlantersPlugin.planterMk3Name) {
                Debug.Log($"Opened Mk3 Planter: {planter.commonInfo.instanceId}"); 
                PlanterMk3GUI.currentPlanter = planter;
                PlanterMk3GUI.buttonStates = new bool[4, 3];
                PlanterMk3GUI.shouldShowGUI = true;
            }
        }

        [HarmonyPatch(typeof(PlanterUI), "OnClose")]
        [HarmonyPostfix]
        static void HidePlantMk3GUI() {
            PlanterMk3GUI.shouldShowGUI = false;
            Debug.Log("Hid Planter Mk3 GUI");
        }
    }
}
