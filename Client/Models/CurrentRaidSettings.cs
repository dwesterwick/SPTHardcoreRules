using EFT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HardcoreRules.Models
{
    public static class CurrentRaidSettings
    {
        public static ESideType SelectedSide { get; set; } = ESideType.Pmc;
        public static bool IsInRaid { get; set; } = false;
    }
}
