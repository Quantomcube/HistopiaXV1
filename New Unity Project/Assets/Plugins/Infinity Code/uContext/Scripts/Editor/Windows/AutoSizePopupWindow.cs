/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.uContext.Windows
{
    public abstract class AutoSizePopupWindow : PopupWindow
    {
        public Action<AutoSizePopupWindow> OnClose;
        public Action<Rect> OnPositionChanged;

        [NonSerialized]
        public AutoSize adjustHeight = AutoSize.ignore;

        [SerializeField]
        public bool closeOnLossFocus = true;

        [NonSerialized]
        public Vector2 scrollPosition;

        [NonSerialized]
        public bool wasMoved;

        public float maxHeight = 400;

        private bool isTooBig = false;

        private void AdjustHeight(float bottom)
        {
            Rect pos = position;
            float currentHeight = pos.height;

            if (bottom > maxHeight)
            {
                if (isTooBig) return;

                if (pos.y < 40) pos.y = 40;

                if (adjustHeight == AutoSize.bottom)
                {
                    pos.y += currentHeight - maxHeight;
                }
                else if (adjustHeight == AutoSize.center)
                {
                    pos.y -= (currentHeight - maxHeight) / 2;
                }

                pos.height = maxHeight;
                position = pos;

                isTooBig = true;
                return;
            }

            isTooBig = false;

            if (!(Mathf.Abs(bottom - currentHeight) > 1)) return;

            if (pos.y < 40) pos.y = 40;

            if (adjustHeight == AutoSize.bottom)
            {
                pos.y += currentHeight - bottom;
            }
            else if (adjustHeight == AutoSize.center)
            {
                pos.y -= (currentHeight - bottom) / 2;
            }

            pos.height = bottom;
            position = pos;
        }

        protected abstract void OnContentGUI();

        protected virtual void OnDestroy()
        {
            if (OnClose != null) OnClose(this);
        }

        protected override void OnGUI()
        {
            Event e = Event.current;

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            OnContentGUI();

            if (adjustHeight != AutoSize.ignore)
            {
                float b = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(0)).yMin;
                if (e.type == EventType.Repaint)
                {
                    float bottom = b + 5;
                    if (Mathf.Abs(bottom - position.height) > 1) AdjustHeight(bottom);
                }
            }
            EditorGUILayout.EndScrollView();
        }

        public void SetRect(Rect rect)
        {
            position = rect;
            if (OnPositionChanged != null) OnPositionChanged(rect);
        }
    }
}