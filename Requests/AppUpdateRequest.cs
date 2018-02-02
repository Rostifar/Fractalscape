using System;
using System.Collections;
using System.IO;
using System.Threading;
using UnityEngine;

namespace Fractalscape
{
    public class AppUpdateRequest : IRequest
    {
        private string _objName = "AppFeed.zip";
        private string _dirName = "AppFeed";
        private string _currentUpdateVersion;
        private string _localVersion;
        private string _versionMetadataTag = "x-amz-meta-updateversion";
        private bool _metadataCheckStarted;
        private bool _appUpdateRequestStarted;
        private bool _running = true;

        private Job _metadataRequestJob;
        private Job _downloadRequestJob;
        private Job _unzipPackageJob;
        private FractalscapeWebClient _webClient;
        private Action<bool, IRequest> _callback;
        private StorageUtils _storageUtils;


        public void Trigger(Action<bool, IRequest> callback)
        {
            _callback = callback;
            _webClient = new FractalscapeWebClient();
            _storageUtils = new StorageUtils();
            var path = Path.Combine(Application.persistentDataPath, _objName);
            var dir = Path.Combine(Application.persistentDataPath, _dirName);

            _metadataRequestJob = new Job(new Thread(delegate()
            {
                _webClient.GetObjectMetadata(_objName);
            }));
            _downloadRequestJob = new Job(new Thread(delegate()
            {
                _webClient.GetObject(_objName);
            }));
            _unzipPackageJob = new Job(new Thread(delegate()
            {
                _storageUtils.UnzipPackage(path, dir);
            }));
            _localVersion = PlayerPrefs.HasKey(_versionMetadataTag)
                ? PlayerPrefs.GetString(_versionMetadataTag)
                : "0";
            Start();
        }

        private void Start()
        {
            WindowManager.Instance.StartCoroutine(HandleMetadataDownload());
        }

        private IEnumerator HandleMetadataDownload()
        {
            yield return RequestProcessor.Instance.StartCoroutine(_metadataRequestJob.Start());
            if (_webClient.CurrentDataMeta.Error ||
                _localVersion == _webClient.CurrentDataMeta.MetaData[_versionMetadataTag])
            {
                _callback(false, this);
            }
            else
            {
                PlayerPrefs.SetString(_versionMetadataTag, _webClient.CurrentDataMeta.MetaData[_versionMetadataTag]);
                PlayerPrefs.Save();
                WindowManager.Instance.StartCoroutine(HandleDownloadRequest());
            }
        }

        private IEnumerator HandleDownloadRequest()
        {
            StorageUtils.ExpansiveSearch(_objName, _dirName);
            yield return RequestProcessor.Instance.StartCoroutine(_downloadRequestJob.Start());
            if (_webClient.CurrentDataDownload.Error)
            {
                _callback(false, this);
            }
            else
            {
                WindowManager.Instance.StartCoroutine(HandleUnpackageRequest());
            }
        }

        private IEnumerator HandleUnpackageRequest()
        {
            yield return RequestProcessor.Instance.StartCoroutine(_unzipPackageJob.Start());
            if (_storageUtils.Error)
            {
                _callback(false, this);
            }
            else
            {
                _callback(true, this);
                AppSession.UpdateAvailable = true;
                var update = JsonUtility.FromJson<AppUpdate>(File.ReadAllText(Path.Combine(Application.persistentDataPath, _dirName) + "/" + _dirName +
                                               ".json"));
                AppSession.AppUpdate = new UsableUpdate {Update = update, UpdateType = AppUpdate.Type.Normal};
            }
        }

        public void FinalizeRequest(bool success)
        {
            _running = false;
        }

        public string Status()
        {
            return "";
        }

        public bool IsRunning()
        {
            return _running;
        }

        public void Update()
        {
        }
    }
}