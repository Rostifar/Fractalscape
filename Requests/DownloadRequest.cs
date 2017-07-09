using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using Oculus.Platform;
using Oculus.Platform.Models;
using UnityEngine;
using Application = UnityEngine.Application;

namespace Fractalscape
{
    //Downloads a single object from an S3 bucket.
    //NOTE: Somewhat unstable, highly important, therefore I recommend that you do not modify this class.
    public sealed class DownloadRequest : IRequest
    {
        private readonly string _sku;
        private readonly string _expName;
        private FractalscapeWebClient _client;
        private Job _downloadJob;
        private Job _metadataDownloadJob;
        private Job _unpackageJob;
        private Action<bool, IRequest> _callback;
        private PrimaryWindow _storeWindow;
        private ProgressWindow _progressWindow;
        private MenuItem _referencedItem;
        private Menu _menu;
        private string _failureMessage = NotFoundMessage;
        private StorageUtils _storageUtils;
        private bool _error;
        private bool _running = true;
        private RequestData _requestData;

        private const string UnpackingMessage = "Unpacking Your Experience";
        private const string DownloadMessage = "Downloading Your Experience";
        private const string CheckingStorageMessage = "Checking Your Storage Capacity";
        private const string SuccessMessage =
            "Your experience has been successfully downloaded! Go back to the main menu to access it.";
        private const string InternalErrorMessage =
            "Error 500! Our servers must be down! Please try again later or contact us for support.";
        private const string NotFoundMessage = "Error 404! We could not find your requested experience!";
        private const string TimeoutMessage = "Error 408! Your download took to long. Please try again.";
        private const string OutOfStorageMessage = "Error! You do not have enough space to download this experience."  + "\n"
                                                   + "Required Space: ";
        private const string UnpackingUnsucessfulMessage = "Error! We could not unpackage your selected experience. Please redownload this experience.";
        private const string DefaultDownloadFailureMessage = "Error! Your experience failed to download. Please try again later.";
        private const string ProcessingMessage = "Processing....";

        public DownloadRequest(string sku, string filetype = ".zip")
        {
            _expName = sku;
            _sku = sku + filetype;
        }

        public void Setup()
        {
            var path = Path.Combine(Application.persistentDataPath, _sku);
            var dir = Path.Combine(Application.persistentDataPath, _expName);

            _storageUtils = new StorageUtils();
            _client = new FractalscapeWebClient();
            _downloadJob = new Job(new Thread(delegate() { _client.GetObject(_sku); }));
            _metadataDownloadJob = new Job(new Thread(delegate() {  _client.GetObjectMetadata(_sku);}));
            _unpackageJob = new Job(new Thread(delegate()
            {
                _storageUtils.UnzipPackage(path, dir);
            }));

            _storeWindow = WindowManager.Instance.GetWindow<PrimaryWindow>(WindowNames.StoreWindow);
            _progressWindow = WindowManager.Instance.GetWindow<ProgressWindow>(WindowNames.ProgressWindow);

            var headers = new List<string>
            {
                CheckingStorageMessage, DownloadMessage, UnpackingMessage
            };

            var bodies = new List<string>
            {
                ProcessingMessage, "", ProcessingMessage
            };

            _requestData = new RequestData(RequestData.ConstructMessages(headers, bodies));
            _progressWindow.AddProgressData(_requestData);
            WindowManager.Instance.ChangeWindow(WindowNames.ProgressWindow, false);
            _referencedItem = _storeWindow.GetMenuItem(_expName);
            _referencedItem.ChangeTitleImageColour(MenuItem.ActivatedColourIndx);
            Menu.AddOpenedItem(_requestData, _expName);
        }

        public void Trigger(Action<bool, IRequest> callback)
        {
            Setup();
            _callback = callback;
            RequestProcessor.Instance.StartCoroutine(HandleMetaDownload());
        }

        public IEnumerator HandleMetaDownload()
        {
            yield return RequestProcessor.Instance.StartCoroutine(_metadataDownloadJob.Start());
            if (_client.CurrentDataMeta.Error)
            {
                _error = true;
                ManageHttpError(_client.CurrentDataMeta.ResCode);
            }
            else
            {
                var uncompressedStorage = Convert.ToInt32(_client.CurrentDataMeta.MetaData["x-amz-meta-uncompressedspace"]);
                if (StorageUtils.SpaceAvailable(true, uncompressedStorage))
                {
                    _progressWindow.UpdateMessage(_requestData.Id);
                    RequestProcessor.Instance.StartCoroutine(HandleDownload());
                }
                else
                {
                    ManageFailure(OutOfStorageMessage + uncompressedStorage + " MB");
                }
            }
        }

        public IEnumerator HandleDownload()
        {
            yield return RequestProcessor.Instance.StartCoroutine(_downloadJob.Start());
            if (_client.CurrentDataDownload.Error)
            {
                _error = true;
                ManageHttpError(_client.CurrentDataDownload.ResCode);
            }
            else
            {
                _progressWindow.UpdateMessage(_requestData.Id);
                RequestProcessor.Instance.StartChildCoroutine(HandleUnpacking());
            }
        }

        public IEnumerator HandleUnpacking()
        {
            yield return RequestProcessor.Instance.StartCoroutine(_unpackageJob.Start());
            if (_storageUtils.Error)
            {
                _error = true;
                File.Delete(Path.Combine(Application.persistentDataPath, _sku));
                ManageFailure(UnpackingUnsucessfulMessage);
            }
            else
            {
                var fractalLog = new FractalLog();
                var fractal = new Fractal {Name = _expName, Type = 1};
                AppSession.DownloadedFractals.Add(fractal);
                fractalLog.Fractals = AppSession.DownloadedFractals;
                
                var str = JsonUtility.ToJson(fractalLog);
                PlayerPrefs.SetString(LogNames.DownloadedFractals, str);
                _callback(true, this);
            }
        }

        public void Update()
        {
            if (_downloadJob.Alive() && _client.ProgressArgs != null)
            {
                _progressWindow.UpdateProgress(_requestData.Id, _client.ProgressArgs);
            }
        }

        private void ManageHttpError(HttpStatusCode code)
        {
            switch (code)
            {
                case HttpStatusCode.NotFound:
                    ManageFailure(NotFoundMessage);
                    break;
                case HttpStatusCode.RequestTimeout:
                    ManageFailure(TimeoutMessage);
                    break;
                case HttpStatusCode.InternalServerError:
                    ManageFailure(InternalErrorMessage);
                    break;
                default:
                    ManageFailure(DefaultDownloadFailureMessage);
                    break;
            }
        }

        private void ManageFailure(string msg)
        {
            _failureMessage = msg;
            if (_callback != null) _callback(false, this);
        }

        public void FinalizeRequest(bool success)
        {
            _referencedItem.ChangeTitleImageColour(MenuItem.DefaultColourIndx);
            _storageUtils.Error = false;
            _storageUtils.UnzipFinished = false;
            if (success)
            {
                _storeWindow.InverseAdd(_referencedItem);
            }
            Menu.RemoveOpenItem(_expName);
            WindowManager.Instance.GetWindow<PrimaryWindow>(WindowNames.StoreWindow).Cleanup();
            WindowManager.Instance.GetWindow<PrimaryWindow>(WindowNames.LibraryWindow).Cleanup();
            _running = false;
        }

        public string Status()
        {
            return _error ? _failureMessage : SuccessMessage;
        }

        public bool IsRunning()
        {
            return _running;
        }
    }
}