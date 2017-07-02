using System;
using System.Collections;
using Fractalscape;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace Fractalscape
{
    public sealed class MusicWindow : Window
    {
        [SerializeField]public Image PlayButtonIcon;
        [SerializeField]public Image ForwardSkipIcon;
        [SerializeField]public Image BackwardSkipIcon;
        [SerializeField]public Image AlbumCover;
        private ButtonType _activatedButton;

        [SerializeField]private Sprite _playButton;
        [SerializeField]private Sprite _pauseButton;
        [SerializeField]private Sprite _forwardSkipButton;
        [SerializeField]private Sprite _backwardSkipButton;
        [SerializeField]private Sprite _activePlayButton;
        [SerializeField]private Sprite _activePauseButton;
        [SerializeField]private Sprite _activeForwardSkipButton;
        [SerializeField]private Sprite _activeBackwardSkipButton;

        public RectTransform ProgressInidicator;
        public GameObject MusicWindowDynamic;
        public GameObject MusicWindowExtraneous;
        public GameObject MusicWindowUpdate;
        public GameObject MusicWindowStatic;

        public Text SongInfo;
        private string _artistName;
        private string _albumName;
        private string _songName;
        private float _songLength; // Seconds

        private float _dTime = 5;
        private float _lastUpdateTime;
        private float _localTime;

        public Text TimeIndicator;
        public Text MaxTimeIndicator;

        private float _progressHeight;
        private float _progressMaxWidth;

        private bool _songPlaying = false;

        public enum ButtonType
        {
            SkipBackward,
            SkipForward,
            Play,
            Pause,
            Null
        }

        private float _buttonActivationDuration;
        private float _buttonActivationStartTime;

        private void Awake()
        {
            _progressHeight = 29.0f;
            _progressMaxWidth = 450.0f;
            PlayButtonIcon.sprite = _pauseButton;
            _activatedButton = ButtonType.Null;
            _buttonActivationDuration = 0.15f;
        }

        public override void EnableSiblings()
        {
            MusicWindowDynamic.SetActive(true);
            MusicWindowExtraneous.SetActive(true);
            MusicWindowStatic.SetActive(true);
            MusicWindowUpdate.SetActive(true);
        }

        public override void DisableSiblings()
        {
            MusicWindowDynamic.SetActive(false);
            MusicWindowExtraneous.SetActive(false);
            MusicWindowStatic.SetActive(false);
            MusicWindowUpdate.SetActive(false);
        }

        public override void Setup()
        {
            EnableSiblings();
        }

        public override void Cleanup()
        {
            DisableSiblings();
        }

        void Update()
        {
            if (Time.time - _buttonActivationStartTime > _buttonActivationDuration)
            {
                if (_activatedButton == ButtonType.Play)
                {
                    PlayButtonIcon.sprite = _pauseButton;
                }
                else if (_activatedButton == ButtonType.Pause)
                {
                    PlayButtonIcon.sprite = _playButton;
                }
                else
                {
                    DeactivateIcon(_activatedButton);
                }
                _activatedButton = ButtonType.Null;
            }

            if (AudioManager.Instance.SongPlaying)
            {
                if (_dTime <= Time.time - _lastUpdateTime)
                {
                    SetSongCurrentTime(AudioManager.Instance.CurrentTime());
                    _lastUpdateTime = Time.time;
                }
                else
                {
                    SetSongCurrentTimeRaw(_localTime + (Time.time - _lastUpdateTime));
                }
            }
        }

        public void LoadNewSong(Song song, float duration)
        {
            SetAlbumCover(AudioManager.Instance.GetAlbumArt(song.Art));
            SetAlbumName(song.AlbumName);
            SetArtistName(song.Artist);
            SetSongName(song.Name);
            SetSongLength(duration);
            _lastUpdateTime = 0;
        }

        public void SetSongCurrentTime(float lengthSeconds)
        {
            _localTime = lengthSeconds;
            TimeIndicator.text = SecondsToDigital(lengthSeconds);
            SetProgressIndicatorPosition(lengthSeconds/_songLength);
        }

        public void SetSongCurrentTimeRaw(float lengthSeconds)
        {
            TimeIndicator.text = SecondsToDigital(lengthSeconds);
        }

        public IEnumerator UpdateCurrentTimeAsync(int delay, float time)
        {
            yield return new WaitForSeconds(delay);
            SetSongCurrentTime(time);
        }

        public void SetAlbumCover(Sprite albumCover)
        {
            AlbumCover.sprite = albumCover;
        }

        public void SetArtistName(string artistName)
        {
            if (artistName != _artistName)
            {
                _artistName = artistName;
                UpdateSongInfo();
            }
        }

        public void SetAlbumName(string albumName)
        {
            if (albumName != _albumName)
            {
                _albumName = albumName;
                UpdateSongInfo();
            }
        }

        public void SetSongName(string songName)
        {
            if (songName != _songName)
            {
                _songName = songName;
                UpdateSongInfo();
            }
        }

        public void SetSongLength(float lengthSeconds)
        {
            _songLength = lengthSeconds;
            MaxTimeIndicator.text = SecondsToDigital(_songLength);
        }

        public void TogglePlayButton()
        {
            if (AudioManager.Instance.SongPlaying)
            {
                PlayButtonIcon.sprite = _playButton;
                ActivateIcon(ButtonType.Pause);
                AudioManager.Instance.PauseSong();
            }
            else
            {
                PlayButtonIcon.sprite = _pauseButton;
                ActivateIcon(ButtonType.Play);
                AudioManager.Instance.ResumeSong();
            }
        }

        public void SkipBack()
        {
            ActivateIcon(ButtonType.SkipBackward);
            AudioManager.Instance.SkipBack();
            if (AudioManager.Instance.SongPaused)
            {
                PlayButtonIcon.sprite = _pauseButton;
                ActivateIcon(ButtonType.Play);
            }
        }

        public void SkipForward()
        {
            ActivateIcon(ButtonType.SkipForward);
            AudioManager.Instance.PlayNextTrack();
            if (AudioManager.Instance.SongPaused)
            {
                PlayButtonIcon.sprite = _pauseButton;
                ActivateIcon(ButtonType.Play);
            }
        }

        private void SetProgressIndicatorPosition(float fractionComplete)
        {
            var newWidth = fractionComplete * _progressMaxWidth;
            ProgressInidicator.sizeDelta = new Vector2(newWidth, _progressHeight);
        }

        private void UpdateSongInfo()
        {
            var newSongInfo = "Artist: " + _artistName + "\n\n";
            newSongInfo += "Album: " + _albumName + "\n\n";
            newSongInfo += "Song: " + _songName + "\n\n";
            SongInfo.text = newSongInfo;
        }

        private void ActivateIcon(ButtonType buttonType)
        {
            if (buttonType != _activatedButton && buttonType != ButtonType.Null)
            {
                DeactivateIcon(_activatedButton);
            }
            _activatedButton = buttonType;
            switch (buttonType)
            {
                case ButtonType.SkipBackward:
                    BackwardSkipIcon.sprite = _activeBackwardSkipButton;
                    break;
                case ButtonType.SkipForward:
                    ForwardSkipIcon.sprite = _activeForwardSkipButton;
                    break;
                case ButtonType.Play:
                    PlayButtonIcon.sprite = _activePlayButton;
                    break;
                case ButtonType.Pause:
                    PlayButtonIcon.sprite = _activePauseButton;
                    break;
            }
            _buttonActivationStartTime = Time.time;
        }

        private void DeactivateIcon(ButtonType buttonType)
        {
            switch (buttonType)
            {
                case ButtonType.SkipBackward:
                    BackwardSkipIcon.sprite = _backwardSkipButton;
                    break;
                case ButtonType.SkipForward:
                    ForwardSkipIcon.sprite = _forwardSkipButton;
                    break;
                case ButtonType.Play:
                    PlayButtonIcon.sprite = _playButton;
                    break;
                case ButtonType.Pause:
                    PlayButtonIcon.sprite = _pauseButton;
                    break;
            }
        }

        private string SecondsToDigital(float totalSeconds)
        {
            var minutes = (int) Math.Floor(totalSeconds / 60.0f);
            var seconds = (int) Math.Floor(totalSeconds - 60.0f * minutes);
            var formattedTime = string.Format("{0}:", minutes);
            if (seconds < 10)
            {
                formattedTime += "0";
            }
            formattedTime += string.Format("{0}", seconds);
            return formattedTime;
        }

        public override string PartnerNavigationWindow()
        {
            return AppSession.InViewer
                ? WindowNames.ViewerNavigationWindow
                : WindowNames.SecondaryNavigationWindow;
        }
    }
}
