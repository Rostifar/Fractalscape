using System;
using System.Collections.Generic;
using RenderHeads.Media.AVProVideo;
using UnityEngine;
using UnityEngine.UI;

namespace Fractalscape
{
    public class TestVideos : MonoBehaviour
    {
        public MediaPlayer Player;
        public MediaPlayer Player1;
        public int L;
        public Text Txt;
        public List<string> Videos = new List<string>
        {
             "0.001", "0.01", "0.002", "0.02", "0.03", "0.04", "0.005", "0.05", "0.06", "0.07", "0.08", "0.09", "0.10", "0.12", "0.14"
        };

        public GameObject CubeStereo;
        public GameObject CubeMono;

        private void Awake()
        {
            Player.Events.AddListener(Call);
            CubeMono.SetActive(false);
            Txt.text = "stereo";
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (!CubeMono.activeSelf)
                {
                    CubeMono.SetActive(true);
                    CubeStereo.SetActive(false);
                    Player1.Play();
                    Player.Pause();

                    Txt.text = "mono";
                }
                else
                {
                    CubeMono.SetActive(false);
                    CubeStereo.SetActive(true);
                    Player1.Pause();
                    Player.Play();
                    Txt.text = "stereo";
                }
            }
        }

        private void Call(MediaPlayer arg0, MediaPlayerEvent.EventType eventType, ErrorCode errorCode)
        {
            switch (eventType)
            {
                case MediaPlayerEvent.EventType.FirstFrameReady:
                    Player.Play();
                    break;
            }
        }

        public void LoadNextVideo()
        {
            if (L == Videos.Count - 1) L = 0;
            if (Player.VideoOpened) Player.CloseVideo();
            Txt.text = Videos[L];
            Player.OpenVideoFromFile(MediaPlayer.FileLocation.RelativeToStreamingAssetsFolder, "TestVids/" + Videos[L] + ".mp4");
            L++;
        }
    }
}