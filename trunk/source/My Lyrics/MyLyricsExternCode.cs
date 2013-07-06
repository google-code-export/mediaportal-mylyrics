using System;
using System.IO;
using System.Timers;
using LyricsEngine;
using MediaPortal.Freedb;
using MediaPortal.GUI.Library;
using MediaPortal.GUI.Music;
using MediaPortal.Music.Database;
using MediaPortal.Player;
using MediaPortal.Playlists;
using MediaPortal.TagReader;
using MediaPortal.Util;
using Utils = MediaPortal.Util.Utils;

namespace MyLyrics
{
  public partial class GUIMyLyrics : GUIWindow, ILyricForm, ISetupForm
  {
    public void GetAlbumArt()
    {
      _CurrentTrackFileName = g_Player.CurrentFile;
      _NextTrackFileName = PlayListPlayer.SingletonPlayer.GetNext();
      GetTrackTags();

      _CurrentThumbFileName = GUIMusicFiles.GetCoverArt(false, _CurrentTrackFileName, _CurrentTrackTag);
      if (_CurrentThumbFileName.Length > 0)
      {
        // let us test if there is a larger cover art image
        string strLarge = Utils.ConvertToLargeCoverArt(_CurrentThumbFileName);
        if (File.Exists(strLarge))
          _CurrentThumbFileName = strLarge;
        AddImageToImagePathContainer(_CurrentThumbFileName);
      }

      UpdateImagePathContainer();
    }

    public void GetAlbumArt(string artist)
    {
      _CurrentThumbFileName = Utils.GetCoverArtName(Thumbs.MusicArtists, artist);

      if (_CurrentThumbFileName.Length > 0)
      {
        // let us test if there is a larger cover art image
        string strLarge = Utils.ConvertToLargeCoverArt(_CurrentThumbFileName);
        if (File.Exists(strLarge))
          _CurrentThumbFileName = strLarge;
        AddImageToImagePathContainer(_CurrentThumbFileName);
      }

      UpdateImagePathContainer();
    }


    private void OnImageTimerTickEvent(object trash_, ElapsedEventArgs args_)
    {
      FlipPictures();
    }

    private void OnLRCPickTimerTickEvent(object trash_, ElapsedEventArgs args_)
    {
      GUIControl.SetControlLabel(GetID, (int)GUI_LRC_Controls.CONTROL_LRCPICK_STATUS, "");
    }

    private void FlipPictures()
    {
      //// Check if we should let the visualization window handle image flipping
      //if (_usingBassEngine && _showVisualization)
      //    return;

      if (GUIelement_ImgCoverArt != null)
      {
        if (_ImagePathContainer.Count > 0)
        {
          GUIelement_ImgCoverArt.Dispose();
          if (_ImagePathContainer.Count > 1)
          {
            int currentImage = 0;
            // get the next image
            foreach (string image in _ImagePathContainer)
            {
              currentImage++;
              if (GUIelement_ImgCoverArt.FileName == image)
                break;
            }
            if (currentImage < _ImagePathContainer.Count)
              GUIelement_ImgCoverArt.SetFileName(_ImagePathContainer[currentImage]);
            else
              // start loop again
              GUIelement_ImgCoverArt.SetFileName(_ImagePathContainer[0]);
          }
          else
            GUIelement_ImgCoverArt.SetFileName(_ImagePathContainer[0]);
          GUIelement_ImgCoverArt.AllocResources();
        }
      }
    }


