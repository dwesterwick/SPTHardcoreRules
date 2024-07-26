using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EFT.UI;
using SPT.Reflection.Patching;
using SPTHardcoreRules.Controllers;

namespace SPTHardcoreRules.Patches
{
    public class MenuShowPatch : ModulePatch
    {
        private static bool _displayedProfileWarning = false;

        protected override MethodBase GetTargetMethod()
        {
            // Same as SPT method to display plugin errors
            return typeof(MenuScreen).GetMethods().First(m => m.Name == nameof(MenuScreen.Show));
        }

        [PatchPostfix]
        private static void PatchPostfix()
        {
            if (!_displayedProfileWarning && !ConfigController.Config.Enabled && ConfigController.UsingHardcoreProfile)
            {
                string profileWarningMessage = "Using a hardcore profile but Hardcore Rules is disabled";

                NotificationManagerClass.DisplayWarningNotification(profileWarningMessage, EFT.Communications.ENotificationDurationType.Long);
                SPT.Common.Utils.ServerLog.Warn(LoggingController.ServerLogMessageSource, profileWarningMessage);

                _displayedProfileWarning = true;
            }
        }
    }
}
