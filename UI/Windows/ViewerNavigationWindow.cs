using CurvedUI;
using UnityEngine;
using UnityEngine.UI;

namespace Fractalscape
{
    public class ViewerNavigationWindow : Window
    {
        [SerializeField] private Image _musicPlayerImage;
        [SerializeField] private Image _viewerImage;

        [SerializeField] private Sprite _activatedMusicPlayerSprite;
        [SerializeField] private Sprite _activatedViewerSprite;

        [SerializeField] private CUI_ZChangeOnHover _homeButtonTran;


        private Sprite _deactivatedMusicPlayerSprite;
        private Sprite _deactivatedViewerSprite;
        private ButtonType _activatedButton;

        public enum ButtonType
        {
            Music,
            Viewer
        }

        private void Awake()
        {
            _activatedButton = ButtonType.Music;
            _deactivatedMusicPlayerSprite = _musicPlayerImage.sprite;
            _deactivatedViewerSprite = _viewerImage.sprite;
            ActivateIcon(ButtonType.Viewer);
        }

        public void OpenViewer()
        {
            WindowManager.Instance.ChangeWindow(WindowNames.ViewerWindow);
            ActivateIcon(ButtonType.Viewer);
        }

        public void OpenMusicPlayer()
        {
            WindowManager.Instance.ChangeWindow(WindowNames.MusicWindow);
            ActivateIcon(ButtonType.Music);
        }

        public void GoHome()
        {
            _homeButtonTran.FlattenObj();
            WindowManager.Instance.ChangeWindow(WindowNames.ConfirmationWindow, false);
            var confWindow = WindowManager.Instance.GetWindow<ConfirmationWindow>(WindowNames.ConfirmationWindow);
            confWindow.DisplayMessage("Would you like to return to the main menu?");
        }

        public void ActivateIcon(ButtonType buttonType)
        {
            if (buttonType == _activatedButton) return;
            switch (buttonType)
            {
                case ButtonType.Music:
                    _musicPlayerImage.sprite = _activatedMusicPlayerSprite;
                    break;
                case ButtonType.Viewer:
                    _viewerImage.sprite = _activatedViewerSprite;
                    break;
            }
            DeactivateIcon(_activatedButton); //change assets to match in code
            _activatedButton = buttonType;
        }

        public void DeactivateIcon(ButtonType buttonType)
        {
            switch (buttonType)
            {
                case ButtonType.Music:
                    _musicPlayerImage.sprite = _deactivatedMusicPlayerSprite;
                    break;
                case ButtonType.Viewer:
                    _viewerImage.sprite = _deactivatedViewerSprite;
                    break;
            }
        }
    }
}