/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace InfinityCode.uContext
{
    public static class EditorReflected
    {
        private static MethodInfo toolbarSearchFieldMethod;
        private static MethodInfo clearCacheMethod;

        public static IList GetGameViews()
        {
#if UNITY_2019_3_OR_NEWER
            IList list = Reflection.GetStaticFieldValue<IList>(EditorTypes.playModeView, "s_PlayModeViews");
#else
            IList list = Reflection.GetStaticFieldValue<IList>(EditorTypes.gameView, "s_GameViews");
#endif
            return list;
        }

        public static string ToolbarSearchField(string value)
        {
            if (toolbarSearchFieldMethod == null)
            {
                toolbarSearchFieldMethod = typeof(EditorGUILayout).GetMethod("ToolbarSearchField", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(string), typeof(GUILayoutOption[]) }, null);
            }
            if (toolbarSearchFieldMethod != null) return toolbarSearchFieldMethod.Invoke(null, new object[] { value, null }) as string;
            return value;
        }

        public static void ClearReorderableListCache(ReorderableList list)
        {
            if (list == null) return;
            if (clearCacheMethod == null) clearCacheMethod = typeof(ReorderableList).GetMethod("ClearCache", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            clearCacheMethod.Invoke(list, null);
        }
    }
}