/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using UnityEditor;
using UnityEngine;

namespace InfinityCode.uContext.Windows
{
    public partial class Bookmarks
    {
        private const string GridSizePref = Prefs.Prefix + "Bookmarks.GridSize";
        private const int gridMargin = 10;

        private int minGridSize = 47;
        private int maxGridSize = 128;
        private static int gridSize = 47;
        private static GUIStyle selectedStyle;

        private bool DrawCell(BookmarkItem item,  Rect rect)
        {
            if (!item.isMissed && item.target == null) item.TryRestoreTarget();

            bool selected = !item.isMissed && item.target != null && Selection.activeObject == item.target;
            EditorGUI.BeginDisabledGroup(item.isMissed);

            if (item.preview == null) InitPreview(item);

            ProcessCellEvents(item, rect);

            if (selected)
            {
                GUI.Box(new RectOffset(2, 2, 2, 2).Add(rect), GUIContent.none, selectedStyle);
            }

            GUI.DrawTexture(new Rect(rect.x + gridMargin, rect.y, gridSize, gridSize), item.preview);
            GUIContent content = new GUIContent(item.title);
            GUIStyle style = new GUIStyle(EditorStyles.miniLabel);
            Vector2 size = style.CalcSize(content);
            if (size.x < rect.width) style.alignment = TextAnchor.MiddleCenter;
            GUI.Label(new Rect(rect.x, rect.y + gridSize, rect.width, 20), content, style);

            EditorGUI.EndDisabledGroup();

            return true;
        }

        private void DrawGridItems(BookmarkItem[] gridItems, ref BookmarkItem removeItem)
        {
            int countCols = Mathf.FloorToInt((position.width - 30) / (gridSize + gridMargin * 2));
            int countRows = Mathf.CeilToInt(gridItems.Length / (float)countCols);
            int rowHeight = gridSize + 20;
            int width = Mathf.Min(countCols, gridItems.Length) * (gridSize + gridMargin * 2);
            int height = countRows * (rowHeight);

            float marginLeft = (position.width - width) / 2;

            GUILayout.Box(GUIContent.none, GUIStyle.none, GUILayout.Width(width), GUILayout.Height(height));
            Rect rect = GUILayoutUtility.GetLastRect();

            for (int i = 0; i < gridItems.Length; i++)
            {
                int row = i / countCols;
                int col = i % countCols;
                Rect r = new Rect(col * (gridSize + gridMargin * 2) + marginLeft, row * rowHeight + rect.y, gridSize + gridMargin * 2, rowHeight);
                BookmarkItem item = gridItems[i];
                if (!DrawCell(item, r)) removeItem = item;
            }
        }

        private void ProcessCellEvents(BookmarkItem item, Rect rect)
        {
            Event e = Event.current;
            if (!rect.Contains(e.mousePosition)) return;
            if (e.type == EventType.MouseUp)
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
            else if (e.type == EventType.MouseDrag)
            {
                if (GUIUtility.hotControl == 0)
                {
                    DragAndDrop.PrepareStartDrag();
                    DragAndDrop.objectReferences = new[] { item.target };

                    DragAndDrop.StartDrag("Drag " + item.target);
                    e.Use();
                }
            }
        }
    }
}