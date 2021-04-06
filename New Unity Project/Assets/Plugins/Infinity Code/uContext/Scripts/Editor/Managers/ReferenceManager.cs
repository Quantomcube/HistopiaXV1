/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Collections.Generic;
using System.IO;
using InfinityCode.uContext.Windows;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.uContext
{
    [Serializable]
    public class ReferenceManager : ScriptableObject
    {
        [SerializeField]
        private List<BookmarkItem> _bookmarks = new List<BookmarkItem>();

        [SerializeField]
        private List<FavoriteWindowItem> _favoriteWindows = new List<FavoriteWindowItem>();

        [SerializeField]
        private List<QuickAccessItem> _quickAccessItems = new List<QuickAccessItem>();

        [SerializeField]
        private List<SceneHistoryItem> _sceneHistory = new List<SceneHistoryItem>();

        [SerializeField]
        private int _upgradeID = 0;

        private static ReferenceManager _instance;

        private static ReferenceManager instance
        {
            get
            {
                if (_instance == null) Load();
                return _instance;
            }
        }

        public static List<BookmarkItem> bookmarks
        {
            get { return instance._bookmarks; }
        }

        public static List<FavoriteWindowItem> favoriteWindows
        {
            get { return instance._favoriteWindows; }
        }

        public static List<QuickAccessItem> quickAccessItems
        {
            get { return instance._quickAccessItems; }
        }

        public static List<SceneHistoryItem> sceneHistory
        {
            get { return instance._sceneHistory; }
            set { instance._sceneHistory = value; }
        }

        public static int upgradeID
        {
            get { return instance._upgradeID; }
            set
            {
                if (_instance._upgradeID >= value) return;

                _instance._upgradeID = value;
                Save();
            }
        }

        private static void Load()
        {
            string path = Resources.assetFolder + "Settings/References.asset";
            try
            {
                if (File.Exists(path))
                {
                    try
                    {
                        _instance = AssetDatabase.LoadAssetAtPath<ReferenceManager>(path);
                    }
                    catch (Exception e)
                    {
                        Log.Add(e);
                    }

                }

                if (_instance == null)
                {
                    _instance = CreateInstance<ReferenceManager>();

#if !UCONTEXT_IGNORE_SETTINGS
                    FileInfo info = new FileInfo(path);
                    if (!info.Directory.Exists) info.Directory.Create();

                    AssetDatabase.CreateAsset(_instance, path);
                    AssetDatabase.SaveAssets();
#endif
                }
            }
            catch (Exception e)
            {
                Log.Add(e);
            }
        }

        public static void Save()
        {
            try
            {
                EditorUtility.SetDirty(_instance); 
            }
            catch
            {

            }

        }
    }
}