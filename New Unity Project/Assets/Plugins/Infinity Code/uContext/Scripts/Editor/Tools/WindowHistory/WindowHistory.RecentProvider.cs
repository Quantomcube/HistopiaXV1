/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using UnityEditor;
using UnityEngine;

namespace InfinityCode.uContext.Tools
{
    public static partial class WindowHistory
    {
        public class RecentProvider : Provider
        {
            public override float order
            {
                get { return 0; }
            }

            public override void GenerateMenu(GenericMenu menu, ref bool hasItems)
            {
                if (!Prefs.recentWindowsInToolbar || recent.Count == 0) return;

                foreach (WindowRecord window in recent)
                {
                    menu.AddItem(new GUIContent("Recent/" + window.title), false, RestoreRecentWindow, window);
                }

                hasItems = true;
            }
        }
    }
}