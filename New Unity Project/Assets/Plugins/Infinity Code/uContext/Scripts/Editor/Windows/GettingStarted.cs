/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.uContext.Windows
{
    public class GettingStarted : EditorWindow
    {
        private List<Texture2D> slides;
        private int slideIndex = 0;
        private GUIStyle labelStyle;

        private void OnDisable()
        {
            if (slides == null) return;

            foreach (Texture2D texture in slides)
            {
                if (texture != null) UnityEngine.Resources.UnloadAsset(texture);
            }

            slides = null;
        }

        private void OnEnable()
        {
            string folder = Resources.assetFolder + "Textures/Getting Started/";
            string[] files = Directory.GetFiles(folder, "*.png");

            slides = new List<Texture2D>();

            foreach (string file in files)
            {
                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(file);
                if (texture != null) slides.Add(texture);
            }

            minSize = new Vector2(604, 454);
            maxSize = new Vector2(604, 454);

            UpdateTitle();
        }

        [MenuItem(WindowsHelper.MenuPath + "Getting Started", false, 121)]
        public static void OpenWindow()
        {
            GetWindow<GettingStarted>(true, "Getting Started", true);
        }

        public void OnGUI()
        {
            Event e = Event.current;
            if (e.type == EventType.KeyDown)
            {
                if (e.keyCode == KeyCode.Space || e.keyCode == KeyCode.RightArrow)
                {
                    slideIndex++;
                    if (slideIndex == slides.Count) slideIndex = 0;
                }
                else if (e.keyCode == KeyCode.Backspace || e.keyCode == KeyCode.LeftArrow)
                {
                    slideIndex--;
                    if (slideIndex < 0) slideIndex = slides.Count - 1;
                }
                
                UpdateTitle();
                Repaint();
            }
            else if (e.type == EventType.MouseUp)
            {
                if (e.button == 0)
                {
                    slideIndex++;
                    if (slideIndex == slides.Count) slideIndex = 0;
                }
                else if (e.button == 1)
                {
                    slideIndex--;
                    if (slideIndex < 0) slideIndex = slides.Count - 1;
                }

                UpdateTitle();

                Repaint();
            }

            GUI.DrawTexture(new Rect(2, 2, position.width - 4, position.height - 4), slides[slideIndex]);
        }

        private void UpdateTitle()
        {
            titleContent = new GUIContent("Getting Started. Frame " + (slideIndex + 1) + " / " + slides.Count + " (click to continue)");
        }
    }
}