    private void GetTrackTags()
    {
      if (!g_Player.Playing)
      {
        _PreviousTrackTag = null;
        _CurrentTrackTag = null;
        _NextTrackTag = null;
        return;
      }

      bool isCurSongCdTrack = IsCdTrack(_CurrentTrackFileName);
      bool isNextSongCdTrack = IsCdTrack(_NextTrackFileName);
      bool isInternetStream = Utils.IsAVStream(_CurrentTrackFileName);
      MusicDatabase dbs = MusicDatabase.Instance;

      if (_CurrentTrackTag != null)
        _PreviousTrackTag = _CurrentTrackTag;

      if (!isCurSongCdTrack && !isInternetStream)
      {
        _CurrentTrackTag = GetTrackTag(dbs, _CurrentTrackFileName, true);
        //_CurrentTrackTag = dbs.GetTag(_CurrentTrackFileName);
        //_CurrentTrackTag.Artist = LyricUtil.CapatalizeString(GUIPropertyManager.GetProperty("#Play.Current.Artist"));
        //_CurrentTrackTag.Title = LyricUtil.CapatalizeString(GUIPropertyManager.GetProperty("#Play.Current.Title"));
      }

      if (!isNextSongCdTrack && !isInternetStream)
      {
        _NextTrackTag = GetTrackTag(dbs, _NextTrackFileName, true);
        //_NextTrackTag = dbs.GetTag(_NextTrackFileName);
      }

      if (isCurSongCdTrack || isNextSongCdTrack)
      {
        PlayList curPlaylist = _PlaylistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC);

        int iCurItemIndex = _PlaylistPlayer.CurrentSong;
        PlayListItem curPlaylistItem = curPlaylist[iCurItemIndex];

        if (curPlaylistItem == null)
        {
          _CurrentTrackTag = null;
          _NextTrackTag = null;
          return;
        }

        int playListItemCount = curPlaylist.Count;
        int nextItemIndex = 0;

        if (iCurItemIndex < playListItemCount - 1)
          nextItemIndex = iCurItemIndex + 1;

        PlayListItem nextPlaylistItem = curPlaylist[nextItemIndex];

        if (isCurSongCdTrack)
          _CurrentTrackTag = (MusicTag)curPlaylistItem.MusicTag;

        if (isNextSongCdTrack && nextPlaylistItem != null)
          _NextTrackTag = (MusicTag)nextPlaylistItem.MusicTag;

        // There's no MusicTag info in the Playlist so check is we have a valid 
        // GUIMusicFiles.MusicCD object
        if ((_CurrentTrackTag == null || _NextTrackTag == null) && GUIMusicFiles.MusicCD != null)
        {
          int curCDTrackNum = GetCDATrackNumber(_CurrentTrackFileName);
          int nextCDTrackNum = GetCDATrackNumber(_NextTrackFileName);

          if (curCDTrackNum < GUIMusicFiles.MusicCD.Tracks.Length)
          {
            CDTrackDetail curTrack = GUIMusicFiles.MusicCD.getTrack(curCDTrackNum);
            _CurrentTrackTag = GetTrackTag(curTrack);
            //_CurrentTrackTag.Artist = LyricUtil.CapatalizeString(GUIPropertyManager.GetProperty("#Play.Current.Artist"));
            //_CurrentTrackTag.Title = LyricUtil.CapatalizeString(GUIPropertyManager.GetProperty("#Play.Current.Title"));
          }
          if (nextCDTrackNum < GUIMusicFiles.MusicCD.Tracks.Length)
          {
            CDTrackDetail nextTrack = GUIMusicFiles.MusicCD.getTrack(nextCDTrackNum);
            _NextTrackTag = GetTrackTag(nextTrack);
          }
        }
      }

      // If we got an Internetstream and are playing via BASS Player
      // then receive the MusicTags from the stream
      if (isInternetStream && BassMusicPlayer.IsDefaultMusicPlayer)
      {
        _NextTrackTag = null;
        _CurrentTrackTag = BassMusicPlayer.Player.GetStreamTags();
      }
    }

    private bool IsCdTrack(string fileName)
    {
      return Path.GetExtension(_CurrentTrackFileName).ToLower() == ".cda";
    }

    private int GetCDATrackNumber(string strFile)
    {
      string strTrack = string.Empty;
      int pos = strFile.IndexOf(".cda");
      if (pos >= 0)
      {
        pos--;
        while (Char.IsDigit(strFile[pos]) && pos > 0)
        {
          strTrack = strFile[pos] + strTrack;
          pos--;
        }
      }

      try
      {
        int iTrack = Convert.ToInt32(strTrack);
        return iTrack;
      }
      catch (Exception)
      {
      }

      return 1;
    }

