using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Oculus.Platform;
using Oculus.Platform.Models;
using UnityEngine;

namespace Fractalscape
{
    public sealed class PurchaseRequest : IRequest
    {
        private readonly string _sku;
        private Job _purchaseJob;
        private Message _response;
        private Action<bool, IRequest> _callback;
        private PrimaryWindow _storeWindow;
        private ProgressWindow _progressWindow;
        private Menu _menu;
        private MenuItem _referencedItem;
        private RequestData _requestData;
        private bool _error;
        private bool _running;
        private const string PurchaseMessage = "Purchasing Selected Experience";
        private const string SuccessMessage = "Transaction complete! Visit the main menu to download your experience.";
        private const string FailureMessage = "Transaction failed! Something must be wrong Oculus's servers. Please try again later.";
        private const string ProcessingMessage = "Processing...";

        public PurchaseRequest(string sku)
        {
            _sku = sku;
        }

        public void Setup()
        {
            _storeWindow = WindowManager.Instance.GetWindow<PrimaryWindow>(WindowNames.StoreWindow);
            _progressWindow = WindowManager.Instance.GetWindow<ProgressWindow>(WindowNames.ProgressWindow);
            _purchaseJob = new Job(new Thread(delegate() { IAP.LaunchCheckoutFlow(_sku).OnComplete(message =>
            {
                _response = message;
            }); }));

            _requestData = new RequestData(new ProgressMessage {Header = PurchaseMessage, Body = ProcessingMessage});
            _progressWindow.AddProgressData(_requestData);
            WindowManager.Instance.ChangeWindow(WindowNames.ProgressWindow, false);
            _referencedItem = _storeWindow.GetMenuItem(_sku);
            _referencedItem.ChangeTitleImageColour(MenuItem.ActivatedColourIndx);
            Menu.AddOpenedItem(_requestData, _sku);
        }

        public void Trigger(Action<bool, IRequest> callback)
        {
            Setup();
            _callback = callback;

            if (AppSession.OculusDown)
            {
                _callback(false, this);
            }
            RequestProcessor.Instance.StartCoroutine(HandlePurchase());
        }

        private IEnumerator HandlePurchase()
        {
            yield return RequestProcessor.Instance.StartCoroutine(_purchaseJob.Start());
            if (_response.IsError)
            {
                _error = true;
                _callback(false, this);
            }
            else
            {
                IAP.GetViewerPurchases().OnComplete(message =>
                {
                    string str;
                        if (message.IsError)
                        {
                            AppSession.PurchasedFractals.Add(_sku);
                            var log = new FractalLog {Fractals = AppSession.PurchasedFractals};
                            str = JsonUtility.ToJson(log);
                        }
                        else
                        {
                            var log = ListToFractalLog(message.GetPurchaseList());
                            str = JsonUtility.ToJson(log);
                            AppSession.PurchasedFractals = log.Fractals;
                        }
                    PlayerPrefs.SetString(LogNames.PurchasedFractals, str);
                    PlayerPrefs.Save();
                    });
                _callback(true, this);
            }
        }

        public string AlertMessage(bool success)
        {
            return success ? SuccessMessage : FailureMessage;
        }

        public void FinalizeRequest(bool success)
        {
            Menu.RemoveOpenItem(_sku);
            _referencedItem.ChangeTitleImageColour(MenuItem.DefaultColourIndx);
            _running = false;
        }

        public string Status()
        {
            return _error ? FailureMessage : SuccessMessage;
        }

        public static bool Purchased(string sku)
        {
            foreach (var purchase in AppSession.PurchasedFractals)
            {
                if (purchase == sku) return true;
            }
            return false;
        }

        public static FractalLog ListToFractalLog(PurchaseList list)
        {
            var log = new FractalLog();
            foreach (var purchase in list)
            {
                log.Fractals.Add(purchase.Sku);
            }
            return log;
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