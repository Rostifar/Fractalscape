using CurvedUI;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace Fractalscape
{
    //Component of all navigation windows. Use functions as needed.
    public sealed class NavigationWindow : Window
    {
        [SerializeField] private CUI_ZChangeOnHover _exitButtonTran;
        [SerializeField] private CUI_ZChangeOnHover _settingsButtonTran;
        [SerializeField] private CUI_ZChangeOnHover _musicButtonTran;

        public Image HomeButtonIcon;
        public Image StoreButtonIcon;
        public Image SettingsButtonIcon;
        private ButtonType _activatedButton;

        [SerializeField]private Sprite _activatedHomeButton;
        [SerializeField]private Sprite _activatedStoreButton;

        private Sprite _deactivatedHomeButton;
        private Sprite _deactivatedStoreButton;

        public enum ButtonType
        {
            Home,
            Store
        }

        private void Awake()
        {
            _activatedButton = ButtonType.Home;
            _deactivatedHomeButton = HomeButtonIcon.sprite;
            _deactivatedStoreButton = StoreButtonIcon.sprite;
            HomeButtonIcon.sprite = _activatedHomeButton;
        }

        public void OpenSettings()
        {
            _settingsButtonTran.FlattenObj();
            WindowManager.Instance.ChangeWindow(WindowNames.SettingsWindow);
        }

        public void ExitApp()
        {
            _exitButtonTran.FlattenObj();
            OVRManager.PlatformUIConfirmQuit();
        }

        public void OpenStore()
        {
            WindowManager.Instance.ChangeWindow(AppSession.IsConnectedToInternet
                ? WindowNames.StoreWindow
                : WindowNames.OfflineWindow);
            ActivateIcon(ButtonType.Store);
        }

        public void OpenLibrary()
        {
            WindowManager.Instance.ChangeWindow(WindowNames.LibraryWindow);
            ActivateIcon(ButtonType.Home);
        }

        public void OpenMusicPlayer()
        {
            _musicButtonTran.FlattenObj();
            SettingsButtonIcon.gameObject.GetComponent<CUI_ZChangeOnHover>().FlattenObj();
            WindowManager.Instance.ChangeWindow(WindowNames.MusicWindow);
        }

        public void SwipeRight()
        {
            var currentWindow = WindowManager.Instance.GetWindow<PrimaryWindow>(WindowManager.Instance.CurrentWindow);
            Assert.IsNotNull(currentWindow);
            currentWindow.Refresh(PrimaryWindow.RefreshDirection.Right);
        }

        public void SwipeLeft()
        {
            var currentWindow = WindowManager.Instance.GetWindow<PrimaryWindow>(WindowManager.Instance.CurrentWindow);
            Assert.IsNotNull(currentWindow);
            currentWindow.Refresh(PrimaryWindow.RefreshDirection.Left);
        }

        public void DeactivateIcon(ButtonType buttonType)
        {
            switch (buttonType)
            {
                case ButtonType.Home:
                    HomeButtonIcon.sprite = _deactivatedHomeButton;
                    break;
                case ButtonType.Store:
                    StoreButtonIcon.sprite = _deactivatedStoreButton;
                    break;
            }
        }

        public void ActivateIcon(ButtonType buttonType)
        {
            if (buttonType == _activatedButton) return;
            switch (buttonType)
            {
                case ButtonType.Home:
                    HomeButtonIcon.sprite = _activatedHomeButton;
                    break;
                case ButtonType.Store:
                    StoreButtonIcon.sprite = _activatedStoreButton;
                    break;
            }
            DeactivateIcon(_activatedButton); //change assets to match in code
            _activatedButton = buttonType;
        }
    }
}