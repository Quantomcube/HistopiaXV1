/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;

namespace InfinityCode.uContext.Windows
{
    [Serializable]
    public class SceneHistoryItem : SearchableItem
    {
        public string name;
        public string path;

        protected override string[] GetSearchStrings()
        {
            return new[] { name };
        }
    }
}