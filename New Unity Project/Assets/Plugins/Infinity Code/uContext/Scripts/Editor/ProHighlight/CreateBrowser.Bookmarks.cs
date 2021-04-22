/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using InfinityCode.uContext.Windows;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.uContext.ProHighlight
{
#if !UCONTEXT_PRO
    [InitializeOnLoad]
    public static class CreateBrowserBookmarks
    {
        static CreateBrowserBookmarks()
        {
            CreateBrowser.OnInitProviders += OnInitProviders;
        }

        private static IEnumerable<CreateBrowser.Provider> OnInitProviders()
        {
            return new[]
            {
                new BookmarkProvider()
            };
        }

        public class BookmarkProvider : CreateBrowser.Provider
        {
            public override float order
            {
                get { return -1; }
            }

            public override string title
            {
                get { return "Bookmarks"; }
            }

            public override void Cache()
            {
                items = new List<CreateBrowser.Item>();
            }

            public override void Draw()
            {
                if (!Prefs.proHighlight) return;

                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
                GUILayout.Label(title);
                EditorGUILayout.Space();
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.LabelField("Creating objects from bookmarks is available in uContext PRO.", EditorStyles.wordWrappedLabel);
                if (GUILayout.Button("Open uContext Pro"))
                {
                    Links.OpenPro();
                }
            }

            public override void Filter(string pattern, List<CreateBrowser.Item> filteredItems)
            {

            }
        }
    }
#endif
}
