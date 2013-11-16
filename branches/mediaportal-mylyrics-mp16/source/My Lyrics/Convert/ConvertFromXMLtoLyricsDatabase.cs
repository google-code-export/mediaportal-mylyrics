using System.Xml;
using MediaPortal.Profile;

namespace MyLyrics.Convert
{
  internal class ConvertFromXMLtoLyricsDatabase
  {
    public LyricsDatabase Convert(string path)
    {
        var db = new LyricsDatabase();
        var tr = new XmlTextReader(path);

        var currentArtist = "";
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
                        var currentTitle = tr.GetAttribute(0);
                        var item = new LyricsItem(currentArtist, currentTitle, "", "");
                        db.Add(DatabaseUtil.CorrectKeyFormat(currentArtist, currentTitle), item);
                    }
                    break;
            }
        }
        tr.Close();

        var lyricsDatabase = new LyricsDatabase();

        using (var xmlreader = new Settings(path, false))
        {
            foreach (var kvp in db)
            {
                var lyrics = xmlreader.GetValueAsString(kvp.Value.Artist, kvp.Value.Title, "");
                lyricsDatabase.Add(DatabaseUtil.CorrectKeyFormat(kvp.Value.Artist, kvp.Value.Title),
                                   new LyricsItem(kvp.Value.Artist, kvp.Value.Title, lyrics, "old database entry"));
            }
        }

        return lyricsDatabase;
    }
  }
}