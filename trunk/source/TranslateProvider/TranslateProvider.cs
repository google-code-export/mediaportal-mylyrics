using Google.API.Translate;

namespace TranslateProvider
{
  public class TranslateProvider
  {
    private readonly TranslateClient _client;
    private readonly string _referrer = string.Empty;

    public TranslateProvider(string referrer)
    {
      _referrer = referrer;
      _client = new TranslateClient(_referrer);
    }

    public string Translate(string input, string translateTo, out string detectedLanguage, out bool reliable,
                            out double confidence, string lineshiftMark)
    {
      string symbolToRepresentLineshift = "QQQQQ";
      input = input.Replace(lineshiftMark, symbolToRepresentLineshift);

      detectedLanguage = _client.Detect(input, out reliable, out confidence);
      string translated = _client.Translate(input, detectedLanguage, translateTo);

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