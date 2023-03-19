using BepInEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTHardcoreRules
{
    [BepInPlugin("com.DanW.SPTHardcoreRules", "SPTHardcoreRulesPlugin", "1.1.0.0")]
    public class SPTHardcoreRulesPlugin : BaseUnityPlugin
    {
        public static ISession CurrentSession { get; set; } = null;

        private void Awake()
        {
            Logger.LogInfo("Loading SPTHardcoreRulesPlugin...");

            Logger.LogInfo("Loading SPTHardcoreRulesPlugin...done.");
        }
    }
}
