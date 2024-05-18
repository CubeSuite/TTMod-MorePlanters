using EquinoxsModUtils;
using FluffyUnderware.DevTools.Extensions;
using GPUInstancer;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MorePlanters.Patches
{
    internal class GameDefinesPatch
    {
        // Objects & Variables
        private static bool hasAdded = false;
        private static PlanterDefinition planterMk1Definition;
        private static PlanterDefinition planterMk2Definition;
        private static PlanterDefinition planterMk3Definition;

        [HarmonyPatch(typeof(GameDefines), "GetMaxResId")]
        [HarmonyPrefix]
        static void AddNewPlantersToGame() {
            if(hasAdded) return;
            hasAdded = true;

            planterMk1Definition = (PlanterDefinition)ModUtils.GetResourceInfoByNameUnsafe(ResourceNames.Planter);

            AddPlanterMk2();
            AddRecipeForPlanterMk2();

            AddPlanterMk3();
            AddRecipeForPlanterMk3();

            ModUtils.SetPrivateStaticField("_topResId", GameDefines.instance, -1);
        }

        // Private Functions

        private static void AddPlanterMk2() {
            planterMk2Definition = (PlanterDefinition)ScriptableObject.CreateInstance(typeof(PlanterDefinition));
            ModUtils.CloneObject(planterMk1Definition, ref planterMk2Definition);

            planterMk2Definition.description = "Produces two plants per seed at 2x speed";
            planterMk2Definition.rawName = MorePlantersPlugin.planterMk2Name;
            planterMk2Definition.rawSprite = ModUtils.LoadSpriteFromFile("MorePlanters.Assets.Images.Planter Mk2.png");
            planterMk2Definition.uniqueId = ModUtils.GetNewResourceID();
            planterMk2Definition.kWPowerConsumption = 250;
            planterMk2Definition.machineTier = 2;

            GameDefines.instance.resources.Add(planterMk2Definition);
            GameDefines.instance.buildableResources.Add(planterMk2Definition);
            ResourceNames.SafeResources.Add(MorePlantersPlugin.planterMk2Name);
        }

        private static void AddRecipeForPlanterMk2() {
            NewRecipeDetails details = new NewRecipeDetails() {
                craftingMethod = CraftingMethod.Assembler,
                craftTierRequired = 0,
                duration = 10,
                ingredients = new List<RecipeResourceInfo>() {
                    new RecipeResourceInfo(ResourceNames.Planter, 1),
                    new RecipeResourceInfo(ResourceNames.AtlantumIngot, 10),
                    new RecipeResourceInfo(ResourceNames.WallLight1x1, 4)
                },
                outputs = new List<RecipeResourceInfo>() {
                    new RecipeResourceInfo(MorePlantersPlugin.planterMk2Name, 1)
                },
                sortPriority = 4
            };
            MorePlantersPlugin.planterMk2Recipe = details.ConvertToRecipe();
            MorePlantersPlugin.planterMk2Recipe.name = MorePlantersPlugin.planterMk2Name;
            MorePlantersPlugin.planterMk2Recipe.uniqueId = ModUtils.GetNewRecipeID(true);
            MorePlantersPlugin.planterMk2Recipe.unlock = planterMk1Definition.unlock;
            GameDefines.instance.schematicsRecipeEntries.Add(MorePlantersPlugin.planterMk2Recipe);
        }

        private static void AddPlanterMk3() {
            planterMk3Definition = (PlanterDefinition)ScriptableObject.CreateInstance(typeof(PlanterDefinition));
            ModUtils.CloneObject(planterMk1Definition, ref planterMk3Definition);

            planterMk3Definition.description = "Houses an integrated Thresher and has the capability to self-seed";
            planterMk3Definition.rawName = MorePlantersPlugin.planterMk3Name;
            planterMk3Definition.rawSprite = ModUtils.LoadSpriteFromFile("MorePlanters.Assets.Images.Planter Mk3.png");
            planterMk3Definition.uniqueId = ModUtils.GetNewResourceID();
            planterMk3Definition.kWPowerConsumption = 500;
            planterMk3Definition.machineTier = 3;

            GameDefines.instance.resources.Add(planterMk3Definition);
            GameDefines.instance.buildableResources.Add(planterMk3Definition);
            ResourceNames.SafeResources.Add(MorePlantersPlugin.planterMk3Name);
        }

        private static void AddRecipeForPlanterMk3() {
            NewRecipeDetails details = new NewRecipeDetails() {
                craftingMethod = CraftingMethod.Assembler,
                craftTierRequired = 0,
                duration = 10,
                ingredients = new List<RecipeResourceInfo>() {
                    new RecipeResourceInfo(MorePlantersPlugin.planterMk2Name, 1),
                    new RecipeResourceInfo(ResourceNames.ThresherMKII, 1),
                    new RecipeResourceInfo(ResourceNames.Biobrick, 50)
                },
                outputs = new List<RecipeResourceInfo>() {
                    new RecipeResourceInfo(MorePlantersPlugin.planterMk3Name, 1)
                },
                sortPriority = 5
            };
            MorePlantersPlugin.planterMk3Recipe = details.ConvertToRecipe();
            MorePlantersPlugin.planterMk3Recipe.name = MorePlantersPlugin.planterMk3Name;
            MorePlantersPlugin.planterMk3Recipe.uniqueId = ModUtils.GetNewRecipeID(true);
            MorePlantersPlugin.planterMk3Recipe.unlock = planterMk1Definition.unlock;
            GameDefines.instance.schematicsRecipeEntries.Add(MorePlantersPlugin.planterMk3Recipe);
        }
    }
}
