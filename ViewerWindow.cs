using System;
using UnityEngine;
using UnityEngine.UI;

namespace Fractalscape
{
    public class ViewerWindow : Window
    {
        private bool _accessedTutorial;

        [SerializeField] private Image _previewImage;
        public GameObject ViewerWindowExtraneous;

        public override void EnableSiblings()
        {
            ViewerWindowExtraneous.SetActive(true);
        }

        public override void DisableSiblings()
        {
            ViewerWindowExtraneous.SetActive(false);
        }

        public void SetPreviewImage(Sprite sprite)
        {
            _previewImage.sprite = sprite;
        }

        public void EnterExperience()
        {
            WindowManager.Instance.SetupForVideoPlayback();
            WindowManager.Instance.DeactivateCanvases();
            VideoPlayerManager.Instance.ActivateStereoViewer();
        }

        public void DisplayInstructions()
        {
            WindowManager.Instance.ChangeWindow(WindowNames.ViewerTutorialWindow, false);
        }

        public override void Setup()
        {
            WindowManager.Instance.GetWindow<ViewerNavigationWindow>(WindowNames.ViewerNavigationWindow)
                .ActivateIcon(ViewerNavigationWindow.ButtonType.Viewer);
            if (_previewImage == null || _previewImage.sprite != AppSession.SelectedItem.Image)
            {
                SetPreviewImage(AppSession.SelectedItem.Image);
            }
            EnableSiblings();
        }

        public override void Cleanup()
        {
             DisableSiblings();
        }

        public override string PartnerNavigationWindow()
        {
            return "ViewerNavigationWindow";
        }
    }
}