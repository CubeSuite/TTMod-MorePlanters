using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using EquinoxsModUtils;
using HarmonyLib;
using MorePlanters.Patches;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace MorePlanters
{
    [BepInPlugin(MyGUID, PluginName, VersionString)]
    public class MorePlantersPlugin : BaseUnityPlugin
    {
        private const string MyGUID = "com.equinox.MorePlanters";
        private const string PluginName = "MorePlanters";
        private const string VersionString = "1.0.1";

        private static readonly Harmony Harmony = new Harmony(MyGUID);
        public static ManualLogSource Log = new ManualLogSource(PluginName);

        // Objects & Variables
        private static string dataFolder => $"{Application.persistentDataPath}/MorePlanters";
        
        public const string planterMk2Name = "Planter MKII";
        public const string planterMk3Name = "Planter MKIII";

        public static SchematicsRecipeData planterMk2Recipe;
        public static SchematicsRecipeData planterMk3Recipe;

        // Assets
        public static GameObject planterMk2Prefab;
        public static GameObject planterMk3Prefab;

        // Config Entries
        public static ConfigEntry<float> slot1XOffset;
        public static ConfigEntry<float> slot2XOffset;
        public static ConfigEntry<float> slot3XOffset;
        public static ConfigEntry<float> slot4XOffset;
        public static ConfigEntry<float> yOffset;

        private void Awake() {
            Logger.LogInfo($"PluginName: {PluginName}, VersionString: {VersionString} is loading...");
            Harmony.PatchAll();

            CreateConfigEntries();
            ApplyPatches();
            LoadPrefabs();

            PlanterMk3GUI.buttonUncheckedTexture = ModUtils.LoadTexture2DFromFile("MorePlanters.Assets.Images.CheckboxUnTicked.png");
            PlanterMk3GUI.buttonCheckedTexture = ModUtils.LoadTexture2DFromFile("MorePlanters.Assets.Images.CheckboxTicked.png");

            ModUtils.AddNewUnlock(new NewUnlockDetails() {
                category = Unlock.TechCategory.Synthesis,
                coreTypeNeeded = ResearchCoreDefinition.CoreType.Green,
                coreCountNeeded = 100,
                description = "Produces two plants per seed at 2x speed",
                displayName = planterMk2Name,
                numScansNeeded = 0,
                requiredTier = TechTreeState.ResearchTier.Tier0,
                treePosition = 0,
                sprite = ModUtils.LoadSpriteFromFile("MorePlanters.Assets.Images.Planter Mk2.png")
            });
            ModUtils.AddNewUnlock(new NewUnlockDetails() {
                category = Unlock.TechCategory.Synthesis,
                coreTypeNeeded = ResearchCoreDefinition.CoreType.Green,
                coreCountNeeded = 250,
                description = "Houses an integrated Thresher and has the capability to self-seed",
                displayName = planterMk3Name,
                numScansNeeded = 0,
                requiredTier = TechTreeState.ResearchTier.Tier0,
                treePosition = 0,
                sprite = ModUtils.LoadSpriteFromFile("MorePlanters.Assets.Images.Planter Mk3.png")
            });

            ModUtils.GameDefinesLoaded += OnGameDefinesLoaded;
            ModUtils.SaveStateLoaded += OnSaveStateLoaded;
            ModUtils.TechTreeStateLoaded += OnTechTreeLoaded;

            Logger.LogInfo($"PluginName: {PluginName}, VersionString: {VersionString} is loaded.");
            Log = Logger;
        }

        private void OnGUI() {
            if (PlanterMk3GUI.shouldShowGUI) {
                PlanterMk3GUI.DrawGUI();
            }
        }

        // Events
        
        private void OnGameDefinesLoaded(object sender, EventArgs e) {
            ResourceInfo planterMk2 = ModUtils.GetResourceInfoByName(planterMk2Name);
            planterMk2.unlock = ModUtils.GetUnlockByName(planterMk2Name);

            ResourceInfo planterMk3 = ModUtils.GetResourceInfoByName(planterMk3Name);
            planterMk3.unlock = ModUtils.GetUnlockByName(planterMk3Name);
        }

        private void OnSaveStateLoaded(object sender, EventArgs e) {
            LoadData(SaveState.instance.metadata.worldName);
        }

        private void OnTechTreeLoaded(object sender, EventArgs e) {
            Unlock atlantumIngot = ModUtils.GetUnlockByName(UnlockNames.AtlantumIngot);
            Unlock thresherMk2 = ModUtils.GetUnlockByName(UnlockNames.ThresherMKII);
            Unlock assemblerMk2 = ModUtils.GetUnlockByName(UnlockNames.AssemblerMKII);

            Unlock planterMk2 = ModUtils.GetUnlockByName(planterMk2Name);
            planterMk2.requiredTier = atlantumIngot.requiredTier;
            planterMk2.treePosition = assemblerMk2.treePosition;
            planterMk2.unlockedRecipes.Add(planterMk2Recipe);

            Unlock planterMk3 = ModUtils.GetUnlockByName(planterMk3Name);
            planterMk3.requiredTier = thresherMk2.requiredTier;
            planterMk3.treePosition = assemblerMk2.treePosition;
            planterMk3.unlockedRecipes.Add(planterMk3Recipe);

            Unlock planter = ModUtils.GetUnlockByName(UnlockNames.Planter);
            planter.unlockedRecipes.Remove(planterMk2Recipe);
            planter.unlockedRecipes.Remove(planterMk3Recipe);
        }

        // Public Functions

        public static void SaveData(string worldName) {
            Directory.CreateDirectory(dataFolder);

            string saveFile = $"{dataFolder}/{worldName}.txt";
            List<string> lines = new List<string>();
            foreach (PlanterExtension extension in PlanterMk3GUI.planterExtensions.Values) {
                lines.Add(extension.Serialise());
            }

            File.WriteAllLines(saveFile, lines);
        }

        public static void LoadData(string worldName) {
            string saveFile = $"{dataFolder}/{worldName}.txt";
            if (!File.Exists(saveFile)) {
                Log.LogWarning($"Save file not found for world '{worldName}'");
                return;
            }

            string[] lines = File.ReadAllLines(saveFile);
            foreach(string line in lines) {
                PlanterExtension extension = PlanterExtension.Deserialise(line);
                PlanterMk3GUI.planterExtensions.Add(extension.instanceId, extension);
            }
        }

        // Private Functions

        private void CreateConfigEntries() {
            slot1XOffset = Config.Bind("General", "Slot 1 X Offset", -295f, new ConfigDescription("Controls the horizontal offset of the filter GUI for the first plant slot", new AcceptableValueRange<float>(-5000, 5000)));
            slot2XOffset = Config.Bind("General", "Slot 2 X Offset", -145f, new ConfigDescription("Controls the horizontal offset of the filter GUI for the second plant slot", new AcceptableValueRange<float>(-5000, 5000)));
            slot3XOffset = Config.Bind("General", "Slot 3 X Offset", 10f, new ConfigDescription("Controls the horizontal offset of the filter GUI for the third plant slot", new AcceptableValueRange<float>(-5000, 5000)));
            slot4XOffset = Config.Bind("General", "Slot 4 X Offset", 160f, new ConfigDescription("Controls the horizontal offset of the filter GUI for the fourth plant slot", new AcceptableValueRange<float>(-5000, 5000)));
            yOffset = Config.Bind("General", "GUI Y Offset", -300f, new ConfigDescription("Controls the vertical offset of the filter GUI", new AcceptableValueRange<float>(-5000, 5000)));
        }

        private void ApplyPatches() {
            Harmony.CreateAndPatchAll(typeof(GameDefinesPatch));
            Harmony.CreateAndPatchAll(typeof(PlanterDefinitionPatch));
            Harmony.CreateAndPatchAll(typeof(PlanterInstancePatch));
            Harmony.CreateAndPatchAll(typeof(PlanterUIPatch));
            Harmony.CreateAndPatchAll(typeof(SaveStatePatch));
        }

        private void LoadPrefabs() {
            AssetBundle bundle = LoadAssetBundle("caspuinox");

            planterMk2Prefab = bundle.LoadAsset<GameObject>("assets/gpui_plantermk2.prefab");
            planterMk3Prefab = bundle.LoadAsset<GameObject>("assets/gpui_plantermk3.prefab");

            planterMk3Prefab.transform.Find("r_spin")?.gameObject.AddComponent<IndependentRotation>();
            planterMk3Prefab.transform.Find("f_spin")?.gameObject.AddComponent<IndependentRotation>();
        }

        private static AssetBundle LoadAssetBundle(string filename) {
            Assembly assembly = Assembly.GetCallingAssembly();
            AssetBundle assetBundle = AssetBundle.LoadFromStream(assembly.GetManifestResourceStream($"MorePlanters.Assets.{filename}"));
            return assetBundle;
        }
    }

    public class IndependentRotation : MonoBehaviour
    {
        public Vector3 rotationAxis = Vector3.left;
        public float rotationSpeed = 90.0f;

        public void SetRotationSettings(Vector3 axis, float speed) {
            rotationAxis = axis;
            rotationSpeed = speed;
        }

        void Update() {
            // Rotate around the specified local axis at the specified speed
            transform.Rotate(rotationAxis * rotationSpeed * Time.deltaTime, Space.Self);
        }
    }
}
