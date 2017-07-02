using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Fractalscape
{
    public class CompletionCallback : AndroidJavaProxy
    {
        public CompletionCallback() : base("android.media.MediaPlayer$OnCompletionListener") {}

        void onCompletion(AndroidJavaObject jo)
        {
            AudioManager.Instance.SongPlaying = false;
            AudioManager.Instance.PlayNextTrack();
        }
    }

    public class ErrorCallback : AndroidJavaProxy
    {
        public ErrorCallback() : base("android.media.MediaPlayer$OnErrorListener") {}

        void onError(AndroidJavaObject jo, int what, int extra)
        {
             Debug.Log(what);
        }
    }

    public class SongStartCallback : AndroidJavaProxy
    {
        public SongStartCallback() : base("com.Torus.Fractalscape.utils.MusicPlayer$OnSongStartListener") {}

        void onSongStart(int duration)
        {
            AudioManager.Instance.SongLoading = false;
            AudioManager.Instance.SongPlaying = true;
            WindowManager.Instance.GetWindow<MusicWindow>(WindowNames.MusicWindow).LoadNewSong(AudioManager.Instance.CurrentSong, duration);
        }
    }

    public class AudioManager : MonoBehaviour
    {
        private static AudioManager _instance;
        private Playlist _playlist;
        private Queue<Song> _queuedSongs;
        private bool _clipIsSong;
        private bool _loadNewSong;
        private Dictionary<string, Sprite> _albumArt;
        private List<Song> _songCache;
        private string _path;
        private float _lastSkipTime;

        private AndroidJavaObject _activity;
        private AndroidJavaObject _musicManager;

        [SerializeField] private AudioSource _sfxSource;
        [SerializeField] private int _songBufferSize;
        [SerializeField] private int _songCacheSize;

        public AudioManager()
        {
            SongPlaying = false;
        }

        private void Awake()
        {
            _path = "jar:file://" + Application.dataPath + "!/assets/Music/";
            _queuedSongs = new Queue<Song>();
            _albumArt = new Dictionary<string, Sprite>();
            _songCache = new List<Song>();

            if (_musicManager == null)
            {
                JniInit();
            }
        }

        private void Start()
        {
            var json = Instantiate(Resources.Load<TextAsset>("Playlist"));
            _playlist = JsonUtility.FromJson<Playlist>(json.text);
            StartCoroutine(LoadNewSongs());
        }

        private void JniInit()
        {
            using (var activityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                _activity = activityClass.GetStatic<AndroidJavaObject>("currentActivity");
            }

            using (var pluginClass = new AndroidJavaClass("com.Torus.Fractalscape.utils.MusicPlayer"))
            {
                _musicManager = pluginClass.CallStatic<AndroidJavaObject>("instance");
            }
            _musicManager.Call("initializePlayer", _activity);
            _musicManager.Call("bindListeners", new CompletionCallback(), new ErrorCallback(),
                new SongStartCallback());    
        }

        public IEnumerator LoadNewSongs()
        {
            if (_songCache.Count > _songCacheSize) ReduceCacheSize();

            for (var i = 0; i < _songBufferSize; i++) //random every time
            {
                var upper = _playlist.Songs.Count - 1;
                var indx = Random.Range(0, upper);
                var song = _playlist.Songs[indx];

                if (!_albumArt.ContainsKey(song.Art))
                {
                    _albumArt.Add(song.Art, Instantiate(Resources.Load<Sprite>("AlbumArt/" + song.Art)));
                }
                QueueSong(song);
                yield return null;
            }
            PlayNextTrack();
        }

        public void QueueSong(Song loadedSong)
        {
            _playlist.Songs.Remove(loadedSong);
            _queuedSongs.Enqueue(loadedSong);
        }

        public void EmptyCache()
        {
            _songCache = new List<Song>();
        }

        public void ReduceCacheSize()
        {
            var reduction = (_songCache.Count - _songCacheSize);
            Debug.Log(reduction);
            Debug.Log(_songCache.Count);
            for (var i = 0; i < reduction; i++)
            {
                var song = _songCache[i];
                _playlist.Songs.Add(song);
                _songCache.Remove(song);
            }
        }

        public void CacheSong(Song song)
        {
            _songCache.Add(song);
        }


        public Song LoadSongFromCache(int indx)
        {
            var song = _songCache[indx];
            _songCache.RemoveAt(indx);
            return song;
        }

        public static AudioManager Instance
        {
            get {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<AudioManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("AudioManager");
                        DontDestroyOnLoad(go);
                        _instance = go.AddComponent<AudioManager>();
                    }
                }
                return _instance;
            }
        }

        public void PlayNextTrack()
        {
            Debug.Log("Cache length:" + _songCache.Count);
            Debug.Log("Queue length:" + _queuedSongs.Count);
            Debug.Log("Playlist length:" + _playlist.Songs.Count);

            if (CurrentSong != null && !_songCache.Contains(CurrentSong))
            {
                 CacheSong(CurrentSong);
            }
            if (_queuedSongs.Count == 0)
            {
                StartCoroutine(LoadNewSongs());
                return;
            }
            var newSong = _queuedSongs.Dequeue();
            SongLoading = true;
            CurrentSong = newSong;
            _musicManager.Call("playSong", _activity, newSong.Name + ".mp3");
        }

        public void PauseSong()
        {
            _musicManager.Call<int>("pauseSong");
            SongPlaying = false;
            SongPaused = true;
        }

        public void ResumeSong()
        {
            var songPosition = _musicManager.Call<int>("resumeSong");
            WindowManager.Instance.GetWindow<MusicWindow>(WindowNames.MusicWindow).SetSongCurrentTime(songPosition);
            SongPlaying = true;
            SongPaused = false;
        }

        public void SkipBack()
        {
            Debug.Log("Cache length:" + _songCache.Count);
            Debug.Log("Queue length:" + _queuedSongs.Count);
            Debug.Log("Playlist length:" + _playlist.Songs.Count);
            if (_songCache.Count == 0) return;

                _playlist.Songs.Add(CurrentSong);
                var song = LoadSongFromCache(_songCache.Count - 1);
                SongLoading = true;
                CurrentSong = song;
                _musicManager.Call("playSong", _activity, song.Name + ".mp3");
                _lastSkipTime = Time.time;
        }

        public int CurrentTime()
        {
            return _musicManager.Call<int>("currentTime");
        }

        public Sprite GetAlbumArt(string art)
        {
            return _albumArt[art];
        }

        public bool SongPlaying { get; set; }

        public bool SongPaused { get; set; }

        public bool SongLoading { get; set; }

        public Song CurrentSong { private set; get; }
    }
}