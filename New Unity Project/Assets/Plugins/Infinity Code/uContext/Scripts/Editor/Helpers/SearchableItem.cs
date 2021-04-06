/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace InfinityCode.uContext
{
    public abstract class SearchableItem
    {
        private const int upFactor = -1;

        protected float _accuracy;

        public float accuracy
        {
            get { return _accuracy; }
        }

        protected static bool Contains(string str1, string str2)
        {
            if (string.IsNullOrEmpty(str1) || string.IsNullOrEmpty(str2) || str2.Length > str1.Length) return false;

            int i, j;

            int l2 = str2.Length;

            for (i = 0; i < str1.Length - l2 + 1; i++)
            {
                for (j = 0; j < l2; j++)
                {
                    char c1 = char.ToUpperInvariant(str1[i + j]);
                    char c2 = char.ToUpperInvariant(str2[j]);
                    if (c1 != c2) break;
                }

                if (j == l2) return true;
            }

            return false;
        }

        public static string GetPattern(string str, out string assetType)
        {
            assetType = string.Empty;
            string search = str;

            Match match = Regex.Match(search, @"(:)(\w*)");
            if (match.Success)
            {
                assetType = match.Groups[2].Value.ToUpperInvariant();
                if (assetType == "PREFAB") assetType = "GAMEOBJECT";
                search = Regex.Replace(search, @"(:)(\w*)", "");
            }

            StaticStringBuilder.Clear();

            bool lastWhite = false;

            for (int i = 0; i < search.Length; i++)
            {
                char c = search[i];
                if (c == ' ' || c == '\t' || c == '\n')
                {
                    if (!lastWhite && StaticStringBuilder.Length > 0)
                    {
                        StaticStringBuilder.Append(' ');
                        lastWhite = true;
                    }
                }
                else
                {
                    StaticStringBuilder.Append(char.ToUpperInvariant(c));
                    lastWhite = false;
                }
            }

            if (lastWhite) StaticStringBuilder.Length -= 1;

            return StaticStringBuilder.GetString();
        }

        protected abstract string[] GetSearchStrings();

        protected static int Match(string str1, string str2)
        {
            int bestExtra = int.MaxValue;

            int l1 = str1.Length;
            int l2 = str2.Length;

            for (int i = 0; i < l1 - l2 + 1; i++)
            {
                bool success = true;
                int iOffset = 0;
                char c1 = str1[i];
                int extra = char.IsUpper(c1) ? upFactor: 0;

                for (int j = 0; j < l2; j++)
                {
                    char c = str2[j];

                    int i1 = i + iOffset + j;
                    if (i1 >= l1)
                    {
                        success = false;
                        break;
                    }

                    c1 = char.ToUpperInvariant(str1[i1]);

                    if (c != c1)
                    {
                        if (j == 0)
                        {
                            success = false;
                            break;
                        }

                        bool successSkip = false;
                        bool searchZeroExtra = false;
                        int skipCount = 1;
                        iOffset++;
                        int ciOffset = 0;

                        while (i + j + iOffset < l1)
                        {
                            char oc = str1[i + iOffset + j];
                            char uc = char.ToUpperInvariant(oc);
                            if (uc == c)
                            {
                                if (!char.IsUpper(oc))
                                {
                                    ciOffset = iOffset;
                                    iOffset++;
                                    if (!char.IsDigit(oc)) searchZeroExtra = true;
                                }
                                else extra += upFactor;

                                successSkip = true;
                                break;
                            }
                            iOffset++;
                            skipCount++;
                        }

                        if (searchZeroExtra)
                        {
                            bool successZeroOffset = false;
                            while (i + j + iOffset < l1)
                            {
                                char oc = str1[i + iOffset + j];
                                if (oc == c)
                                {
                                    successZeroOffset = true;
                                    extra += upFactor;
                                    break;
                                }
                                iOffset++;
                            }

                            if (!successZeroOffset)
                            {
                                iOffset = ciOffset;
                                extra += skipCount;
                            }
                        }

                        if (!successSkip)
                        {
                            success = false;
                            break;
                        }
                    }
                }

                if (success)
                {
                    if (extra == l2 * upFactor) return extra;
                    bestExtra = Math.Min(extra, bestExtra);
                }
            }

            return bestExtra != int.MaxValue ? bestExtra : int.MinValue;
        }

        public virtual float UpdateAccuracy(string pattern)
        {
            _accuracy = 0;

            if (string.IsNullOrEmpty(pattern))
            {
                _accuracy = 1;
                return 1;
            }

            string[] search = GetSearchStrings();

            for (int i = 0; i < search.Length; i++)
            {
                string s = search[i];
                int r = Match(s, pattern);
                if (r != int.MinValue)
                {
                    _accuracy = Mathf.Max(accuracy, 1 - r / (float)s.Length);
                    if (r == pattern.Length * upFactor) return accuracy;
                }
            }

            return _accuracy;
        }
    }
}