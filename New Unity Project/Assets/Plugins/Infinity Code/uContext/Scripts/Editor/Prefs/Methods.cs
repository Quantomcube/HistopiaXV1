/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using InfinityCode.uContext.Windows;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace InfinityCode.uContext
{
    public static partial class Prefs
    {
        private const string PrefsKey = "uContext";
        public const string Prefix = PrefsKey + ".";

        public static Action<GenericMenu> OnInsertProMenuItems;

        private static PrefManager[] _managers;
        private static string[] _keywords;
        private static PrefManager[] _generalManagers;
        private static string[] escapeChars = {"%", "%25", ";", "%3B", "(", "%28", ")", "%29"};
        private static bool forceSave = false;
        private static Vector2 scrollPosition;

        internal static PrefManager[] managers
        {
            get
            {
                if (_managers == null)
                {
                    List<PrefManager> items = Reflection.GetInheritedItems<PrefManager>();
                    _managers = items.OrderBy(d => d.order).ToArray();
                }
                return _managers;
            }
        }


        internal static PrefManager[] generalManagers
        {
            get
            {
                if (_generalManagers == null)
                {
                    _generalManagers = managers.Where(i => !i.GetType().IsSubclassOf(typeof(StandalonePrefManager))).ToArray();
                }
                return _generalManagers;
            }
        }

        static Prefs()
        {
            Load();
        }

        private static void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Restore default settings", EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
            {
                if (EditorUtility.DisplayDialog("Restore default settings", "Are you sure you want to restore the default settings?", "Restore", "Cancel"))
                {
                    if (EditorPrefs.HasKey(PrefsKey)) EditorPrefs.DeleteKey(PrefsKey);
                    AssetDatabase.ImportAsset(Resources.assetFolder + "Scripts/Editor/Prefs/Methods.cs", ImportAssetOptions.ForceUpdate);
                }
            }

            GUILayout.Label("", EditorStyles.toolbarButton, GUILayout.ExpandWidth(true));
            if (GUILayout.Button("Help", EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem( new GUIContent("Welcome"), false, Welcome.OpenWindow);
                menu.AddItem( new GUIContent("Getting Started"), false, GettingStarted.OpenWindow);
                menu.AddItem( new GUIContent("Shortcuts"), false, Shortcuts.OpenWindow);
                menu.AddSeparator("");
                menu.AddItem( new GUIContent("Product Page"), false, Links.OpenHomepage);
                menu.AddItem( new GUIContent("Documentation"), false, Links.OpenDocumentation);
                menu.AddItem( new GUIContent("Videos"), false, Links.OpenYouTube);
                menu.AddSeparator("");
                menu.AddItem( new GUIContent("Support"), false, Links.OpenSupport);
                menu.AddItem( new GUIContent("Forum"), false, Links.OpenForum);

                if (OnInsertProMenuItems != null) OnInsertProMenuItems(menu);

                menu.AddSeparator("");
                menu.AddItem( new GUIContent("Rate and Review"), false, Welcome.RateAndReview);
                menu.AddItem( new GUIContent("About"), false, About.OpenWindow);

                menu.ShowAsContext();
            }

            EditorGUILayout.EndHorizontal();
        }

        private static FieldInfo GetField(FieldInfo[] fields, string key)
        {
            for (int i = 0; i < fields.Length; i++)
            {
                if (fields[i].Name == key) return fields[i];
            }

            return null;
        }

        public static IEnumerable<string> GetKeywords()
        {
            if (_keywords == null) _keywords = generalManagers.SelectMany(m => m.keywords).ToArray();
            return _keywords;
        }

        private static void Load()
        {
            string prefStr = EditorPrefs.GetString(PrefsKey, String.Empty);
            if (string.IsNullOrEmpty(prefStr)) return;

            Type prefType = typeof(Prefs);
            FieldInfo[] fields = prefType.GetFields(BindingFlags.Static | BindingFlags.Public);

            int i = 0;
            try
            {
                LoadFields(prefStr, fields, ref i, null);
            }
            catch (Exception e)
            {
                Log.Add(e); 
            }
        }

        private static void LoadFields(string prefStr, FieldInfo[] fields, ref int i, object target)
        {
            StaticStringBuilder.Clear();
            bool isKey = true;
            string key = null;

            while (i < prefStr.Length)
            {
                char c = prefStr[i];
                i++;
                if (c == ':' && isKey)
                {
                    key = StaticStringBuilder.GetString();
                    isKey = false;
                    StaticStringBuilder.Clear();
                }
                else if (c == ';')
                {
                    string value = StaticStringBuilder.GetString();
                    isKey = true;
                    SetValue(target, fields, key, value);
                    StaticStringBuilder.Clear();
                }
                else if (c == '(')
                {
                    FieldInfo field = GetField(fields, key);
                    if (field == null || (field.FieldType.IsValueType && field.FieldType.IsPrimitive) || field.FieldType == typeof(string))
                    {
                        int indent = 1;
                        i++;
                        while (indent > 0 && i < prefStr.Length)
                        {
                            c = prefStr[i];
                            if (c == ')') indent--;
                            else if (c == '(') indent++;
                            i++;
                        }

                        isKey = true;
                    }
                    else
                    {
                        Type type = field.FieldType; 
                        object newTarget = Activator.CreateInstance(type, false); 

                        BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance;
                        if (type == typeof(Vector2Int)) bindingFlags |= BindingFlags.NonPublic;

                        FieldInfo[] objFields = type.GetFields(bindingFlags);

                        LoadFields(prefStr, objFields, ref i, newTarget);
                        field.SetValue(target, newTarget);
                        i++;
                        isKey = true;
                    }
                }
                else if (c == ')')
                {
                    string value = StaticStringBuilder.GetString();
                    SetValue(target, fields, key, value);
                    StaticStringBuilder.Clear();
                    return;
                }
                else StaticStringBuilder.Append(c);
            }
        }

        public static string ModifierToString(EventModifiers modifiers)
        {
            StaticStringBuilder.Clear();
            if ((modifiers & EventModifiers.Control) == EventModifiers.Control) StaticStringBuilder.Append("CTRL");
            if ((modifiers & EventModifiers.Command) == EventModifiers.Command)
            {
                if (StaticStringBuilder.Length > 0) StaticStringBuilder.Append(" + ");
                StaticStringBuilder.Append("CMD");
            }
            if ((modifiers & EventModifiers.Shift) == EventModifiers.Shift)
            {
                if (StaticStringBuilder.Length > 0) StaticStringBuilder.Append(" + ");
                StaticStringBuilder.Append("SHIFT");
            }
            if ((modifiers & EventModifiers.Alt) == EventModifiers.Alt)
            {
                if (StaticStringBuilder.Length > 0) StaticStringBuilder.Append(" + ");
                StaticStringBuilder.Append("ALT");
            }
            if ((modifiers & EventModifiers.FunctionKey) == EventModifiers.FunctionKey)
            {
                if (StaticStringBuilder.Length > 0) StaticStringBuilder.Append(" + ");
                StaticStringBuilder.Append("FN");
            }

            string v = StaticStringBuilder.GetString();
            StaticStringBuilder.Clear();
            return v;
        }

        public static string ModifierToString(EventModifiers modifiers, string extra)
        {
            string v = ModifierToString(modifiers);
            if (!string.IsNullOrEmpty(v)) v += " + ";
            v += extra;
            return v;
        }

        public static string ModifierToString(EventModifiers modifiers, KeyCode keycode)
        {
            return ModifierToString(modifiers, keycode.ToString());
        }

        public static void OnGUI(string searchContext)
        {
            DrawToolbar();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            foreach (PrefManager manager in generalManagers)
            {
                try
                {
                    EditorGUI.BeginChangeCheck();
                    manager.Draw();
                    EditorGUILayout.Space();
                    if (EditorGUI.EndChangeCheck() || forceSave)
                    {
                        Save();
                        forceSave = false;
                    }
                }
                catch
                {
                    
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private static void Save() 
        {
            FieldInfo[] fields = typeof(Prefs).GetFields(BindingFlags.Static | BindingFlags.Public);
            StaticStringBuilder.Clear();

            try
            {
                SaveFields(fields, null);
                string value = StaticStringBuilder.GetString();
                EditorPrefs.SetString(PrefsKey, value);
            }
            catch (Exception e)
            {
                Log.Add(e);
            }

            StaticStringBuilder.Clear();
        }

        private static void SaveFields(FieldInfo[] fields, object target)
        {
            for (int i = 0; i < fields.Length; i++)
            {
                FieldInfo field = fields[i];
                if (field.IsLiteral || field.IsInitOnly) continue; 
                object value = field.GetValue(target); 

                if (value == null) continue; 

                if (i > 0) StaticStringBuilder.Append(";");
                StaticStringBuilder.Append(field.Name).Append(":");

                Type type = value.GetType();
                if (type == typeof(string)) StaticStringBuilder.AppendEscaped(value as string, escapeChars);
                else if (type.IsEnum) StaticStringBuilder.Append(value.ToString());
                else if (type.IsValueType && type.IsPrimitive) StaticStringBuilder.Append(value);
                else
                {
                    BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public;
                    if (type == typeof(Vector2Int)) bindingFlags |= BindingFlags.NonPublic;
                    FieldInfo[] objFields = type.GetFields(bindingFlags);
                    if (objFields.Length == 0) continue;

                    StaticStringBuilder.Append("(");

                    SaveFields(objFields, value);

                    StaticStringBuilder.Append(")");
                }
            }
        }

        private static void SetValue(object target, FieldInfo[] fields, string key, object value)
        {
            FieldInfo field = GetField(fields, key);
            if (field == null) return;

            Type type = field.FieldType;
            if (type == typeof(string))
            {
                field.SetValue(target, Unescape(value as string, escapeChars));
            }
            else if (field.FieldType.IsEnum)
            {
                string strVal = value as string;
                if (strVal != null)
                {
                    try
                    {
                        value = Enum.Parse(type, strVal);
                        field.SetValue(target, value);
                    }
                    catch
                    {
                        Debug.Log("Some exception");
                    }
                }
            }
            else if (type.IsValueType)
            {
                try
                {
                    MethodInfo method = type.GetMethod("Parse", new[] { typeof(string) });
                    if (method == null)
                    {
                        Debug.Log("No parse for " + key); 
                        return;
                    }
                    value = method.Invoke(null, new[] { value });
                    if (value != null) field.SetValue(target, value); 
                }
                catch
                {

                }
            }
        }

        private static string Unescape(string value, string[] escapeCodes)
        {
            if (escapeChars == null || escapeChars.Length % 2 != 0) throw new Exception("Length of escapeCodes must be N * 2");

            StaticStringBuilder.Clear();

            bool success = true;

            for (int i = 0; i < value.Length; i++)
            {
                success = false;
                for (int j = 0; j < escapeCodes.Length; j += 2)
                {
                    string code = escapeCodes[j + 1];
                    if (value.Length - i - code.Length <= 0) continue;

                    success = true;

                    for (int k = 0; k < code.Length; k++)
                    {
                        if (code[k] != value[i + k])
                        {
                            success = false;
                            break;
                        }
                    }

                    if (success)
                    {
                        StaticStringBuilder.Append(escapeCodes[j]);
                        i += code.Length - 1;
                        break;
                    }
                }

                if (!success) StaticStringBuilder.Append(value[i]);
            }

            value = StaticStringBuilder.GetString();
            StaticStringBuilder.Clear();

            return value;
        }
    }
}