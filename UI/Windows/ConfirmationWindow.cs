using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace Fractalscape
{
    public class ConfirmationWindow : Window
    {
        [SerializeField] private Text _msg;
        [SerializeField] private IRequest _pendingRequest;
        [SerializeField] private Button _acceptButton;
        [SerializeField] private Button _cancelButton;
        private GameObject _callbackWindow;
        public GameObject ConfirmationWindowExtraneous;
        

        public enum ButtonState
        {
            Default,
            Viewer
        }

        private void Awake()
        {
            _acceptButton.onClick.AddListener(AcceptAction);
            _cancelButton.onClick.AddListener(CancelAction);
            IsSetup = false;
        }

        public override void EnableSiblings()
        {
            ConfirmationWindowExtraneous.SetActive(true);
        }

        public override void DisableSiblings()
        {
            ConfirmationWindowExtraneous.SetActive(false);
        }

        public override void Setup()
        {
            EnableSiblings();
        }

        public override void Cleanup()
        {
            if (WindowManager.Instance.NewWindow is ProgressWindow)
            {
                WindowManager.Instance.PopPreviousWindow();
            }
            DisableSiblings();
        }

        public void SetButtonState(ButtonState state)
        {
            _acceptButton.onClick.RemoveAllListeners();
            _cancelButton.onClick.RemoveAllListeners();
            switch (state)
            {
                case ButtonState.Default:
                    _acceptButton.onClick.AddListener(AcceptAction);
                    _cancelButton.onClick.AddListener(CancelAction);
                    break;
                case ButtonState.Viewer:
                    _acceptButton.onClick.AddListener(ReturnToHome);
                    _cancelButton.onClick.AddListener(CancelAction);
                    break;
            }
        }

        public void DisplayMessage(string message)
        {
            _msg.text = message;
        }

        public void SetPendingRequest(IRequest req)
        {
            _pendingRequest = req;
        }

        public void AcceptAction()
        {
            Assert.IsNotNull(_pendingRequest);
            RequestProcessor.Instance.MakeRequest(_pendingRequest);
        }

        public void CancelAction()
        {
            WindowManager.Instance.Revert();
        }

        public void AddCallbackWindow(GameObject window)
        {
            _callbackWindow = window;
        }

        public void ReturnToHome()
        {
            IsSetup = false;
            WindowManager.Instance.EmptyStack();
            VideoPlayerManager.Instance.StopVideo();
            VideoPlayerManager.Instance.DeactivateStereoViewer();
            AppSession.InViewer = false;
            
            if (_callbackWindow != null)
            {
                WindowManager.Instance.ChangeWindow(_callbackWindow, false);
                WindowManager.Instance.AddToStack(WindowManager.Instance.GetWindow<PrimaryWindow>(WindowNames.LibraryWindow).gameObject);
                _callbackWindow = null;
            }
            else WindowManager.Instance.ChangeWindow(WindowNames.LibraryWindow);

            SetButtonState(ButtonState.Default);
        }

        public bool IsSetup { get; set; }
    }
}
