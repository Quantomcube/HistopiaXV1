/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.uContext.PopupWindows
{
    public abstract class PopupWindowItem : LayoutItem, ISortableLayoutItem
    {
        protected GUIContent _guiContent;

        public virtual GUIContent guiContent
        {
            get
            {
                return _guiContent;
            }
        }

        public virtual Texture icon
        {
            get { return null; }
        }

        protected virtual string label
        {
            get { return string.Empty; }
        }

        public abstract float order { get; }

        protected override void CalcSize()
        {
            GUIStyle style = Styles.buttonAlignLeft;
            _size = style.CalcSize(guiContent);
            _size.x += style.margin.horizontal;
            _size.y += style.margin.bottom;
        }

        public override void Dispose()
        {
            base.Dispose();

            _guiContent = null;
        }

        public override void Draw()
        {
            if (!GUILayout.Button(guiContent, Styles.buttonAlignLeft)) return;

            Event e = Event.current;
            if (e.button == 0)
            {
                try
                {
                    if (Prefs.popupWindowTab && Prefs.popupWindowTabModifiers == e.modifiers) ShowTab(e.mousePosition);
                    else if (Prefs.popupWindowUtility && Prefs.popupWindowUtilityModifiers == e.modifiers) ShowUtility(e.mousePosition);
                    else if (Prefs.popupWindowPopup && Prefs.popupWindowPopupModifiers == e.modifiers) ShowPopup(e.mousePosition);
                }
                catch (Exception ex)
                {
                    Log.Add(ex);
                }

                uContextMenu.Close();
            }
            else
            {
                try
                {
                    ShowContextMenu();
                }
                catch (Exception ex)
                {
                    Log.Add(ex);
                }
            }
        }

        protected override void Init()
        {
            _guiContent = new GUIContent(label, icon);
        }

        protected virtual void ShowContextMenu()
        {
            Vector2 mousePosition = Event.current.mousePosition;
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Open As Tab"), false, () =>
            {
                try
                {
                    ShowTab(mousePosition);
                }
                catch (Exception e)
                {
                    Log.Add(e);
                }
                uContextMenu.Close();
            });
            menu.AddItem(new GUIContent("Open As Utility"), false, () =>
            {
                try
                {
                    ShowUtility(mousePosition);
                }
                catch (Exception e)
                {
                    Log.Add(e);
                }
                uContextMenu.Close();
            });
            menu.AddItem(new GUIContent("Open As DropDown"), false, () =>
            {
                try
                {
                    ShowPopup(mousePosition);
                }
                catch (Exception e)
                {
                    Log.Add(e);
                }
                uContextMenu.Close();
            });
            menu.ShowAsContext();
        }

        protected virtual void ShowPopup(Vector2 mousePosition)
        {
        }

        protected virtual void ShowTab(Vector2 mousePosition)
        {
            
        }

        protected virtual void ShowUtility(Vector2 mousePosition)
        {
            
        }
    }
}