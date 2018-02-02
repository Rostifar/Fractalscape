using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Oculus.Platform;
using Oculus.Platform.Models;
using UnityEngine;
using UnityEngine.Assertions;

namespace Fractalscape
{
    public sealed class PurchaseRequest : IRequest
    {
        private Fractal _fractal;
        private readonly string _displayName;
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
        private const string FailureMessage = "Transaction failed! Please try again later or contact us.";
        private const string ProcessingMessage = "Processing...";

        public PurchaseRequest(string displayName, Fractal fractal)
        {
            _fractal = fractal;
            _displayName = displayName;
        }

        public void Setup()
        {
            _storeWindow = WindowManager.Instance.GetWindow<PrimaryWindow>(WindowNames.StoreWindow);
            _progressWindow = WindowManager.Instance.GetWindow<ProgressWindow>(WindowNames.ProgressWindow);
            _purchaseJob = new Job(new Thread(delegate() { 
            }));

            _requestData = new RequestData(new ProgressMessage {Header = PurchaseMessage, Body = ProcessingMessage});
            _progressWindow.AddProgressData(_requestData);
            WindowManager.Instance.ChangeWindow(WindowNames.ProgressWindow, false);
            _referencedItem = _storeWindow.GetMenuItem(_displayName);
            _referencedItem.ChangeTitleImageColour(MenuItem.ActivatedColourIndx);
            Menu.AddOpenedItem(_requestData, _displayName);
        }

        public void Trigger(Action<bool, IRequest> callback)
        {
            Setup();
            _callback = callback;

            HandlePurchase();
        }

        private void HandlePurchase() //this is sk
        {
            Debug.Log("Beginnning purchase request.");
            IAP.LaunchCheckoutFlow(_fractal.Sku).OnComplete(message =>
            {
                if (message.IsError)
                {
                    Debug.Log("Purchase failed");
                    _error = true;
                    _callback(false, this);
                }
                else
                {
                    AppSession.PurchasedFractals.Add(_fractal);
                    var str = JsonUtility.ToJson(AppSession.PurchasedFractals);
                    PlayerPrefs.SetString(LogNames.PurchasedFractals, str);
                    PlayerPrefs.Save();
                    _callback(true, this);
                }
            });             
        }

        public string AlertMessage(bool success)
        {
            return success ? SuccessMessage : FailureMessage;
        }

        public void FinalizeRequest(bool success)
        {
            Menu.RemoveOpenItem(_displayName);
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
                if (purchase.Name == sku) return true;
            }
            return false;
        }

        public static FractalLog ListToFractalLog(PurchaseList list)
        {
            Assert.IsNotNull(list);
            var log = new FractalLog();
            log.Fractals = new List<Fractal>();
            Assert.IsNotNull(log, "Log is NULL");
            Assert.IsNotNull(log.Fractals, "Log.Fractals is null");
            for (var i = 0; i < list.Count; i++)
            {
                Assert.IsNotNull(FractalLog.GetElementBySku(AppSession.DownloadedFractals, list[i].Sku), "FractalLog.GetElementBySku(AppSession.DownloadedFractals, list[i].Sku) != null");
                Assert.IsNotNull("");
                log.Fractals.Add(new Fractal
                {
                    Name = FractalLog.GetElementBySku(AppSession.DownloadedFractals, list[i].Sku).Name, 
                    Sku = list[i].Sku,
                    Type = 1
                });    
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