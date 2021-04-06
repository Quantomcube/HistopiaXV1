/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.IO;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.uContext
{
    public static class Resources
    {
        private static string _assetFolder;
        private static string _iconsFolder;

        public static string assetFolder
        {
            get
            {
                if (string.IsNullOrEmpty(_assetFolder))
                {
                    string[] assets = AssetDatabase.FindAssets("uContextMenu");
                    FileInfo info = new FileInfo(AssetDatabase.GUIDToAssetPath(assets[0]));
                    _assetFolder = info.Directory.Parent.Parent.Parent.FullName.Substring(Application.dataPath.Length - 6) + "/";
                }

                return _assetFolder;
            } 
        }

        public static string iconsFolder
        {
            get
            {
                if (string.IsNullOrEmpty(_iconsFolder)) _iconsFolder = assetFolder + "Icons/";

                return _iconsFolder;
            }
        }

        public static Texture2D CreateSinglePixelTexture(byte r, byte g, byte b, byte a)
        {
            return CreateSinglePixelTexture(new Color32(r, g, b, a));
        }

        public static Texture2D CreateSinglePixelTexture(float v, float a = 1)
        {
            return CreateSinglePixelTexture(v, v, v, a);
        }

        public static Texture2D CreateSinglePixelTexture(float r, float g, float b, float a)
        {
            return CreateSinglePixelTexture(new Color(r, g, b, a));
        }

        public static Texture2D CreateSinglePixelTexture(Color color)
        {
            Texture2D t = new Texture2D(1, 1);
            t.SetPixel(0, 0, color);
            t.Apply();
            return t;
        }

        public static T Load<T>(string path) where T: Object
        {
            return AssetDatabase.LoadAssetAtPath<T>(assetFolder + path);
        }

        public static Texture2D LoadIcon(string path, string ext = ".png")
        {
            return AssetDatabase.LoadAssetAtPath<Texture2D>(iconsFolder + path + ext);
        }

        public static void Unload(Object asset)
        {
            if (asset != null) UnityEngine.Resources.UnloadAsset(asset);
        }
    }
}