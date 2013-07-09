using System.Collections.Generic;

namespace LyricsEngine
{
    public static class Setup
    {
        static Setup()
        {
            ActiveSites = new List<string>();
        }

        public static List<string> ActiveSites { get; set; }
        
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