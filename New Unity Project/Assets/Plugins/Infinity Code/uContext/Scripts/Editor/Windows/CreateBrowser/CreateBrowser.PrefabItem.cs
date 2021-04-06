/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using UnityEditor;
using UnityEngine;

namespace InfinityCode.uContext.Windows
{
    public partial class CreateBrowser
    {
        public class PrefabItem : Item
        {
            public string path;

            public PrefabItem(string label, string path)
            {
                if (label.Length < 8) return;

                this.label = label.Substring(0, label.Length - 7);
                this.path = path;
            }

            public override void Dispose()
            {
                base.Dispose();

                path = null;
                previewEditor = null;
            }

            public override void Draw()
            {
                if (content.image == null)
                {
                    GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    content.image = AssetPreview.GetAssetPreview(asset);
                    if (content.image == null) content.image = AssetPreview.GetMiniThumbnail(asset);
                }

                base.Draw();
            }

            public void DrawPreview()
            {
                if (previewPrefab != this && previewEditor != null)
                {
                    DestroyImmediate(previewEditor);
                }

                if (previewEditor == null)
                {
                    previewPrefab = this;
                    previewEditor = Editor.CreateEditor(AssetDatabase.LoadAssetAtPath<GameObject>(path));
                }

                if (previewEditor != null) previewEditor.OnInteractivePreviewGUI(GUILayoutUtility.GetRect(128, 128), Styles.grayRow);
            }

            protected override void InitContent()
            {
                _content = new GUIContent(label);
            }

            public override void OnClick()
            {
                if (instance.OnSelectPrefab != null) instance.OnSelectPrefab(path);
                instance.Close();
            }
        }
    }
}