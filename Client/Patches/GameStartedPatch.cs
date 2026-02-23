using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SPT.Reflection.Patching;
using EFT;
using HardcoreRules.Models;

namespace HardcoreRules.Patches
{
    internal class GameStartedPatch: ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(GameWorld).GetMethod(nameof(GameWorld.OnGameStarted), BindingFlags.Public | BindingFlags.Instance);
        }

        [PatchPostfix]
        protected static void PatchPostfix()
        {
            CurrentRaidSettings.IsInRaid = true;
        }
    }
}
