using EquinoxsModUtils;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MorePlanters.Patches
{
    internal class PlanterInstancePatch
    {
        [HarmonyPatch(typeof(PlanterInstance), "SimUpdate")]
        [HarmonyPrefix]
        static void ApplySpeedBoost(PlanterInstance __instance) {
            if (__instance.myDef.displayName == ResourceNames.Planter) return;

            for (int i = 0; i < __instance.plantSlots.Count(); i++) {
                if (__instance.plantSlots[i].growthProgress > __instance.plantSlots[i].totalGrowthDuration) {
                    __instance.plantSlots[i].growthProgress = __instance.plantSlots[i].totalGrowthDuration;
                }

                int plantId = __instance.plantSlots[i].plantId;
                if (plantId == -1) return;
                ResourceInfo seedInfo = SaveState.GetResInfoFromId(plantId);
                if (__instance.plantSlots[i].totalGrowthDuration != 120f) continue;
                __instance.plantSlots[i].totalGrowthDuration = 60f;
            }
        }

        [HarmonyPatch(typeof(PlanterInstance), "TakeAvailableRes")]
        [HarmonyPrefix]
        static bool DoublePlantsForInserterTake(PlanterInstance __instance, ref bool __result, out ResourceStack stack, int filterType, int maxCount) {
            stack = ResourceStack.CreateEmptyStack();
            if (__instance.myDef.displayName == ResourceNames.Planter) return true;

            if(__instance.myDef.displayName == MorePlantersPlugin.planterMk3Name) {
                HandleTakeAvailableResForMk3(__instance, ref __result, out stack);
                return false;
            }

            int num = 0;
            for (int i = 0; i < __instance.plantSlots.Length; i++) {
                ref PlanterInstance.PlantSlot ptr = ref __instance.plantSlots[i];
                if (ptr.state == PlanterInstance.PlantState.Harvestable && (filterType < 0 || ptr.plantId == filterType) && num < maxCount) {
                    num += 2;
                    filterType = ptr.plantId;
                    if (__instance.myDef.displayName == MorePlantersPlugin.planterMk2Name) {
                        ptr.MakeEmpty();
                        __instance.GetOutputInventory().RemoveResourcesFromSlot(i, SlotTransferMode.All);
                    }
                    else {
                        ptr.growthProgress = 0;
                        ptr.state = PlanterInstance.PlantState.Growing;
                    }
                }
            }
            if (num > 0) {
                stack = ResourceStack.CreateSimpleStack(filterType, num);
            }

            __result = num > 0;
            return false;
        }

        [HarmonyPatch(typeof(PlanterInstance), "TakeAll")]
        [HarmonyPrefix]
        static bool DoublePlantsForTakeAll(PlanterInstance __instance, ref List<ResourceStack> __result, bool actuallyTake) {
            if (__instance.myDef.displayName == ResourceNames.Planter) return true;

            if(__instance.myDef.displayName == MorePlantersPlugin.planterMk3Name) {
                __result = HandleTakeAllForMk3(__instance, actuallyTake);
                return false;
            }

            List<ResourceStack> list = new List<ResourceStack>();
            for (int i = 0; i < __instance.plantSlots.Length; i++) {
                ref PlanterInstance.PlantSlot ptr = ref __instance.plantSlots[i];
                if (ptr.state == PlanterInstance.PlantState.Harvestable) {
                    list.Add(ResourceStack.CreateSimpleStack(ptr.plantId, 2));
                    if (actuallyTake) {
                        ptr.MakeEmpty();
                        __instance.GetOutputInventory().RemoveResourcesFromSlot(i, SlotTransferMode.All);
                    }
                }
            }

            __result = list;

            return false;
        }

        // Private Functions

        private static void HandleTakeAvailableResForMk3(PlanterInstance __instance, ref bool __result, out ResourceStack stack) {
            for(int i = 0; i < __instance.plantSlots.Length; i++) {
                if (__instance.plantSlots[i].state == PlanterInstance.PlantState.Harvestable) {
                    int resIdForSlot = GetResIDForSlot(__instance.commonInfo.instanceId, i);
                    if (resIdForSlot == -1) continue;

                    int count = GetCountForResID(resIdForSlot);
                    if (!PlanterMk3GUI.planterExtensions.ContainsKey(__instance.commonInfo.instanceId)) continue;
                    PlanterMk3GUI.planterExtensions[__instance.commonInfo.instanceId].queuedOutputs.Add(ResourceStack.CreateSimpleStack(resIdForSlot, count));
                    __instance.plantSlots[i].growthProgress = 0;
                    __instance.plantSlots[i].state = PlanterInstance.PlantState.Growing;
                }
            }

            if (!PlanterMk3GUI.planterExtensions.ContainsKey(__instance.commonInfo.instanceId)) {
                __result = false;
                stack = ResourceStack.CreateEmptyStack();
                return;
            }

            if (PlanterMk3GUI.planterExtensions[__instance.commonInfo.instanceId].queuedOutputs.Count > 0) {
                stack = PlanterMk3GUI.planterExtensions[__instance.commonInfo.instanceId].queuedOutputs.First();
                PlanterMk3GUI.planterExtensions[__instance.commonInfo.instanceId].queuedOutputs.RemoveAt(0);
                __result = true;
            }
            else {
                __result = false;
                stack = ResourceStack.CreateEmptyStack();
            }
        }

        private static List<ResourceStack> HandleTakeAllForMk3(PlanterInstance __instance, bool actuallyTake) {
            List<ResourceStack> list = new List<ResourceStack>();
            for (int i = 0; i < __instance.plantSlots.Length; i++) {
                ref PlanterInstance.PlantSlot ptr = ref __instance.plantSlots[i];
                if (ptr.state == PlanterInstance.PlantState.Harvestable) {
                    int slotResID = GetResIDForSlot(__instance.commonInfo.instanceId, i);
                    if (slotResID == -1) slotResID = ptr.plantId;
                    int count = GetCountForResID(slotResID);
                    list.Add(ResourceStack.CreateSimpleStack(slotResID, count));
                    if (actuallyTake) {
                        ptr.MakeEmpty();
                        __instance.GetOutputInventory().RemoveResourcesFromSlot(i, SlotTransferMode.All);
                    }
                }
            }

            return list;
        }

        private static int GetResIDForSlot(uint id, int slot) {
            if (!PlanterMk3GUI.planterExtensions.ContainsKey(id)) return -1;

            switch (slot) {
                case 0:
                    if (PlanterMk3GUI.planterExtensions[id].slot1Output != null) {
                        return PlanterMk3GUI.planterExtensions[id].slot1Output.uniqueId;
                    }

                    return -1;

                case 1:
                    if (PlanterMk3GUI.planterExtensions[id].slot2Output != null) {
                        return PlanterMk3GUI.planterExtensions[id].slot2Output.uniqueId;
                    }

                    return -1;

                case 2:
                    if (PlanterMk3GUI.planterExtensions[id].slot3Output != null) {
                        return PlanterMk3GUI.planterExtensions[id].slot3Output.uniqueId;
                    }

                    return -1;

                case 3:
                    if (PlanterMk3GUI.planterExtensions[id].slot4Output != null) {
                        return PlanterMk3GUI.planterExtensions[id].slot4Output.uniqueId;
                    }

                    return -1;
            }

            return -1;
        }

        private static int GetCountForResID(int resID) {
            ResourceInfo info = SaveState.GetResInfoFromId(resID);
            switch (info.displayName) {
                case ResourceNames.KindlevineStems: return 6;
                case ResourceNames.KindlevineExtract: return 18;
                case ResourceNames.PlantmatterFiber: return 18;

                case ResourceNames.ShiverthornBuds: return 6;
                case ResourceNames.ShiverthornExtract: return 18;
                case ResourceNames.Plantmatter: return 18;
            }

            return 2;
        }
    }
}
