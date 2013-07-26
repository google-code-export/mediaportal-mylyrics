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
            _currentTrackFileName = g_Player.CurrentFile;
            _nextTrackFileName = PlayListPlayer.SingletonPlayer.GetNext();
            GetTrackTags();

            _currentThumbFileName = GUIMusicBaseWindow.GetCoverArt(false, _currentTrackFileName, _currentTrackTag);
            if (_currentThumbFileName.Length > 0)
            {
                // let us test if there is a larger cover art image
                var strLarge = Utils.ConvertToLargeCoverArt(_currentThumbFileName);
                if (File.Exists(strLarge))
                    _currentThumbFileName = strLarge;
                AddImageToImagePathContainer(_currentThumbFileName);
            }

            UpdateImagePathContainer();
        }

        public void GetAlbumArt(string artist)
        {
            _currentThumbFileName = Utils.GetCoverArtName(Thumbs.MusicArtists, artist);

            if (_currentThumbFileName.Length > 0)
            {
                // let us test if there is a larger cover art image
                var strLarge = Utils.ConvertToLargeCoverArt(_currentThumbFileName);
                if (File.Exists(strLarge))
                    _currentThumbFileName = strLarge;
                AddImageToImagePathContainer(_currentThumbFileName);
            }

            UpdateImagePathContainer();
        }


        private void OnImageTimerTickEvent(object trash, ElapsedEventArgs args)
        {
            FlipPictures();
        }

        private void OnLRCPickTimerTickEvent(object trash, ElapsedEventArgs args)
        {
            GUIControl.SetControlLabel(GetID, (int) GUILRCControls.ControlLrcPickStatus, "");
        }

        private void FlipPictures()
        {
            //// Check if we should let the visualization window handle image flipping
            //if (_usingBassEngine && _showVisualization)
            //    return;

            if (GuIelementImgCoverArt != null)
            {
                if (_imagePathContainer.Count > 0)
                {
                    GuIelementImgCoverArt.Dispose();
                    if (_imagePathContainer.Count > 1)
                    {
                        var currentImage = 0;
                        // get the next image
                        foreach (var image in _imagePathContainer)
                        {
                            currentImage++;
                            if (GuIelementImgCoverArt.FileName == image)
                                break;
                        }
                        if (currentImage < _imagePathContainer.Count)
                            GuIelementImgCoverArt.SetFileName(_imagePathContainer[currentImage]);
                        else
                            // start loop again
                            GuIelementImgCoverArt.SetFileName(_imagePathContainer[0]);
                    }
                    else
                        GuIelementImgCoverArt.SetFileName(_imagePathContainer[0]);
                    GuIelementImgCoverArt.AllocResources();
                }
            }
        }


        private void GetTrackTags()
        {
            if (!g_Player.Playing)
            {
                _currentTrackTag = null;
                _nextTrackTag = null;
                return;
            }

            var isCurSongCdTrack = IsCdTrack();
            var isNextSongCdTrack = IsCdTrack();
            var isInternetStream = Utils.IsAVStream(_currentTrackFileName);
            var dbs = MusicDatabase.Instance;


            if (!isCurSongCdTrack && !isInternetStream)
            {
                _currentTrackTag = GetTrackTag(dbs, _currentTrackFileName, true);
                //_CurrentTrackTag = dbs.GetTag(_CurrentTrackFileName);
                //_CurrentTrackTag.Artist = LyricUtil.CapatalizeString(GUIPropertyManager.GetProperty("#Play.Current.Artist"));
                //_CurrentTrackTag.Title = LyricUtil.CapatalizeString(GUIPropertyManager.GetProperty("#Play.Current.Title"));
            }

            if (!isNextSongCdTrack && !isInternetStream)
            {
                _nextTrackTag = GetTrackTag(dbs, _nextTrackFileName, true);
                //_NextTrackTag = dbs.GetTag(_NextTrackFileName);
            }

            if (isCurSongCdTrack || isNextSongCdTrack)
            {
                var curPlaylist = _playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC);

                var iCurItemIndex = _playlistPlayer.CurrentSong;
                var curPlaylistItem = curPlaylist[iCurItemIndex];

                if (curPlaylistItem == null)
                {
                    _currentTrackTag = null;
                    _nextTrackTag = null;
                    return;
                }

                var playListItemCount = curPlaylist.Count;
                var nextItemIndex = 0;

                if (iCurItemIndex < playListItemCount - 1)
                    nextItemIndex = iCurItemIndex + 1;

                var nextPlaylistItem = curPlaylist[nextItemIndex];

                if (isCurSongCdTrack)
                    _currentTrackTag = (MusicTag) curPlaylistItem.MusicTag;

                if (isNextSongCdTrack && nextPlaylistItem != null)
                    _nextTrackTag = (MusicTag) nextPlaylistItem.MusicTag;

                // There's no MusicTag info in the Playlist so check is we have a valid 
                // GUIMusicFiles.MusicCD object
                if ((_currentTrackTag == null || _nextTrackTag == null) && GUIMusicFiles.MusicCD != null)
                {
                    var curCDTrackNum = GetCDATrackNumber(_currentTrackFileName);
                    var nextCDTrackNum = GetCDATrackNumber(_nextTrackFileName);

                    if (curCDTrackNum < GUIMusicFiles.MusicCD.Tracks.Length)
                    {
                        var curTrack = GUIMusicFiles.MusicCD.getTrack(curCDTrackNum);
                        _currentTrackTag = GetTrackTag(curTrack);
                        //_CurrentTrackTag.Artist = LyricUtil.CapatalizeString(GUIPropertyManager.GetProperty("#Play.Current.Artist"));
                        //_CurrentTrackTag.Title = LyricUtil.CapatalizeString(GUIPropertyManager.GetProperty("#Play.Current.Title"));
                    }
                    if (nextCDTrackNum < GUIMusicFiles.MusicCD.Tracks.Length)
                    {
                        var nextTrack = GUIMusicFiles.MusicCD.getTrack(nextCDTrackNum);
                        _nextTrackTag = GetTrackTag(nextTrack);
                    }
                }
            }

            // If we got an Internetstream and are playing via BASS Player
            // then receive the MusicTags from the stream
            if (isInternetStream && BassMusicPlayer.IsDefaultMusicPlayer)
            {
                _nextTrackTag = null;
                _currentTrackTag = BassMusicPlayer.Player.GetStreamTags();
            }
        }

        private bool IsCdTrack()
        {
            var extension = Path.GetExtension(_currentTrackFileName);
            return extension != null && extension.ToLower() == ".cda";
        }

        private static int GetCDATrackNumber(string strFile)
        {
            var strTrack = string.Empty;
            var pos = strFile.IndexOf(".cda", StringComparison.Ordinal);
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
                var iTrack = System.Convert.ToInt32(strTrack);
                return iTrack;
            }
            catch (Exception)
            {
                ;
            }

            return 1;
        }

        private MusicTag GetTrackTag(CDTrackDetail cdTrack)
        {
            if (GUIMusicFiles.MusicCD == null)
                return null;
            var tag = new MusicTag();
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
            MusicTag tag;
            var song = new Song();
            var bFound = dbs.GetSongByFileName(strFile, ref song);
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
            tag = song.ToMusicTag();
            return tag;
        }

        private MusicTag BuildMusicTagFromSong(Song song)
        {
            var tmpTag = new MusicTag();

            tmpTag.Title = song.Title;
            tmpTag.Album = song.Album;
            tmpTag.Artist = song.Artist;
            tmpTag.Duration = song.Duration;
            tmpTag.Genre = song.Genre;
            tmpTag.Track = song.Track;
            tmpTag.Year = song.Year;
            tmpTag.Rating = song.Rating;

            return tmpTag;
        }


        private void UpdateImagePathContainer()
        {
            if (_imagePathContainer.Count <= 0)
            {
                try
                {
                    GuIelementImgCoverArt.Dispose();
                    GuIelementImgCoverArt.SetFileName(GUIGraphicsContext.Skin + @"\media\missing_coverart.png");
                    GuIelementImgCoverArt.AllocResources();
                }
                catch (Exception ex)
                {
                    GuIelementImgCoverArt.Dispose();
                    Log.Debug("GUIMusicPlayingNow: could not set default image - {0}", ex.Message);
                }
            }

            if (g_Player.Duration > 0 && _imagePathContainer.Count > 1)
                //  ImageChangeTimer.Interval = (g_Player.Duration * 1000) / (ImagePathContainer.Count * 8); // change each cover 8x
                _imageChangeTimer.Interval = 15*1000; // change covers every 15 seconds
            else
                _imageChangeTimer.Interval = 3600*1000;

            _imageChangeTimer.Stop();
            _imageChangeTimer.Start();
        }

        private bool AddImageToImagePathContainer(string newImage)
        {
            lock (_imageMutex)
            {
                var imagePath = System.Convert.ToString(newImage);
                if (imagePath.IndexOf(@"missing_coverart", StringComparison.Ordinal) > 0)
                    // && (ImagePathContainer.Count > 0))
                {
                    Log.Debug("GUIMusicPlayingNow: Found placeholder - not inserting image {0}", imagePath);
                    return false;
                }

                var success = false;
                if (_imagePathContainer != null)
                {
                    if (_imagePathContainer.Contains(imagePath))
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

                    if (File.Exists(imagePath))
                    {
                        try
                        {
                            Log.Debug("GUIMusicPlayingNow: adding image to container - {0}", imagePath);
                            _imagePathContainer.Add(imagePath);
                            success = true;
                        }
                        catch (Exception ex)
                        {
                            Log.Error("GUIMusicPlayingNow: error adding image ({0}) - {1}", imagePath, ex.Message);
                        }

                        // display the first pic automatically
                        if (_imagePathContainer.Count == 1)
                            FlipPictures();
                    }
                }

                return success;
            }
        }
    }
}