    private MusicTag GetTrackTag(CDTrackDetail cdTrack)
    {
      if (GUIMusicFiles.MusicCD == null)
        return null;
      MusicTag tag = new MusicTag();
      tag.Artist = GUIMusicFiles.MusicCD.Artist;
      tag.Album = GUIMusicFiles.MusicCD.Title;
      tag.Genre = GUIMusicFiles.MusicCD.Genre;
      tag.Year = GUIMusicFiles.MusicCD.Year;
      tag.Duration = cdTrack.Duration;
      tag.Title = cdTrack.Title;
      return tag;
    }

    private MusicTag GetTrackTag(MusicDatabase dbs, string strFile, bool useID3)
    {
      MusicTag tag = null;
      Song song = new Song();
      bool bFound = dbs.GetSongByFileName(strFile, ref song);
      if (!bFound)
      {
        if (useID3)
        {
          tag = TagReader.ReadTag(strFile);
            var currentTitle = GUIPropertyManager.GetProperty("#Play.Current.Title");
            if (tag != null && tag.Title != currentTitle) // Track information not available
          {
            return tag;
          }
        }
        // tagreader failed or not using it
        song.Title = Path.GetFileNameWithoutExtension(strFile);
        song.Artist = string.Empty;
        song.Album = string.Empty;
      }
      tag = new MusicTag();
      tag = song.ToMusicTag();
      return tag;
    }

    private MusicTag BuildMusicTagFromSong(Song Song_)
    {
      MusicTag tmpTag = new MusicTag();

      tmpTag.Title = Song_.Title;
      tmpTag.Album = Song_.Album;
      tmpTag.Artist = Song_.Artist;
      tmpTag.Duration = Song_.Duration;
      tmpTag.Genre = Song_.Genre;
      tmpTag.Track = Song_.Track;
      tmpTag.Year = Song_.Year;
      tmpTag.Rating = Song_.Rating;

      return tmpTag;
    }


    private void UpdateImagePathContainer()
    {
      if (_ImagePathContainer.Count <= 0)
      {
        try
        {
          GUIelement_ImgCoverArt.Dispose();
          GUIelement_ImgCoverArt.SetFileName(GUIGraphicsContext.Skin + @"\media\missing_coverart.png");
          GUIelement_ImgCoverArt.AllocResources();
        }
        catch (Exception ex)
        {
          GUIelement_ImgCoverArt.Dispose();
          Log.Debug("GUIMusicPlayingNow: could not set default image - {0}", ex.Message);
        }
      }

      if (g_Player.Duration > 0 && _ImagePathContainer.Count > 1)
        //  ImageChangeTimer.Interval = (g_Player.Duration * 1000) / (ImagePathContainer.Count * 8); // change each cover 8x
        _ImageChangeTimer.Interval = 15 * 1000; // change covers every 15 seconds
      else
        _ImageChangeTimer.Interval = 3600 * 1000;

      _ImageChangeTimer.Stop();
      _ImageChangeTimer.Start();
    }

    private bool AddImageToImagePathContainer(string newImage)
    {
      lock (_imageMutex)
      {
        string ImagePath = Convert.ToString(newImage);
        if (ImagePath.IndexOf(@"missing_coverart") > 0) // && (ImagePathContainer.Count > 0))
        {
          Log.Debug("GUIMusicPlayingNow: Found placeholder - not inserting image {0}", ImagePath);
          return false;
        }

        bool success = false;
        if (_ImagePathContainer != null)
        {
          if (_ImagePathContainer.Contains(ImagePath))
            return false;

          //// check for placeholder
          //int indexDel = 0;
          //bool found = false;
          //foreach (string pic in ImagePathContainer)
          //{
          //  indexDel++;
          //  if (pic.IndexOf("missing_coverart.png") > 0)
          //  {
          //    found = true;
          //    break;
          //  }
          //}
          //if (found)
          //  ImagePathContainer.RemoveAt(indexDel - 1);

          if (File.Exists(ImagePath))
          {
            try
            {
              Log.Debug("GUIMusicPlayingNow: adding image to container - {0}", ImagePath);
              _ImagePathContainer.Add(ImagePath);
              success = true;
            }
            catch (Exception ex)
            {
              Log.Error("GUIMusicPlayingNow: error adding image ({0}) - {1}", ImagePath, ex.Message);
            }

            // display the first pic automatically
            if (_ImagePathContainer.Count == 1)
              FlipPictures();
          }
        }

        return success;
      }
    }
  }
}