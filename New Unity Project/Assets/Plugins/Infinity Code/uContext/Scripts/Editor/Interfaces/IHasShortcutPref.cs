/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;

namespace InfinityCode.uContext
{
    public interface IHasShortcutPref
    {
        IEnumerable<Prefs.Shortcut> GetShortcuts();
    }
}