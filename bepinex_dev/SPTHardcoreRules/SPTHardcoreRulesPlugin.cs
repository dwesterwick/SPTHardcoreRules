using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using EFT;
using TestClientMod;

namespace SPTHardcoreRules
{
    [BepInPlugin("com.DanW.SPTHardcoreRules", "SPTHardcoreRulesPlugin", "1.1.0.0")]
    public class SPTHardcoreRulesPlugin : BaseUnityPlugin
    {
        public static ISession CurrentSession { get; set; } = null;
        public static bool InsuranceEnabled { get; private set; } = false;

        private void Awake()
        {
            Logger.LogInfo("Loading SPTHardcoreRulesPlugin...");

            new ShowScreenPatch().Enable();
            new InsuranceScreenPatch().Enable();

            Logger.LogInfo("Loading SPTHardcoreRulesPlugin...done.");
        }
    }
}
