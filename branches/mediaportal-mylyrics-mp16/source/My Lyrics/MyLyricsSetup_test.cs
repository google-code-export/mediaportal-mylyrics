using System;
using System.Collections.Generic;
using System.Text;

namespace MyLyrics
{
    class MyLyricsSetup_test
    {

        #region main funcion
        /// <summary>
        /// This program is a simple demo of using Google Web APIs from .NET
        /// </summary>
        [STAThread]
        static void Main()
        {
            //Config.Dir.Config = @"C:\Program Files\Team MediaPortal\MediaPortal";
            System.IO.Directory.SetCurrentDirectory(@"C:\Documents and Settings\Administrator\My Documents\Visual Studio 2005\Projects\MP-plugins\MyLyrics\My Lyrics\bin\Debug");
            System.Windows.Forms.Application.Run(new MyLyricsSetup());
            //System.Windows.Forms.Application.Run(new MySeriaTest());
        }

        #endregion

    }


}
