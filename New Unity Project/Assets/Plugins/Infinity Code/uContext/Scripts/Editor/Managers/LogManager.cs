/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace InfinityCode.uContext
{
    [InitializeOnLoad]
    public static class LogManager
    {
        private const double FREQUENCY = 1;
        private const int ERROR_MODE = 16640;
        private const int ERROR_MODE2 = 8405248;
        private const int EXCEPTION_MODE = 4325632;
        private const int EXCEPTION_MODE2 = 12714240;

        private static MethodInfo endGettingEntriesMethod;
        private static MethodInfo getCountMethod;
        private static MethodInfo getEntryMethod;
        private static MethodInfo rowGotDoubleClickedMethod;
        private static MethodInfo startGettingEntriesMethod;

        private static Dictionary<int, List<Entry>> entries;
        private static double lastUpdatedTime;
        private static bool isDirty;
        private static int lastCount;

        static LogManager()
        {

            startGettingEntriesMethod = EditorTypes.logEntries.GetMethod("StartGettingEntries", BindingFlags.Static | BindingFlags.Public);
            endGettingEntriesMethod = EditorTypes.logEntries.GetMethod("EndGettingEntries", BindingFlags.Static | BindingFlags.Public);
            getEntryMethod = EditorTypes.logEntries.GetMethod("GetEntryInternal", BindingFlags.Static | BindingFlags.Public);
            getCountMethod = EditorTypes.logEntries.GetMethod("GetCount", BindingFlags.Static | BindingFlags.Public);
            rowGotDoubleClickedMethod = EditorTypes.logEntries.GetMethod("RowGotDoubleClicked", BindingFlags.Static | BindingFlags.Public);

            Application.logMessageReceived -= OnLogMessageReceived;
            Application.logMessageReceived += OnLogMessageReceived;

            Application.logMessageReceivedThreaded -= OnLogMessageReceived;
            Application.logMessageReceivedThreaded += OnLogMessageReceived;

            EditorApplication.update -= OnUpdate;
            EditorApplication.update += OnUpdate;

            entries = new Dictionary<int, List<Entry>>();

            UpdateEntries();
        }

        public static List<Entry> GetEntries(int id)
        {
            List<Entry> localEntries;
            if (entries.TryGetValue(id, out localEntries)) return localEntries;
            return null;
        }

        private static void OnLogMessageReceived(string condition, string stacktrace, LogType type)
        {
            isDirty = true;
        }

        private static void OnUpdate()
        {
            if (!Prefs.hierarchyErrorIcons) return;

            if (EditorApplication.timeSinceStartup - lastUpdatedTime > FREQUENCY)
            {
                if (!isDirty)
                {
                    int currentCount = (int) getCountMethod.Invoke(null, new object[0]);
                    if (lastCount > currentCount) isDirty = true;
                    lastCount = currentCount;
                }
            }

            if (isDirty) UpdateEntries();
        }

        private static void UpdateEntries()
        {
            entries.Clear();

            try
            {
                int count = (int)startGettingEntriesMethod.Invoke(null, new object[0]);
                object nativeEntry = Activator.CreateInstance(EditorTypes.logEntry);

                int maxRecords = Mathf.Min(count, 999);

                FieldInfo modeField = EditorTypes.logEntry.GetField("mode", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                FieldInfo instanceField = EditorTypes.logEntry.GetField("instanceID", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

                for (int i = 0; i < maxRecords; i++) 
                {
                    getEntryMethod.Invoke(null, new[] {i, nativeEntry});
                    int mode = (int) modeField.GetValue(nativeEntry);
                    if (mode != ERROR_MODE && mode != ERROR_MODE2 && 
                        mode != EXCEPTION_MODE && mode != EXCEPTION_MODE2) continue;

                    
                    int instanceID = (int)instanceField.GetValue(nativeEntry);
                    if (instanceID == 0) continue;

                    Entry entry = new Entry(nativeEntry, i);
                    Object reference = EditorUtility.InstanceIDToObject(instanceID);
                    if (reference == null) continue;

                    GameObject target = reference as GameObject;

                    if (target == null)
                    {
                        Component component = reference as Component;
                        if (component == null) continue;
                        target = component.gameObject;
                    }

                    List<Entry> localEntries;
                    int id = target.GetInstanceID();
                    if (entries.TryGetValue(id, out localEntries)) localEntries.Add(entry);
                    else entries.Add(id, new List<Entry> {entry});
                }

                EditorApplication.RepaintHierarchyWindow();
            }
            catch (Exception ex)
            {
                Log.Add(ex);
            }

            endGettingEntriesMethod.Invoke(null, new object[0]);
            lastUpdatedTime = EditorApplication.timeSinceStartup;
            isDirty = false;
        }

        public class Entry
        {
            public string message;
            private int index;

            public Entry(object nativeEntry, int index)
            {
                this.index = index;
                message = Reflection.GetFieldValue<string>(EditorTypes.logEntry, "message", nativeEntry);
            }

            public void Open()
            {
                rowGotDoubleClickedMethod.Invoke(null, new object[]{ index });
            }
        }
    }
}