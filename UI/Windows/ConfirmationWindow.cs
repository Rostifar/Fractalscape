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

        public void ReturnToHome()
        {
            IsSetup = false;
            WindowManager.Instance.ChangeWindow(WindowNames.LibraryWindow);
            WindowManager.Instance.EmptyStack();
            VideoPlayerManager.Instance.StopVideo();
            VideoPlayerManager.Instance.DeactivateStereoViewer();
            AppSession.InViewer = false;
            SetButtonState(ButtonState.Default);
        }

        public bool IsSetup { get; set; }
    }
}
