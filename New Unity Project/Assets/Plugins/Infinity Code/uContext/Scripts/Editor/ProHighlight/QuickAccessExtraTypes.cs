/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using UnityEditor;
using UnityEngine;

namespace InfinityCode.uContext.ProHighlight
{
#if !UCONTEXT_PRO
    [InitializeOnLoad]
    public static class QuickAccessExtraTypes
    {
        static QuickAccessExtraTypes()
        {
            QuickAccess.OnInvokeExternal += OnInvokeExternal;
        }

        private static void OnInvokeExternal(QuickAccessItem item)
        {
            string message = "Using " + item.typeName + " type available in uContext Pro.";
            if (EditorUtility.DisplayDialog("uContext Pro", message, "Open uContext Pro", "Cancel"))
            {
                Links.OpenPro();
            }
        }
    }
#endif
}