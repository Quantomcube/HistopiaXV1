/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Globalization;

namespace InfinityCode.uContext
{
    public static class Culture
    {
        public static CultureInfo cultureInfo
        {
            get { return CultureInfo.InvariantCulture; }
        }

        public static NumberFormatInfo numberFormat
        {
            get { return cultureInfo.NumberFormat; }
        }
    }
}
