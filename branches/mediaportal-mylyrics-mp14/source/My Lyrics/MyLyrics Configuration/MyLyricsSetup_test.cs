using System;
using System.IO;
using System.Windows.Forms;

namespace MyLyrics
{
  internal class MyLyricsSetup_test
  {
    #region main funcion

    /// <summary>
    /// This program is a simple demo of using Google Web APIs from .NET
    /// </summary>
    [STAThread]
    private static void Main()
    {
      try
      {
        //Config.Dir.Config = @"C:\Program Files\Team MediaPortal\MediaPortal";
        Directory.SetCurrentDirectory(
            @"C:\Users\saamand\Documents\Visual Studio 2008\Projects\MyLyrics\My Lyrics\bin\Debug");
        Application.Run(new MyLyricsSetup());
        //System.Windows.Forms.Application.Run(new MySeriaTest());
      }
      catch (Exception e)
      {
        Console.WriteLine(e.Message);
      }
    }

    #endregion
  }
}