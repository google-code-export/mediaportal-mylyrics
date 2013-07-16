using System.Collections.Generic;
using System.Xml;
using MediaPortal.Profile;

namespace MyLyrics
{
  internal class ConvertFromXMLtoLyricsDatabase
  {
    public LyricsDatabase Convert(string path)
    {
        LyricsDatabase db = new LyricsDatabase();
        XmlTextReader tr = new XmlTextReader(path);

        string currentArtist = "";
        string currentTitle = "";
        while (tr.Read())
        {
            switch (tr.Name)
            {
                case "section":
                    if (tr.AttributeCount == 1)
                    {
                        currentArtist = tr.GetAttribute(0);
                    }
                    break;
                case "entry":
                    if (tr.AttributeCount == 1)
                    {
                        currentTitle = tr.GetAttribute(0);
                        LyricsItem item = new LyricsItem(currentArtist, currentTitle, "", "");
                        db.Add(DatabaseUtil.CorrectKeyFormat(currentArtist, currentTitle), item);
                    }
                    break;
            }
        }
        tr.Close();

        LyricsDatabase lyricsDatabase = new LyricsDatabase();

        using (Settings xmlreader = new Settings(path, false))
        {
            string lyrics;
            foreach (KeyValuePair<string, LyricsItem> kvp in db)
            {
                lyrics = xmlreader.GetValueAsString(kvp.Value.Artist, kvp.Value.Title, "");
                lyricsDatabase.Add(DatabaseUtil.CorrectKeyFormat(kvp.Value.Artist, kvp.Value.Title),
                                   new LyricsItem(kvp.Value.Artist, kvp.Value.Title, lyrics, "old database entry"));
            }
        }

        return lyricsDatabase;
    }
  }
}