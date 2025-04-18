﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SPT.Reflection.Patching;
using EFT;
using EFT.UI.Matchmaker;
using SPTHardcoreRules.Controllers;
using SPTHardcoreRules.Models;

namespace SPTHardcoreRules.Patches
{
    public class UpdateSideSelectionPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            string methodName = "UpdateSideSelection";

            IEnumerable<Type> matchMakerSideSelectionScreenTypes = typeof(MatchMakerSideSelectionScreen).GetNestedTypes();
            Type targetType = FindTargetType(matchMakerSideSelectionScreenTypes, methodName);
            LoggingController.LogInfo("Found type for UpdateSideSelectionPatch: " + targetType.FullName);

            return targetType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
        }

        [PatchPostfix]
        protected static void PatchPostfix(ESideType side)
        {
            CurrentRaidSettings.SelectedSide = side;
        }

        public static Type FindTargetType(IEnumerable<Type> allTypes, string methodName)
        {
            List<Type> targetTypeOptions = allTypes
                .Where(t => t.GetMethods().Any(m => m.Name.Contains(methodName)))
                .ToList();

            if (targetTypeOptions.Count != 1)
            {
                throw new TypeLoadException("Cannot find any type containing method " + methodName);
            }

            return targetTypeOptions[0];
        }
    }
}
