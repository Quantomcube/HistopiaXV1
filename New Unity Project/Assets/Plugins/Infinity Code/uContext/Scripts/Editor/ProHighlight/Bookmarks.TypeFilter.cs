/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using InfinityCode.uContext.Windows;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.uContext.ProHighlight
{
#if !UCONTEXT_PRO
    [InitializeOnLoad]
    public static class BookmarksTypeFilter
    {
        private static GUIContent filterByTypeContent;

        static BookmarksTypeFilter()
        {
            Bookmarks.OnToolbarMiddle += OnToolbarMiddle;
        }

        private static void OnToolbarMiddle(Bookmarks wnd)
        {
            if (!Prefs.proHighlight) return;
            if (filterByTypeContent == null) filterByTypeContent = EditorGUIUtility.IconContent("FilterByType", "Search by Type (Available in uContext PRO)");
            if (!GUILayout.Button(filterByTypeContent, EditorStyles.toolbarButton)) return;

            if (EditorUtility.DisplayDialog("Warning", "Search by Type available in uContext Pro.", "Open uContext Pro", "Cancel"))
            {
                Links.OpenPro();
            }
        }
    }

#endif
}