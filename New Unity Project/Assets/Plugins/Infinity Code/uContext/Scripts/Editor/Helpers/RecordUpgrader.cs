/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using InfinityCode.uContext.JSON;
using InfinityCode.uContext.Windows;
using UnityEditor;

namespace InfinityCode.uContext
{
    [InitializeOnLoad]
    public static class RecordUpgrader
    {
        private const int CurrentUpgradeID = 2;
        private const string BookmarkItemSeparator = "|";

        static RecordUpgrader()
        {
            int upgradeID = ReferenceManager.upgradeID;
            if (upgradeID < 1)
            {
                UpgradeFavoriteWindows();
                UpgradeBookmarks();
                UpgradeSceneHistory();
                InitDefaultQuickAccessItems();
            }

            if (upgradeID < 2)
            {
                UpgradeEditQuickAccessItem();
            }

            ReferenceManager.upgradeID = CurrentUpgradeID;
        }

        public static void InitDefaultQuickAccessItems()
        {
            List<QuickAccessItem> items = ReferenceManager.quickAccessItems;
            if (items.Count > 0) return;

            QuickAccessItem save = new QuickAccessItem(QuickAccessItemType.menuItem)
            {
                settings = new[] { "File/Save" },
                icon = QuickAccessItemIcon.texture,
                iconSettings = Resources.iconsFolder + "Save.png",
                tooltip = "Save",
                expanded = false
            };

            QuickAccessItem hierarchy = new QuickAccessItem(QuickAccessItemType.window)
            {
                settings = new[] { EditorTypes.sceneHierarchyWindow.AssemblyQualifiedName },
                icon = QuickAccessItemIcon.texture,
                iconSettings = Resources.iconsFolder + "Hierarchy2.png",
                tooltip = "Hierarchy",
                visibleRules = SceneViewVisibleRules.onMaximized,
                expanded = false
            };

            QuickAccessItem project = new QuickAccessItem(QuickAccessItemType.window)
            {
                settings = new[] { EditorTypes.projectBrowser.AssemblyQualifiedName },
                icon = QuickAccessItemIcon.texture,
                iconSettings = Resources.iconsFolder + "Project.png",
                tooltip = "Project",
                visibleRules = SceneViewVisibleRules.onMaximized,
                expanded = false
            };

            QuickAccessItem inspector = new QuickAccessItem(QuickAccessItemType.window)
            {
                settings = new[] { EditorTypes.inspectorWindow.AssemblyQualifiedName },
                icon = QuickAccessItemIcon.texture,
                iconSettings = Resources.iconsFolder + "Inspector.png",
                tooltip = "Inspector",
                visibleRules = SceneViewVisibleRules.onMaximized,
                expanded = false
            };

            QuickAccessItem bookmarks = new QuickAccessItem(QuickAccessItemType.window)
            {
                settings = new[] { "InfinityCode.uContext.Windows.Bookmarks, uContext-Editor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null" },
                icon = QuickAccessItemIcon.texture,
                iconSettings = Resources.iconsFolder + "Star-White.png",
                tooltip = "Bookmarks",
                expanded = false
            };

            QuickAccessItem viewGallery = new QuickAccessItem(QuickAccessItemType.window)
            {
                settings = new[] { "InfinityCode.uContext.Windows.ViewGallery, uContext-Editor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null" },
                icon = QuickAccessItemIcon.editorIconContent,
                iconSettings = "d_ViewToolOrbit",
                tooltip = "View Gallery",
                expanded = false
            };

            QuickAccessItem distanceTool = new QuickAccessItem(QuickAccessItemType.window)
            {
                settings = new[] { "InfinityCode.uContext.Windows.DistanceTool, uContext-Editor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null" },
                icon = QuickAccessItemIcon.texture,
                iconSettings = Resources.iconsFolder + "Rule.png",
                tooltip = "Distance Tool",
                expanded = false
            };

            QuickAccessItem quickAccessSettings = new QuickAccessItem(QuickAccessItemType.settings)
            {
                settings = new[] { "Project/uContext/Quick Access Bar" },
                icon = QuickAccessItemIcon.editorIconContent,
                iconSettings = "d_Settings",
                tooltip = "Edit Items",
                expanded = false
            };

            QuickAccessItem info = new QuickAccessItem(QuickAccessItemType.window)
            {
                settings = new[] { "InfinityCode.uContext.Windows.Welcome, uContext-Editor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null" },
                icon = QuickAccessItemIcon.editorIconContent,
                iconSettings = "_Help",
                tooltip = "Info",
                expanded = false
            };

            items.Add(save);
            items.Add(hierarchy);
            items.Add(project);
            items.Add(inspector);
            items.Add(bookmarks);
            items.Add(viewGallery);
            items.Add(distanceTool);
            items.Add(new QuickAccessItem(QuickAccessItemType.flexibleSpace));
            items.Add(quickAccessSettings);
            items.Add(info);
        }

