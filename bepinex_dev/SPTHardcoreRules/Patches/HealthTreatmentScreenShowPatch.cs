using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EFT.UI;
using SPT.Reflection.Patching;
using TMPro;

namespace SPTHardcoreRules.Patches
{
    public class HealthTreatmentScreenShowPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(HealthTreatmentServiceView).GetMethod("Show", BindingFlags.Public | BindingFlags.Instance);
        }

        [PatchPostfix]
        private static void PatchPostfix(DefaultUIButton ____applyButton, TextMeshProUGUI ____treatAllField, TextMeshProUGUI ____quickHealNote, TextMeshProUGUI ____costTotalField)
        {
            ____applyButton.GameObject.SetActive(false);
            ____treatAllField.gameObject.SetActive(false);
            
            ____quickHealNote.SetText("Post-raid healing disabled per Hardcore Rules");

            //____costTotalField.gameObject.SetActive(false);
            ____costTotalField.SetText("N/A");
        }
    }
}
