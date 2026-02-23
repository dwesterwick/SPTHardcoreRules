using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EFT.UI;
using SPT.Reflection.Patching;
using HardcoreRules.Utils;
using HardcoreRules.Helpers;

namespace HardcoreRules.Patches
{
    internal class MenuShowPatch : ModulePatch
    {
        private static bool _displayedProfileWarning = false;

        protected override MethodBase GetTargetMethod()
        {
            // Same as SPT method to display plugin errors
            return typeof(MenuScreen).GetMethods().First(m => m.Name == nameof(MenuScreen.Show));
        }

        [PatchPostfix]
        protected static void PatchPostfix()
        {
            if (!_displayedProfileWarning && !ConfigUtil.CurrentConfig.IsModEnabled() && ConfigUtil.UsingHardcoreProfile)
            {
                string profileWarningMessage = "Using a hardcore profile but Hardcore Rules is disabled";

                NotificationManagerClass.DisplayWarningNotification(profileWarningMessage, EFT.Communications.ENotificationDurationType.Long);
                SPT.Common.Utils.ServerLog.Warn(ModInfo.MODNAME, profileWarningMessage);

                _displayedProfileWarning = true;
            }
        }
    }
}
