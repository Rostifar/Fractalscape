using System;
using System.Collections;
using System.Threading;
using UnityEngine;

namespace Fractalscape
{
    public sealed class UninstallItemRequest : IRequest
    {
        private readonly string _displayName;
        private Fractal _fractal;
        private PrimaryWindow _libraryWindow;
        private ProgressWindow _progressWindow;
        private Menu _menu;
        private MenuItem _referencedItem;
        private Action<bool, IRequest> _callback;
        private Job _uninstallJob;
        private bool _error;
        private bool _running;
        private RequestData _requestData;
        private const string UninstallMessage = "Uninstalling Selected Experience";
        private const string ProcessingMessage = "Processing....";
        private const string SuccessMessage = "Your experience has been sucessfully uninstalled!";
        private const string FailureMessage = "Uninstall failed. The item must not be downloaded on your device. Moving item back to the store.";

        public UninstallItemRequest(string displayName, Fractal fractal)
        {
            _displayName = displayName;
            _fractal = fractal;
        }

        public void Setup()
        {
            _libraryWindow = WindowManager.Instance.GetWindow<PrimaryWindow>(WindowNames.LibraryWindow);
            _progressWindow = WindowManager.Instance.GetWindow<ProgressWindow>(WindowNames.ProgressWindow);
            _uninstallJob = new Job(new Thread(delegate() { MonoAssetIo.Instance.UninstallExperience(_fractal.Name); }));

            _requestData = new RequestData(new ProgressMessage{Header = UninstallMessage, Body = ProcessingMessage});
            _progressWindow.AddProgressData(_requestData);
            WindowManager.Instance.ChangeWindow(WindowNames.ProgressWindow, false);

            _referencedItem = _libraryWindow.GetMenuItem(_displayName);
            _referencedItem.ChangeTitleImageColour(MenuItem.ActivatedColourIndx);

            Menu.AddOpenedItem(_requestData, _displayName);
        }

        public void Trigger(Action<bool, IRequest> callback)
        {
            Setup();
            _callback = callback;
            RequestProcessor.Instance.StartCoroutine(HandleUninstall());
        }

        private IEnumerator HandleUninstall()
        {
            yield return RequestProcessor.Instance.StartCoroutine(_uninstallJob.Start());
            if (MonoAssetIo.Instance.Success)
            {
                var fractalLog = new FractalLog();
                
                for (var i = 0; i < AppSession.DownloadedFractals.Count; i++)
                {
                    if (AppSession.DownloadedFractals[i].Name == _fractal.Name) 
                        AppSession.DownloadedFractals.RemoveAt(i);
                }
                fractalLog.Fractals = AppSession.DownloadedFractals;
                PlayerPrefs.SetString(LogNames.DownloadedFractals, JsonUtility.ToJson(fractalLog));
                PlayerPrefs.Save();
            }
            _callback(MonoAssetIo.Instance.Success, this);
        }

        public void FinalizeRequest(bool success)
        {
            MonoAssetIo.Instance.ProcessFinished = false;
            MonoAssetIo.Instance.Success = false;
            _referencedItem.ChangeTitleImageColour(MenuItem.DefaultColourIndx);
            _libraryWindow.InverseAdd(_referencedItem);
            Menu.RemoveOpenItem(_displayName);
            WindowManager.Instance.GetWindow<PrimaryWindow>(WindowNames.StoreWindow).Cleanup();
            WindowManager.Instance.GetWindow<PrimaryWindow>(WindowNames.LibraryWindow).Cleanup();
            _running = false;
        }

        public string Status()
        {
            return _error ? FailureMessage : SuccessMessage;
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