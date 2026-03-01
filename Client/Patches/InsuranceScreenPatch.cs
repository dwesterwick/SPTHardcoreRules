using Comfort.Common;
using HardcoreRules.Utils;
using HarmonyLib;
using SPT.Reflection.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace HardcoreRules.Patches
{
    internal class InsuranceScreenPatch: ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(MainMenuControllerClass).GetMethod("method_80", BindingFlags.Public | BindingFlags.Instance);
        }

        [PatchTranspiler]
        protected static IEnumerable<CodeInstruction> PatchTranspiler(IEnumerable<CodeInstruction> originalInstructions)
        {
            MethodInfo showInsuranceScreenMethodInfo = typeof(MainMenuControllerClass).GetMethod("method_51", BindingFlags.Public | BindingFlags.Instance);
            MethodInfo showAcceptScreenMethodInfo = typeof(MainMenuControllerClass).GetMethod("method_52", BindingFlags.Public | BindingFlags.Instance);

            List<CodeInstruction> modifiedInstructions = originalInstructions.ToList();

            for (int i = 0; i < modifiedInstructions.Count; i++)
            {
                if ((modifiedInstructions[i].opcode == OpCodes.Call) && ((MethodInfo)modifiedInstructions[i].operand == showInsuranceScreenMethodInfo))
                {
                    Singleton<LoggingUtil>.Instance.LogInfo("Disabling insurance screen...");

                    modifiedInstructions[i].operand = showAcceptScreenMethodInfo;
                }
            }

            return modifiedInstructions;
        }
    }
}
