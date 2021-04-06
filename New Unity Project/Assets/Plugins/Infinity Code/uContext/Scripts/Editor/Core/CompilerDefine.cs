/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using UnityEditor;

namespace InfinityCode.uContext
{
    [InitializeOnLoad]
    public class CompilerDefine : Editor
    {
        private const string key = "UCONTEXT";

        static CompilerDefine()
        {
            string symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            if (!string.IsNullOrEmpty(symbols))
            {
                string[] keys = symbols.Split(';');
                foreach (string k in keys)
                {
                    if (k == key) return;
                }
            }

            symbols += ";" + key;
            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, symbols);
        }
    }
}