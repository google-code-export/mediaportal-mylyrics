using Google.API.Translate;

namespace TranslateProvider
{
  public class TranslateProvider
  {
    private readonly TranslateClient _client;

      public TranslateProvider(string referrer)
    {
        _client = new TranslateClient(referrer);
    }

      public string Translate(string input, string translateTo, out string detectedLanguage, out bool reliable,
                            out double confidence, string lineshiftMark)
    {
      const string symbolToRepresentLineshift = "QQQQQ";
      input = input.Replace(lineshiftMark, symbolToRepresentLineshift);

      detectedLanguage = _client.Detect(input, out reliable, out confidence);
      var translated = _client.Translate(input, detectedLanguage, translateTo);

      translated = translated.Replace(symbolToRepresentLineshift, lineshiftMark);

      //translated = translated.Replace("[*] ", lineshiftMark);
      //translated = translated.Replace("[ *] ", lineshiftMark);
      //translated = translated.Replace("[* ] ", lineshiftMark);
      //translated = translated.Replace("[ * ] ", lineshiftMark);
      translated = translated.Replace(" " + lineshiftMark, lineshiftMark);
      return translated;
    }
  }
}