/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using UnityEditor;

namespace InfinityCode.uContext
{
    public static class Settings
    {
        private const string UCONTEXT_SETTINGS_PATH = "Project/uContext";
        private const string CONTEXT_MENU_SETTINGS_PATH = UCONTEXT_SETTINGS_PATH + "/Context Menu";
        private const string FAVORITE_WINDOWS_SETTINGS_PATH = UCONTEXT_SETTINGS_PATH + "/Favorite Windows";
        private const string HIERARCHY_SETTINGS_PATH = UCONTEXT_SETTINGS_PATH + "/Hierarchy";
        private const string QUICK_ACCESS_SETTINGS_PATH = UCONTEXT_SETTINGS_PATH + "/Quick Access Bar";
        private const string SEARCH_WINDOWS_SETTINGS_PATH = UCONTEXT_SETTINGS_PATH + "/Search";
        private const string VIEWS_SETTINGS_PATH = UCONTEXT_SETTINGS_PATH + "/Views";

        [SettingsProvider]
        public static SettingsProvider GetContextMenuSettingsProvider()
        {
            SettingsProvider provider = new SettingsProvider(CONTEXT_MENU_SETTINGS_PATH, SettingsScope.Project)
            {
                label = "Context Menu",
                guiHandler = Prefs.ContextMenuManager.DrawWithToolbar,
                keywords = Prefs.ContextMenuManager.GetKeywords()
            };
            return provider;
        }

        [SettingsProvider]
        public static SettingsProvider GetHierarchySettingsProvider()
        {
            SettingsProvider provider = new SettingsProvider(HIERARCHY_SETTINGS_PATH, SettingsScope.Project)
            {
                label = "Hierarchy",
                guiHandler = Prefs.HierarchyIconManager.DrawWithToolbar,
                keywords = Prefs.HierarchyIconManager.GetKeywords()
            };
            return provider;
        }

        [SettingsProvider]
        public static SettingsProvider GetFavoriteWindowsSettingsProvider()
        {
            SettingsProvider provider = new SettingsProvider(FAVORITE_WINDOWS_SETTINGS_PATH, SettingsScope.Project)
            {
                label = "Favorite Windows",
                guiHandler = Prefs.FavoriteWindowsManager.DrawWithToolbar,
                keywords = Prefs.FavoriteWindowsManager.GetKeywords()
            };
            return provider;
        }

        [SettingsProvider]
        public static SettingsProvider GetQuickAccessSettingsProvider()
        {
            SettingsProvider provider = new SettingsProvider(QUICK_ACCESS_SETTINGS_PATH, SettingsScope.Project)
            {
                label = "Quick Access Bar",
                guiHandler = Prefs.QuickAccessBarManager.DrawWithToolbar,
                keywords = Prefs.QuickAccessBarManager.GetKeywords()
            };
            return provider;
        }

        [SettingsProvider]
        public static SettingsProvider GetSearchSettingsProvider()
        {
            SettingsProvider provider = new SettingsProvider(SEARCH_WINDOWS_SETTINGS_PATH, SettingsScope.Project)
            {
                label = "Search",
                guiHandler = Prefs.SearchManager.DrawWithToolbar,
                keywords = Prefs.SearchManager.GetKeywords()
            };
            return provider;
        }

        [SettingsProvider]
        public static SettingsProvider GetSettingsProvider()
        {
            SettingsProvider provider = new SettingsProvider(UCONTEXT_SETTINGS_PATH, SettingsScope.Project)
            {
                label = "uContext",
                guiHandler = Prefs.OnGUI,
                keywords = Prefs.GetKeywords()
            };
            return provider;
        }

        [SettingsProvider]
        public static SettingsProvider GetViewsSettingsProvider()
        {
            SettingsProvider provider = new SettingsProvider(VIEWS_SETTINGS_PATH, SettingsScope.Project)
            {
                label = "Views",
                guiHandler = Prefs.ViewGalleryManager.DrawWithToolbar,
                keywords = Prefs.ViewGalleryManager.GetKeywords()
            };
            return provider;
        }

        public static void OpenFavoriteWindowsSettings()
        {
            SettingsService.OpenProjectSettings(FAVORITE_WINDOWS_SETTINGS_PATH);
        }

        public static void OpenQuickAccessSettings()
        {
            SettingsService.OpenProjectSettings(QUICK_ACCESS_SETTINGS_PATH);
        }

        [MenuItem(WindowsHelper.MenuPath + "Settings", false, 122)]
        public static void OpenSettings()
        {
            SettingsService.OpenProjectSettings(UCONTEXT_SETTINGS_PATH);
        }
    }
}