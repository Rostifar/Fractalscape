using System;
using System.Collections;
using System.Collections.Generic;
using Amazon.S3.Model;
using Ionic.Zip;
using UnityEngine;
using UnityEngine.UI;

namespace Fractalscape
{
    public struct ProgressData
    {
        public string Id;
        public string Header;
        public string Body;
        public ProgressWindow.ProgressType Type;
    }

    public class ProgressWindow : Window
    {
        [SerializeField] private Text _progressText;
        [SerializeField] private Text _titleText;
        private RequestData _currentStream;
        private static Dictionary<string, RequestData> OpenStreams = new Dictionary<string, RequestData>();
        public GameObject ProgressWindowUpdate;

        public enum ProgressType
        {
            Background,
            Short
        }


        public override void EnableSiblings()
        {
            ProgressWindowUpdate.SetActive(true);
        }

        public override void DisableSiblings()
        {
            ProgressWindowUpdate.SetActive(false);
        }

        public void AddProgressData(RequestData data)
        {
            _currentStream = data;
            OpenStreams.Add(data.Id, data);
        }

        public void SetCurrentDataSource(string id)
        {
            try
            {
                _currentStream = OpenStreams[id];
            }
            catch (Exception e)
            {
                Debug.Log("Error! Stream does not exist.");
            }
        }


        public void UpdateProgress(string id, WriteObjectProgressArgs e)
        {
            if (_currentStream.Id != id) return;
            _progressText.text = e.PercentDone + "%" + "\t" + "Downloaded: " + SizeSuffix(e.TransferredBytes) + " / " +
                                 SizeSuffix(e.TotalBytes);

        }

        public void UpdateProgress(string id, ReadProgressEventArgs e)
        {
            if (_currentStream.Id != id) return;
            _progressText.text = "Unpacked: " + SizeSuffix(e.BytesTransferred) + " / " +
                                 SizeSuffix(e.BytesTransferred);
        }

        public void UpdateProgress(string id, int percent, long low, long high)
        {
            if (_currentStream.Id != id) return;
            _progressText.text = percent + "%" + "\t" + "Downloaded: " + SizeSuffix(low) + " / " +
                                 SizeSuffix(high);
        }

        public void UpdateMessage(string id)
        {
            if (_currentStream.Id != id)
            {
                _currentStream.ActivateNewMessage();
            }
            else
            {
                _currentStream.ActivateNewMessage(_titleText, _progressText);
            }
        }

        public override void Setup()
        {
            _titleText.text = _currentStream.Message.Header;
            _progressText.text = _currentStream.Message.Body;
            EnableSiblings();
        }

        public override void Cleanup()
        {
            DisableSiblings();
        }

        static readonly string[] SizeSuffixes =
            { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
        static string SizeSuffix(long value, int decimalPlaces = 1)
        {
            if (value < 0) { return "-" + SizeSuffix(-value); }
            if (value == 0) { return "0.0 bytes"; }

            // mag is 0 for bytes, 1 for KB, 2, for MB, etc.
            var mag = (int)Math.Log(value, 1024);

            // 1L << (mag * 10) == 2 ^ (10 * mag)
            // [i.e. the number of bytes in the unit corresponding to mag]
            var adjustedSize = (decimal)value / (1L << (mag * 10));

            // make adjustment when the value is large enough that
            // it would round up to 1000 or more
            if (Math.Round(adjustedSize, decimalPlaces) >= 1000)
            {
                mag += 1;
                adjustedSize /= 1024;
            }

            return string.Format("{0:n" + decimalPlaces + "} {1}",
                adjustedSize,
                SizeSuffixes[mag]);
        }
    }
}