        private static BookmarkItem ParseBookmarkItem(int version, string line)
        {
            try
            {
                BookmarkItem item = new BookmarkItem();
                string[] parts = line.Split(new[] { BookmarkItemSeparator }, StringSplitOptions.RemoveEmptyEntries);
                int id = int.Parse(parts[0]);
                item.target = EditorUtility.InstanceIDToObject(id);
                item.globalInstanceID = GlobalObjectId.GetGlobalObjectIdSlow(item.target).ToString();
                item.title = Unescape(parts[1]);
                item.type = parts[2];
                item.path = Unescape(parts[3]);
                item.tooltip = item.path;
                if (!item.path.StartsWith("Assets/")) item.tooltip = item.tooltip.Substring(1);
                if (version == 2) item.isProjectItem = int.Parse(parts[4]) == 1;
                return item;
            }
            catch
            {

            }

            return null;
        }

        private static string Unescape(string s)
        {
            return s.Replace("%2C", BookmarkItemSeparator).Replace("%25", "%");
        }

        private static void UpgradeBookmarks()
        {
            const string Filename = "uContextBookmarks.bmk";

            if (!File.Exists(Filename)) return;

            List<BookmarkItem> items = ReferenceManager.bookmarks;
            items.Clear();
            FileStream stream = File.OpenRead(Filename);
            StreamReader reader = new StreamReader(stream);

            if (reader.EndOfStream)
            {
                reader.Close();
                return;
            }
            string versionStr = reader.ReadLine();
            int version;
            if (string.IsNullOrEmpty(versionStr) || !int.TryParse(versionStr, out version))
            {
                version = 1;
            }

            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                BookmarkItem item = ParseBookmarkItem(version, line);
                if (item != null) items.Add(item);
            }

            reader.Close();
            ReferenceManager.Save();
            File.Delete(Filename);
        }

        private static void UpgradeEditQuickAccessItem()
        {
            foreach (QuickAccessItem item in ReferenceManager.quickAccessItems)
            {
                if (item.type != QuickAccessItemType.window) continue;
                if (item.settings[0] != "InfinityCode.uContext.QuickAccessEditor, uContext-Editor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null") continue;

                item.type = QuickAccessItemType.settings;
                item.settings[0] = "Project/uContext/Quick Access Bar";
                item.typeName = null;
            }

            ReferenceManager.Save();
        }

        private static void UpgradeFavoriteWindows()
        {
            const string Filename = "uContextFavoriteWindows.json";

            if (!File.Exists(Filename)) return;
            string text = File.ReadAllText(Filename, Encoding.UTF8);
            JsonArray items = JsonArray.ParseArray(text);
            List<FavoriteWindowItem> records = ReferenceManager.favoriteWindows;
            records.Clear();
            foreach (JsonItem item in items) records.Add(new FavoriteWindowItem(item));
            ReferenceManager.Save();

            File.Delete(Filename);
        }

        private static void UpgradeSceneHistory()
        {
            const string FILENAME = "SceneHistory.json";

            if (!File.Exists(FILENAME)) return;

            ReferenceManager.sceneHistory.Clear();

            string json = File.ReadAllText(FILENAME, Encoding.UTF8);
            ReferenceManager.sceneHistory = Json.Deserialize<List<SceneHistoryItem>>(json);
            ReferenceManager.Save();

            File.Delete(FILENAME);
        }
    }
}