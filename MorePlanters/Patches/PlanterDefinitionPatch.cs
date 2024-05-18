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
    internal class PlanterDefinitionPatch
    {
        // Objects & Variables
        private static Dictionary<Vector3, GameObject> myVisualsMap = new Dictionary<Vector3, GameObject>();

        [HarmonyPatch(typeof(MachineDefinition<PlanterInstance, PlanterDefinition>), "OnBuild")]
        [HarmonyPostfix]
        static void AddVisuals(PlanterDefinition __instance, MachineInstanceRef<PlanterInstance> instRef) {
            GameObject prefab = null;
            switch (__instance.displayName) {
                case ResourceNames.Planter: return;
                case MorePlantersPlugin.planterMk2Name: prefab = MorePlantersPlugin.planterMk2Prefab; break;
                case MorePlantersPlugin.planterMk3Name: prefab = MorePlantersPlugin.planterMk3Prefab; break;
            }

            GameObject myVisuals = GameObject.Instantiate(prefab, instRef.gridInfo.BottomCenter, Quaternion.Euler(0, instRef.gridInfo.yawRot + 90, 0));
            myVisuals.transform.localScale = new Vector3(1.01f, 1.01f, 1.01f);
            myVisualsMap.Add(instRef.gridInfo.BottomCenter, myVisuals);
        }

        [HarmonyPatch(typeof(MachineDefinition<PlanterInstance, PlanterDefinition>), "OnDeconstruct")]
        [HarmonyPostfix]
        static void RemoveVisuals(PlanterDefinition __instance, ref PlanterInstance erasedInstance) {
            if (!myVisualsMap.ContainsKey(erasedInstance.gridInfo.BottomCenter)) return;

            GameObject.Destroy(myVisualsMap[erasedInstance.gridInfo.BottomCenter]);
            myVisualsMap.Remove(erasedInstance.gridInfo.BottomCenter);
        }
    }
}
