using EquinoxsModUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MorePlanters
{
    public static class PlanterMk3GUI
    {
        // Objects & Variables
        public static bool shouldShowGUI = false;
        public static PlanterInstance currentPlanter;
        public static bool[,] buttonStates = new bool[4, 3];
        public static Dictionary<uint, PlanterExtension> planterExtensions = new Dictionary<uint, PlanterExtension>();
        // ToDo: Save and Load 

        // GUI Textures
        public static Texture2D buttonUncheckedTexture;
        public static Texture2D buttonCheckedTexture;

        // Temp - Replace with settings
        public static float slot1XOffset => MorePlantersPlugin.slot1XOffset.Value;
        public static float slot2XOffset => MorePlantersPlugin.slot2XOffset.Value;
        public static float slot3XOffset => MorePlantersPlugin.slot3XOffset.Value;
        public static float slot4XOffset => MorePlantersPlugin.slot4XOffset.Value;
        public static float yOffset => MorePlantersPlugin.yOffset.Value;
        
        // Public Functions

        public static void DrawGUI() {
            uint id = currentPlanter.commonInfo.instanceId;
            if (!planterExtensions.ContainsKey(id)) {
                planterExtensions.Add(id, new PlanterExtension(id));
                
                for (int i = 0; i < 4; i++) {
                    buttonStates[i, 0] = true;
                    buttonStates[i, 1] = false;
                    buttonStates[i, 2] = false;

                    if (currentPlanter.plantSlots[i].state == PlanterInstance.PlantState.Empty) continue;

                    ResourceInfo seed = SaveState.GetResInfoFromId(currentPlanter.plantSlots[i].plantId);
                    string defaultOutputName = seed.displayName == ResourceNames.Kindlevine ? ResourceNames.KindlevineStems : ResourceNames.ShiverthornBuds;
                    switch (i) {
                        case 0: planterExtensions[id].slot1Output = ModUtils.GetResourceInfoByName(defaultOutputName); break;
                        case 1: planterExtensions[id].slot2Output = ModUtils.GetResourceInfoByName(defaultOutputName); break;
                        case 2: planterExtensions[id].slot3Output = ModUtils.GetResourceInfoByName(defaultOutputName); break;
                        case 3: planterExtensions[id].slot4Output = ModUtils.GetResourceInfoByName(defaultOutputName); break;
                    }
                }

                planterExtensions[id].buttonStates = buttonStates;
            }
            else {
                buttonStates = planterExtensions[id].buttonStates;
            }

            for(int i = 0; i < currentPlanter.plantSlots.Length; i++) {
                if (currentPlanter.plantSlots[i].state == PlanterInstance.PlantState.Empty) continue;
                DrawOptionsForPlantSlot(i);
            }
        }

        // Private Functions

        private static void DrawOptionsForPlantSlot(int slot) {
            ResourceInfo seed = SaveState.GetResInfoFromId(currentPlanter.plantSlots[slot].plantId);
            float xPos = Screen.width / 2 + GetOffsetForSlot(slot);
            float yPos = Screen.height / 2 + yOffset;

            GUIStyle uncheckedButtonStyle = new GUIStyle() {
                padding = new RectOffset(5, 5, 0, 0),
                normal = { background = buttonUncheckedTexture },
                hover = { background = buttonUncheckedTexture },
                active = { background = buttonUncheckedTexture },
                focused = { background = buttonUncheckedTexture },
                onNormal = { background = buttonUncheckedTexture },
                onHover = { background = buttonUncheckedTexture },
                onActive = { background = buttonUncheckedTexture },
                onFocused = { background = buttonUncheckedTexture },
            };
            GUIStyle checkedButtonStyle = new GUIStyle() {
                padding = new RectOffset(5, 5, 0, 0),
                normal = { background = buttonCheckedTexture },
                hover = { background = buttonCheckedTexture },
                active = { background = buttonCheckedTexture },
                focused = { background = buttonCheckedTexture },
                onNormal = { background = buttonCheckedTexture },
                onHover = { background = buttonCheckedTexture },
                onActive = { background = buttonCheckedTexture },
                onFocused = { background = buttonCheckedTexture },
            };

            if (seed.displayName == ResourceNames.Kindlevine) {
                if (GUI.Button(new Rect(xPos, yPos, 40, 40), ModUtils.GetImageForResource(ResourceNames.KindlevineStems), buttonStates[slot, 0] ? checkedButtonStyle : uncheckedButtonStyle)) {
                    HandleButtonClick(slot, 0, ResourceNames.KindlevineStems);
                }
                if (GUI.Button(new Rect(xPos + 45, yPos, 40, 40), ModUtils.GetImageForResource(ResourceNames.KindlevineExtract), buttonStates[slot, 1] ? checkedButtonStyle : uncheckedButtonStyle)) {
                    HandleButtonClick(slot, 1, ResourceNames.KindlevineExtract);
                }
                if(GUI.Button(new Rect(xPos + 90, yPos, 40, 40), ModUtils.GetImageForResource(ResourceNames.PlantmatterFiber), buttonStates[slot, 2] ? checkedButtonStyle : uncheckedButtonStyle)) {
                    HandleButtonClick(slot, 2, ResourceNames.PlantmatterFiber);
                }
            }
            else if (seed.displayName == ResourceNames.Shiverthorn) {
                if (GUI.Button(new Rect(xPos, yPos, 40, 40), ModUtils.GetImageForResource(ResourceNames.ShiverthornBuds), buttonStates[slot, 0] ? checkedButtonStyle : uncheckedButtonStyle)) {
                    HandleButtonClick(slot, 0, ResourceNames.ShiverthornBuds);
                }
                if (GUI.Button(new Rect(xPos + 45, yPos, 40, 40), ModUtils.GetImageForResource(ResourceNames.ShiverthornExtract), buttonStates[slot, 1] ? checkedButtonStyle : uncheckedButtonStyle)) {
                    HandleButtonClick(slot, 1, ResourceNames.ShiverthornExtract);
                }
                if (GUI.Button(new Rect(xPos + 90, yPos, 40, 40), ModUtils.GetImageForResource(ResourceNames.Plantmatter), buttonStates[slot, 2] ? checkedButtonStyle : uncheckedButtonStyle)) {
                    HandleButtonClick(slot, 2, ResourceNames.Plantmatter);
                }
            }
        }

        private static float GetOffsetForSlot(int slot) {
            switch (slot) {
                case 0: return slot1XOffset;
                case 1: return slot2XOffset;
                case 2: return slot3XOffset;
                case 3: return slot4XOffset;
            }

            return 0;
        }

        private static void HandleButtonClick(int plantSlot, int buttonIndex, string outputName) {
            for (int i = 0; i < 3; i++) buttonStates[plantSlot, i] = false;
            buttonStates[plantSlot, buttonIndex] = true;

            uint id = currentPlanter.commonInfo.instanceId;
            planterExtensions[id].buttonStates = buttonStates;

            switch (plantSlot) {
                case 0: planterExtensions[id].slot1Output = ModUtils.GetResourceInfoByName(outputName); break;
                case 1: planterExtensions[id].slot2Output = ModUtils.GetResourceInfoByName(outputName); break;
                case 2: planterExtensions[id].slot3Output = ModUtils.GetResourceInfoByName(outputName); break;
                case 3: planterExtensions[id].slot4Output = ModUtils.GetResourceInfoByName(outputName); break;
            }
        }
    }

    public class PlanterExtension {
        public uint instanceId;
        public bool[,] buttonStates;
        public ResourceInfo slot1Output;
        public ResourceInfo slot2Output;
        public ResourceInfo slot3Output;
        public ResourceInfo slot4Output;

        public List<ResourceStack> queuedOutputs = new List<ResourceStack>();

        public string Serialise() {
            string output = $"{instanceId}|";

            for(int x = 0; x < 4; x++) {
                for(int y = 0; y < 3; y++) {
                    output += buttonStates[x, y] ? "1" : "0";
                }
            }

            string slot1 = slot1Output == null ? "null" : slot1Output.displayName;
            string slot2 = slot2Output == null ? "null" : slot2Output.displayName;
            string slot3 = slot3Output == null ? "null" : slot3Output.displayName;
            string slot4 = slot4Output == null ? "null" : slot4Output.displayName;

            output += $"|{slot1}";
            output += $"|{slot2}";
            output += $"|{slot3}";
            output += $"|{slot4}";

            foreach(ResourceStack stack in queuedOutputs) {
                output += $"|{stack.info.uniqueId},{stack.count}";
            }

            return output;
        }

        public static PlanterExtension Deserialise(string input) {
            PlanterExtension extension = new PlanterExtension(0);
            List<string> parts = input.Split('|').ToList();

            extension.instanceId = uint.Parse(parts[0]);
            parts.RemoveAt(0);

            for(int x = 0; x < 4; x++) {
                for (int y = 0; y < 3; y++) {
                    int index = x * 3 + y;
                    extension.buttonStates[x, y] = parts[0][index] == '1';
                }
            }

            parts.RemoveAt(0);

            if (parts[0] != "null") extension.slot1Output = ModUtils.GetResourceInfoByName(parts[0]);
            parts.RemoveAt(0);

            if (parts[0] != "null") extension.slot2Output = ModUtils.GetResourceInfoByName(parts[0]);
            parts.RemoveAt(0);

            if (parts[0] != "null") extension.slot3Output = ModUtils.GetResourceInfoByName(parts[0]);
            parts.RemoveAt(0);

            if (parts[0] != "null") extension.slot4Output = ModUtils.GetResourceInfoByName(parts[0]);
            parts.RemoveAt(0);

            foreach(string part in parts) {
                int resId = int.Parse(part.Split(',')[0]);
                int count = int.Parse(part.Split(',')[1]);
                extension.queuedOutputs.Add(ResourceStack.CreateSimpleStack(resId, count));
            }

            return extension;
        }

        public PlanterExtension(uint id) {
            instanceId = id;
            buttonStates = new bool[4, 3];
        }
    }
}
