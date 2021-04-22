/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using InfinityCode.uContext.Windows;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.uContext.ProHighlight
{
#if !UCONTEXT_PRO
    [InitializeOnLoad]
    public static class ViewGalleryCameras
    {
        static ViewGalleryCameras()
        {
            ViewGallery.OnDrawCameras += OnDrawCameras;
        }

        private static void OnDrawCameras(ViewGallery gallery, float rowHeight, float maxLabelWidth, ref int offsetY, ref int row)
        {
            if (!Prefs.proHighlight) return;

            GUI.Label(new Rect(5, offsetY, gallery.position.width - 10, 20), "View from cameras available in uContext Pro.", EditorStyles.boldLabel);

            offsetY += 20;

            if (GUI.Button(new Rect(5, offsetY, gallery.position.width - 10, 16), "Open uContext Pro")) Links.OpenPro();

            offsetY += 20;
        }
    }
#endif
}