using RenderHeads.Media.AVProVideo;
using UnityEngine;

namespace Fractalscape
{
    public class VideoPlayerManager : MonoBehaviour
    {
        private static VideoPlayerManager _instance;

        [SerializeField] private MediaPlayer _avproPlayer;
        [SerializeField] private CubemapCube _cubemapCube;
        [SerializeField] private UpdateStereoMaterial _updateStereoMaterial;

        private const string VideoErrorMessage =
            "Error! Your experience could not be loaded. Please re-download it or contact Torus Support.";

        public static VideoPlayerManager Instance
        {
            get {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<VideoPlayerManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("VideoPlayerManager");
                        DontDestroyOnLoad(go);
                        _instance = go.AddComponent<VideoPlayerManager>();
                    }
                }
                return _instance;
            }
        }

        private void Awake()
        {
            _avproPlayer.Events.AddListener(OnVideoEvent);
            _updateStereoMaterial._camera = AppSession.AppCamera;
            DeactivateStereoViewer();
        }

        private void OnVideoEvent(MediaPlayer player, MediaPlayerEvent.EventType eventType, ErrorCode errorCode)
        {
            Debug.Log(eventType);
            switch (eventType)
            {
                case MediaPlayerEvent.EventType.FirstFrameReady:
                    if (AppSession.InViewer)
                    {
                        WindowManager.Instance.SetupForVideoPlayback();
                        ActivateStereoViewer();
                    }
                    break;
                case MediaPlayerEvent.EventType.Error:
                    if (AppSession.InViewer)
                    {
                        AppSession.InViewer = false;
                        WindowManager.Instance.ChangeWindow(WindowNames.FailureWindow);
                        WindowManager.Instance.GetWindow<AlertWindow>(WindowNames.AlertWindow).DisplayMessage(VideoErrorMessage);
                    }
                    break;
            }
        }

        public void LoadVideo(string path)
        {
            _avproPlayer.OpenVideoFromFile(MediaPlayer.FileLocation.RelativeToPeristentDataFolder, path);
        }
   
        public void LoadVideoStreamingAssets(string path)
        {
            _avproPlayer.OpenVideoFromFile(MediaPlayer.FileLocation.RelativeToStreamingAssetsFolder, path);
        }

        public void SetVideoLooping(bool loop)
        {
            _avproPlayer.m_Loop = loop;
        }

        public void PauseVideo()
        {
            if (VideoOpen) _avproPlayer.Pause();
        }

        public void PlayVideo()
        {
            if (VideoOpen && VideoCanPlay) _avproPlayer.Play();
        }

        public void CloseVideo()
        {
            if (VideoOpen) _avproPlayer.Control.CloseVideo();
        }

        public void StopVideo()
        {
            if (VideoOpen) _avproPlayer.Stop();
        }

        public void ActivateUpdateStereoMaterial()
        {
            _updateStereoMaterial.gameObject.SetActive(true);
        }

        public void DeactivateUpdateStereoMaterial()
        {
            _updateStereoMaterial.gameObject.SetActive(false);
        }

        public void ActivateVideoCubeMap()
        {
            _cubemapCube.gameObject.SetActive(true);
        }

        public void DeactivateVideoCubeMap()
        {
            _cubemapCube.gameObject.SetActive(false);
        }

        public void ActivateStereoViewer()
        {
            _cubemapCube.gameObject.SetActive(true);
            _updateStereoMaterial.gameObject.SetActive(true);
            if (VideoCanPlay) PlayVideo();
        }

        public void DeactivateStereoViewer()
        {
            if (_avproPlayer.Control != null && VideoPlaying) PauseVideo();
            _cubemapCube.gameObject.SetActive(false);
            _updateStereoMaterial.gameObject.SetActive(false);
        }

        public bool VideoOpen {
            get { return _avproPlayer.VideoOpened; }
        }

        public bool VideoCanPlay
        {
            get { return _avproPlayer.Control.CanPlay(); }
        }

        public bool VideoPlaying
        {
            get { return _avproPlayer.Control.IsPlaying(); }
        }
    }
}