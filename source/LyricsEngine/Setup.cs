using System.Collections.Generic;

namespace LyricsEngine
{
    public static class Setup
    {
        public static List<string> AllLyricsSites = new List<string>();

        static Setup()
        {
            ActiveSites = new List<string>();
        }

        public static List<string> ActiveSites { get; set; }


        public static List<string> AllSites()
        {
            return AllLyricsSites;
        }

        public static bool IsMember(string site)
        {
            return ActiveSites.Contains(site);
        }

        public static int NoOfSites()
        {
            return ActiveSites.Count;
        }
    }
}