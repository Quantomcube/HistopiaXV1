/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using InfinityCode.uContext.MainLayout;
using UnityEngine;

namespace InfinityCode.uContext.Windows
{
    public class LayoutWindow: PopupWindow
    {
        private MainLayoutItem item;

        protected void OnDestroy()
        {
            item = null;
        }

        protected override void OnGUI()
        {
            if (item == null)
            {
                uContextMenu.Close();
                return;
            }

            try
            {
                base.OnGUI();
                item.OnGUI();
            }
            catch (Exception e)
            {
                Log.Add(e);
            }
        }

        public static LayoutWindow Show(MainLayoutItem item, Rect rect)
        {
            LayoutWindow wnd = CreateInstance<LayoutWindow>();
            wnd.item = item;
            wnd.position = rect;
            wnd.ShowPopup();
            wnd.Focus();
            return wnd;
        }
    }
}
 