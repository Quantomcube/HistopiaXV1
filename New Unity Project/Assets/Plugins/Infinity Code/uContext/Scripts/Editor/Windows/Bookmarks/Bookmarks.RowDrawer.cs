/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using UnityEditor;
using UnityEngine;

namespace InfinityCode.uContext.Windows
{
    public partial class Bookmarks
    {
        private static GUIStyle audioClipButtonStyle;
        private static GUIContent closeContent;
        private static GUIContent showContent;
        private static GUIStyle showContentStyle;

        private bool DrawRow(BookmarkItem item)
        {
            bool returnVal = true;

            if (!item.isMissed && item.target == null) item.TryRestoreTarget();

            bool selected = !item.isMissed && item.target != null && Selection.activeObject == item.target;
            EditorGUILayout.BeginHorizontal(selected ? Styles.selectedRow : Styles.transparentRow);

            EditorGUI.BeginDisabledGroup(item.isMissed);
            DrawRowFirstButton(item);
            DrawRowSecondButton(item);

            ButtonEvent event1 = DrawRowPreview(item);

            GUIContent content = new GUIContent(item.title, !item.isMissed ? GetShowTooltip(item) : "(Missed) " + GetShowTooltip(item));
            ButtonEvent event2 = GUILayoutUtils.Button(content, EditorStyles.label, GUILayout.Height(20), GUILayout.MaxWidth(instance.position.width - 100));

            ProcessRowEvents(item, event1, event2);

            EditorGUI.EndDisabledGroup();

            if (folderItems == null && GUILayout.Button(closeContent, Styles.transparentButton, GUILayout.ExpandWidth(false), GUILayout.Height(12))) returnVal = false;

            EditorGUILayout.EndHorizontal();

            return returnVal;
        }

        private void DrawRowFirstButton(BookmarkItem item)
        {
            showContent.tooltip = GetShowTooltip(item);
            if (GUILayout.Button(showContent, showContentStyle, GUILayout.ExpandWidth(false), GUILayout.Height(12)))
            {
                if (item.target is Component) ComponentWindow.Show(item.target as Component);
                else if (item.target as GameObject)
                {
                    if (item.isProjectItem)
                    {
                        GameObjectUtils.OpenPrefab(item.path);
                    }
                    else
                    {
                        Selection.activeGameObject = item.gameObject;
                        WindowsHelper.ShowInspector();
                    }
                }
                else EditorUtility.OpenWithDefaultApp(AssetDatabase.GetAssetPath(item.target));
            }
        }

        private ButtonEvent DrawRowPreview(BookmarkItem item)
        {
            if (item.preview == null) InitPreview(item);

            ButtonEvent event1 = GUILayoutUtils.Button(item.preview, GUIStyle.none, GUILayout.Height(20), GUILayout.Width(20));
            return event1;
        }

        private void DrawRowSecondButton(BookmarkItem item)
        {
            if (item.isProjectItem)
            {
                if (item.target is AudioClip) PlayStopAudioClipButton(item);
                else GUILayout.Space(20);
            }
            else if (isGameObjectHidden != null && item.gameObject != null)
            {
                bool hidden = (bool)isGameObjectHidden.Invoke(null, new object[] { item.gameObject });
                if (GUILayout.Button(hidden ? hiddenContent : visibleContent, Styles.transparentButton, GUILayout.ExpandWidth(false)))
                {
                    toggleGameObjectVisibility.Invoke(sceneVisibilityManagerInstanceProp.GetValue(null, null), new object[] { item.gameObject, true });
                }
            }
            else
            {
                GUILayout.Space(20);
            }
        }

        private void DrawTreeItems(BookmarkItem[] treeItems, ref BookmarkItem removeItem)
        {
            foreach (BookmarkItem item in treeItems)
            {
                if (!DrawRow(item)) removeItem = item;
            }
        }

        private string GetShowTooltip(BookmarkItem item)
        {
            string tooltip = "Show";

            if (item.isProjectItem)
            {
                if (item.target is GameObject) tooltip = "Open Prefab";
                else if (item.target is Component) tooltip = "Open Component Window";
                else if (item.target is DefaultAsset) tooltip = "Show In Explorer";
                else tooltip = "Open in default application";
            }
            else
            {
                if (item.target is GameObject) tooltip = "Open Inspector Window";
                else if (item.target is Component) tooltip = "Open Component Window";
            }

            return tooltip;
        }

        private void PlayStopAudioClipButton(BookmarkItem item)
        {
            AudioClip audioClip = item.target as AudioClip;
            bool isPlayed = AudioUtils.IsClipPlaying(audioClip);
            GUIContent playContent = isPlayed ? EditorIconContents.pauseButtonOn : EditorIconContents.playButtonOn;

            if (audioClipButtonStyle == null)
            {
                audioClipButtonStyle = new GUIStyle();
                audioClipButtonStyle.margin.top = 3;
            }

            if (GUILayout.Button(playContent, audioClipButtonStyle, GUILayout.Width(20)))
            {
                if (isPlayed) AudioUtils.StopClip(audioClip);
                else AudioUtils.PlayClip(audioClip);
            }
        }

        private void ProcessRowEvents(BookmarkItem item, ButtonEvent event1, ButtonEvent event2)
        {
            Event e = Event.current;
            if (event1 == ButtonEvent.click || event2 == ButtonEvent.click)
            {
                if (e.button == 0)
                {
                    if (Selection.activeObject == item.target)
                    {
                        ProcessDoubleClick(item);
                    }
                    else
                    {
                        lastClickTime = EditorApplication.timeSinceStartup;
                        Selection.activeObject = item.target;
                        EditorGUIUtility.PingObject(item.target);
                    }

                    e.Use();
                }
                else if (e.button == 1)
                {
                    ShowContextMenu(item);
                    e.Use();
                }
            }
            else if (event1 == ButtonEvent.drag || event2 == ButtonEvent.drag)
            {
                DragAndDrop.PrepareStartDrag();
                DragAndDrop.objectReferences = new[] { item.target };

                DragAndDrop.StartDrag("Drag " + item.target);
                e.Use();
            }
        }
    }
}