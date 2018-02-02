using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Fractalscape
{
    public sealed class RequestProcessor : MonoBehaviour
    {
        private static RequestProcessor _instance;
        private List<IRequest> _runningRequests;

        private const string MaxProcessErrorMessage =
            "Error! Too many requests running at one time! Please try again later.";

        private void Awake()
        {
            _runningRequests = new List<IRequest>();
        }

        public static RequestProcessor Instance
        {
            get {
                if (_instance == null)   //If _instance is null then we find it from the scene
                {
                    _instance = FindObjectOfType<RequestProcessor>();
                    if (_instance == null)   //If you forgot to attach gamemanager to any object then this wil1 create one
                    {
                        GameObject go = new GameObject("RequestProcessor");
                        DontDestroyOnLoad(go);
                        _instance = go.AddComponent<RequestProcessor>();
                    }
                }
                return _instance;
            }
        }

        private void Update()
        {
            CurrentRequests = _runningRequests.Count;
            for (var i = 0; i < CurrentRequests; i++)
            {
                _runningRequests[i].Update();
                if (!_runningRequests[i].IsRunning() || _runningRequests[i] == null) _runningRequests.RemoveAt(i);
            }
        }

        public void MakeRequest(IRequest request, Action<bool, IRequest> callback)
        {
                _runningRequests.Add(request);
                CurrentRequest = request;
                request.Trigger(callback);
        }

        public void MakeRequest(IRequest request)
        {
            Assert.IsNotNull(request);
                _runningRequests.Add(request);
                CurrentRequest = request;
                request.Trigger(ErrorCallback);
        }

        private void ErrorCallback(bool success, IRequest request)
        {
            var window = success
                ? WindowManager.Instance.GetWindow<AlertWindow>(WindowNames.SuccessWindow)
                : WindowManager.Instance.GetWindow<AlertWindow>(WindowNames.FailureWindow);

            if (AppSession.InViewer)
            {
                WindowManager.Instance.GetWindow<ConfirmationWindow>(WindowNames.ConfirmationWindow)
                    .AddCallbackWindow(window.gameObject);
            }
            else
            {
                WindowManager.Instance.ChangeWindow(window.gameObject, false);
            }
            window.DisplayMessage(request.Status());

            request.FinalizeRequest(success);
        }

        private void ReportError(string message)
        {
            WindowManager.Instance.ChangeWindow(WindowNames.FailureWindow, false);
            var window = WindowManager.Instance.GetWindow<AlertWindow>(WindowManager.Instance.CurrentWindow);
            window.DisplayMessage(message);
        }

        public Coroutine StartChildCoroutine(IEnumerator i)
        {
            return StartCoroutine(i);
        }

        public IRequest CurrentRequest { get; private set; }

        public int CurrentRequests { get; private set; }
    }
}