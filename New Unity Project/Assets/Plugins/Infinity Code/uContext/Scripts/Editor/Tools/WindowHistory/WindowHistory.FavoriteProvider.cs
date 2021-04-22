/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using InfinityCode.uContext.Windows;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.uContext.Tools
{
    public static partial class WindowHistory
    {
        public class FavoriteProvider : Provider
        {
            public override float order
            {
                get { return -1; }
            }

            public override void GenerateMenu(GenericMenu menu, ref bool hasItems)
            {
                if (!Prefs.favoriteWindowsInToolbar) return;

                foreach (var window in ReferenceManager.favoriteWindows)
                {
                    menu.AddItem(new GUIContent("Favorites/" + window.title), false, window.Open);
                }
                if (ReferenceManager.favoriteWindows.Count > 0) menu.AddSeparator("Favorites/");
                menu.AddItem(new GUIContent("Favorites/Edit"), false, Settings.OpenFavoriteWindowsSettings);
                hasItems = true;
            }
        }
    }
}