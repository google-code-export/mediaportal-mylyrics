
namespace LyricsEngine.LyricsSites
{
    public enum LyricType
    {
        Lrc = 0,
        UnsyncedLyrics = 1
    }

    public enum SiteType
    {
        Api = 0,
        Scrapper = 1
    }

    public enum SiteComplexity
    {
        OneStep = 1,
        TwoSteps = 2
    }

    public interface ILyricSite
    {
        string Name { get; }

        void FindLyrics();

        string Lyric { get; }

        LyricType GetLyricType();

        SiteType GetSiteType();

        SiteComplexity GetSiteComplexity();

        bool SiteActive();
    }
}
