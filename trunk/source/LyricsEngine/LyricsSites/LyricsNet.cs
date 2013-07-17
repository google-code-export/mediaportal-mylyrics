using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace LyricsEngine.LyricsSites
{
    public class LyricsNet : AbstractSite
    {
        # region const

        // Name
        private const string SiteName = "LyricsNet";

        // Base url
        private const string SiteBaseUrl = "http://www.lyrics.net";

        // Cannot find exact match
        private const string CannotFindExactMatch = "Cannot find exact match";
        // space
        private const string Space = "%20";

        private const string SearchPathQuery = "/artist/";
        private const string LyricsPath = "/lyric/";

        # endregion

        # region patterns

        //////////////////////////
        // First phase patterns //
        //////////////////////////

        // RegEx to find lyrics page
        private const string FindLyricsPagePatternPrefix = @"<a href=""/lyric/(?<lyricsIndex>\d+)"">";
        private const string FindLyricsPagePatternSuffix = "</a>";

        ///////////////////////////
        // Second phase patterns //
        ///////////////////////////

        // Validation RegEx - Either TitleAndArtist or Title & Artist should be valid
        // Title+Artist RegEx
//        private const string TitleAndArtistSearchPattern = @"<title>(?<titleartist>.*?)</title>";
        // Title RegEx (multiline)
//        private const string TitleSearchStartPattern = @"<span class=""artist_song_name_txt"">";
//        private const string TitleSearchEndPattern = @"</span>";
//        private const string TitleSearchPattern = @"<span class=""artist_song_name_txt"">(?<title>.*?)</span>";
        // Artist RegEx
//        private const string ArtistSearchPattern = @"<a class=""artist_singer_title"" href=""/artist\?prfid=?\d+&lang=?\d+"">(?<artist>.*?)</a>";


        // Lyrics RegEx
        // Lyrics start RegEx
        private const string LyricsStartSearchPattern = @"<pre id=""lyric-body-text"" class=""lyric-body"" dir=""ltr"" data-lang=""en"">";
        // Lyrics end RegEx
        private const string LyricsEndSearchPattern =  @"</pre>";

        # endregion

        // step 1 output
        private string lyricsIndex;
        private bool _firstStepComplete;


        public LyricsNet(string artist, string title, WaitHandle mEventStopSiteSearches, int timeLimit) : base(artist, title, mEventStopSiteSearches, timeLimit)
        {
        }

        #region interface implemetation

        protected override void FindLyricsWithTimer()
        {
            var fixHebrewArtist = Artist;
            var fixedHebrewTitle = Title;

            // 1st step - find lyrics page
            var firstUrlString = BaseUrl + SearchPathQuery + fixHebrewArtist + Space + fixedHebrewTitle;

            var findLyricsPageWebClient = new LyricsWebClient();
            findLyricsPageWebClient.OpenReadCompleted += FirstCallbackMethod;
            findLyricsPageWebClient.OpenReadAsync(new Uri(firstUrlString));

            while (_firstStepComplete == false)
            {
                if (MEventStopSiteSearches.WaitOne(1, true))
                {
                    _firstStepComplete = true;
                }
                else
                {
                    Thread.Sleep(100);
                }
            }

            if (lyricsIndex == null)
            {
                LyricText = NotFound;
                return;
            }
            // 2nd step - find lyrics
            var secondUrlString = BaseUrl + LyricsPath + lyricsIndex;

            var findLyricsWebClient = new LyricsWebClient(firstUrlString);
            findLyricsWebClient.OpenReadCompleted += SecondCallbackMethod;
            findLyricsWebClient.OpenReadAsync(new Uri(secondUrlString));

            while (Complete == false)
            {
                if (MEventStopSiteSearches.WaitOne(1, true))
                {
                    Complete = true;
                }
                else
                {
                    Thread.Sleep(100);
                }
            }
        }

        public override string Name
        {
            get { return SiteName; }
        }

        public override string BaseUrl
        {
            get { return SiteBaseUrl;}
        }


        public override LyricType GetLyricType()
        {
            return LyricType.UnsyncedLyrics;
        }

        public override SiteType GetSiteType()
        {
            return SiteType.Scrapper;
        }

        public override SiteComplexity GetSiteComplexity()
        {
            return SiteComplexity.TwoSteps;
        }

        public override SiteSpeed GetSiteSpeed()
        {
            return SiteSpeed.Medium;
        }

        public override bool SiteActive()
        {
            return true;
        }

        #endregion interface implemetation

        #region private methods

        // Finds lyrics page
        private void FirstCallbackMethod(object sender, OpenReadCompletedEventArgs e)
        {
            var thisMayBeTheCorrectPage = false;

            Stream reply = null;
            StreamReader reader = null;

            try
            {
                reply = e.Result;
                reader = new StreamReader(reply, Encoding.UTF8);

                while (!thisMayBeTheCorrectPage)
                {
                    // Read line
                    if (reader.EndOfStream)
                    {
                        break;
                    }
                    var line = reader.ReadLine() ?? "";

                    // Try to find match in line
                    var findLyricsPagePattern = FindLyricsPagePatternPrefix + Title + FindLyricsPagePatternSuffix;
                    var findLyricsPageMatch = Regex.Match(line, findLyricsPagePattern, RegexOptions.IgnoreCase);

                    if (findLyricsPageMatch.Groups.Count == 2)
                    {
                        lyricsIndex = findLyricsPageMatch.Groups[1].Value;

                        if (Convert.ToUInt32(lyricsIndex) > 0)
                        {
                            // Found page
                            thisMayBeTheCorrectPage = true;
                        }
                    }
                }

                // Not found
                if (!thisMayBeTheCorrectPage)
                {
                    LyricText = NotFound;
                }
            }
            catch
            {
                LyricText = NotFound;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }

                if (reply != null)
                {
                    reply.Close();
                }
                _firstStepComplete = true;
            }
        }

        // Find lyrics
        private void SecondCallbackMethod(object sender, OpenReadCompletedEventArgs e)
        {
            var thisMayBeTheCorrectLyric = false;
            var lyricTemp = new StringBuilder();

            Stream reply = null;
            StreamReader reader = null;

            try
            {
                reply = e.Result;
                reader = new StreamReader(reply, Encoding.UTF8);

                // Validation
                var titleAndArtistInPage = string.Empty;
                var inTitle = false;
                var titleInPage = string.Empty;
                var artistInPage = string.Empty;
                var validateArtistAndTitle = false;
                var validateArtist = false;
                var validateTitle = false;

                var foundStart = false;

                while (!Complete)
                {
                    // Read line
                    if (reader.EndOfStream)
                    {
                        break;
                    }
                    var line = reader.ReadLine() ?? string.Empty;

                    // Find artist + title in <title> line and validate correct artist/title
                    /*
                    if (titleAndArtistInPage == string.Empty)
                    {
                        var findTitleAndArtistMatch = Regex.Match(line, TitleAndArtistSearchPattern, RegexOptions.IgnoreCase);
                        if (findTitleAndArtistMatch.Groups.Count == 2)
                        {
                            titleAndArtistInPage = findTitleAndArtistMatch.Groups[1].Value;

                            // validation ArtistAndTitle
                            if (ValidateArtistAndTitle(titleAndArtistInPage))
                            {
                                validateArtistAndTitle = true;
                            }
                        }
                    }
                    */

                    /*
                    //Find title in <span class="artist_song_name_txt">(?.*)</span> line
                    if (titleInPage == String.Empty)
                    {
                        var findTitleStartMatch = Regex.Match(line, TitleSearchStartPattern, RegexOptions.IgnoreCase);
                        if (findTitleStartMatch.Success)
                        {
                            inTitle = true;
                        }
                    }
                    if (inTitle) // title found in page
                    {
                        titleInPage += line;

                        // Search for ending of title 
                        var findTitleEndMatch = Regex.Match(line, TitleSearchEndPattern, RegexOptions.IgnoreCase);
                        if (findTitleEndMatch.Success)
                        {
                            inTitle = false;
                        }
                    }
                     */
 
                    /*
                    // Finish and validate
                    if (titleInPage != string.Empty && !inTitle)
                    {
                        // Search for ending of artist 
                        var findTitleMatch = Regex.Match(titleInPage, TitleSearchPattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                        if (findTitleMatch.Groups.Count == 2)
                        {
                            titleInPage = findTitleMatch.Groups[1].Value;

                            // validation Title
                            if (IgnoreSpecialChars(Title).Equals(IgnoreSpecialChars(titleInPage).Trim()))
                            {
                                validateTitle = true;
                            }
                        }
                    }
                    */

                    /*
                    // Find artist in <a class="artist_singer_title" href="/artist?prfid=975&lang=1">()</a>
                    if (artistInPage == string.Empty)
                    {
                        var findArtistMatch = Regex.Match(line, ArtistSearchPattern, RegexOptions.IgnoreCase);
                        if (findArtistMatch.Groups.Count == 2)
                        {
                            artistInPage = findArtistMatch.Groups[1].Value;

                            // validation Artist
                            if (IgnoreSpecialChars(Artist).Equals(IgnoreSpecialChars(artistInPage).Trim()))
                            {
                                validateArtist = true;
                            }
                        }
                    }
                    */

                    if (!foundStart)
                    {
                        // Try to find lyrics start in line
                        var findLyricsPageMatch = Regex.Match(line, LyricsStartSearchPattern, RegexOptions.IgnoreCase);

                        if (findLyricsPageMatch.Groups.Count == 2)
                        {
                            foundStart = true;

                            // Here's where we use the data from the validation - just when we hit the first lyrics row
                            if (!((validateArtist && validateTitle) || validateArtistAndTitle))
                            {
                                throw new ArgumentException(CannotFindExactMatch);
                            }

                            // Initialize with first line
                            lyricTemp.Append(findLyricsPageMatch.Groups[1].Value).Append(Environment.NewLine);
                        }
                    }
                    else // already found start
                    {
                        // Try to find lyrics end in line
                        var findLyricsPageMatch = Regex.Match(line, LyricsEndSearchPattern, RegexOptions.IgnoreCase);
                        if (findLyricsPageMatch.Groups.Count == 2)
                        {
                            // Add last line
                            lyricTemp.Append(findLyricsPageMatch.Groups[1].Value).Append(Environment.NewLine);
                            thisMayBeTheCorrectLyric = true;
                            break;
                        }

                        // Add line to lyrics
                        lyricTemp.Append(line).Append(Environment.NewLine);
                    }
                }

                if (thisMayBeTheCorrectLyric)
                {
                    // Clean lyrics
                    LyricText = CleanLyrics(lyricTemp);

                    if (LyricText.Length == 0 || (LyricText.Contains("<") || LyricText.Contains(">") || LyricText.Contains("a href")))
                    {
                        LyricText = NotFound;
                    }
                }
            }
            catch
            {
                LyricText = NotFound;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }

                if (reply != null)
                {
                    reply.Close();
                }
                Complete = true;
            }
        }

        private static string IgnoreSpecialChars(string orig)
        {
            return orig.Replace("\'", "").Replace("\"", "").Trim();
        }

        private static string CleanLyrics(StringBuilder lyricTemp)
        {
            lyricTemp.Replace("<br>", "");
            lyricTemp.Replace("<br/>", "");
            lyricTemp.Replace("&quot;", "\"");

            return lyricTemp.ToString().Trim();
        }

        private static string FixEscapeCharacters(string text)
        {
            text = text.Replace("(", "");
            text = text.Replace(")", "");
            text = text.Replace("#", "");
            text = text.Replace("/", "");

            text = text.Replace("%", "%25");

            text = text.Replace(" ", "%20");
            text = text.Replace("$", "%24");
            text = text.Replace("&", "%26");
            text = text.Replace("'", "%27");
            text = text.Replace("+", "%2B");
            text = text.Replace(",", "%2C");
            text = text.Replace(":", "%3A");
            text = text.Replace(";", "%3B");
            text = text.Replace("=", "%3D");
            text = text.Replace("?", "%3F");
            text = text.Replace("@", "%40");
            text = text.Replace("&amp;", "&");

            return text;
        }

        #endregion private methods
    }
}
