/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Linq;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.uContext.Tools
{
    public static partial class WindowHistory
    {
        public class OpenedProvider : Provider
        {
            public override float order
            {
                get { return 1000; }
            }

            public override void GenerateMenu(GenericMenu menu, ref bool hasItems)
            {
                if (hasItems) menu.AddSeparator("");

                foreach (WindowRecord window in windows.Values.OrderBy(w => w.title))
                {
                    menu.AddItem(new GUIContent(window.title), lastFocusedWindowType == window.type, FocusWindow, window);
                }
            }
        }
    }
}

