/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Diagnostics;

namespace InfinityCode.uContext
{
    public static class Links
    {
        public const string basic = "https://assetstore.unity.com/packages/tools/utilities/ucontext-basic-182221";
        public const string basicReviews = basic + "/reviews";
        public const string documentation = "https://infinity-code.com/documentation/ucontext.pdf";
        public const string forum = "https://forum.infinity-code.com";
        public const string homepage = "https://infinity-code.com/assets/ucontext";
        public const string pro = "https://assetstore.unity.com/packages/tools/utilities/ucontext-pro-141831";
        public const string proReviews = pro + "/reviews";
        public const string support = "mailto:support@infinity-code.com?subject=uContext";
        public const string youtube = "https://www.youtube.com/playlist?list=PL2QU1uhBMew_mR83EYhex5q3uZaMTwg1S";

        public static void Open(string url)
        {
            Process.Start(url);
        }

        public static void OpenAssetStore()
        {
#if !UCONTEXT_PRO
            OpenBasic();
#else
            OpenPro();
#endif
        }

        public static void OpenBasic()
        {
            Open(basic);
        }

        public static void OpenBasicReviews()
        {
            Open(basicReviews);
        }

        public static void OpenDocumentation()
        {
            Open(documentation);
        }

        public static void OpenForum()
        {
            Open(forum);
        }

        public static void OpenHomepage()
        {
            Open(homepage);
        }

        public static void OpenPro()
        {
            Open(pro);
        }

        public static void OpenProReviews()
        {
            Open(proReviews);
        }

        public static void OpenReviews()
        {
#if !UCONTEXT_PRO
            OpenBasicReviews();
#else
            OpenProReviews();
#endif
        }

        public static void OpenSupport()
        {
            Open(support);
        }

        public static void OpenYouTube()
        {
            Open(youtube);
        }
    }
}