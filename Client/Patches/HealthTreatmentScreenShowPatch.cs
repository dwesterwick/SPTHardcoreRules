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

namespace HardcoreRules.Patches
{
    internal class HealthTreatmentScreenShowPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(HealthTreatmentServiceView).GetMethod(nameof(HealthTreatmentServiceView.Show), BindingFlags.Public | BindingFlags.Instance);
        }

        [PatchPostfix]
        protected static void PatchPostfix(DefaultUIButton ____applyButton, UpdatableToggle ____selectAllToggle, TextMeshProUGUI ____quickHealNote, TextMeshProUGUI ____costTotalField, TextMeshProUGUI ____cashInStashField)
        {
            ____applyButton.GameObject.SetActive(false);
            ____selectAllToggle.gameObject.SetActive(false);

            ____quickHealNote.SetText("Post-raid healing disabled per Hardcore Rules");

            ____costTotalField.fontSize = ____cashInStashField.fontSize;
            ____costTotalField.SetText("N/A");
        }
    }
}
