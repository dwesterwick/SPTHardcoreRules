using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HardcoreRules.Server.Internal
{
    internal static class SptDependencyLoader
    {
        // TO DO: Can this be read from build properties?
        private const string RELATIVE_PATH_TO_SPT_INSTALL_DIRECTORY = "..\\..\\";

        public static void LoadDependencies(Action loadSptDependenciesAction)
        {
            string cd = Directory.GetCurrentDirectory();

            string tempCd = Path.GetFullPath(Path.Combine(cd, RELATIVE_PATH_TO_SPT_INSTALL_DIRECTORY, "..\\..\\..\\..\\SPT"));
            if (!Directory.Exists(tempCd))
            {
                throw new DirectoryNotFoundException($"Could not find directory {tempCd}");
            }

            Directory.SetCurrentDirectory(tempCd);

            loadSptDependenciesAction();

            Directory.SetCurrentDirectory(cd);
        }
    }
}
