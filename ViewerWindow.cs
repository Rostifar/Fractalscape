using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

namespace Fractalscape
{
    public class ViewerWindow : Window
    {
        private bool _accessedTutorial;

        [SerializeField] private Image _previewImage;
        private MenuItem _currentItem; 
        public GameObject ViewerWindowExtraneous;

        public override void EnableSiblings()
        {
            ViewerWindowExtraneous.SetActive(true);
            
            _previewImage.sprite = _currentItem.ThiccImage;
        }

        public override void DisableSiblings()
        {
            ViewerWindowExtraneous.SetActive(false);
        }

        public void SetCurrentItem(MenuItem item)
        {
            _currentItem = item;
            _previewImage.sprite = item.ThiccImage;
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
            
            Debug.Log("Viewing:" + AppSession.SelectedItem.Name);

            GetComponentInChildren<Image>().sprite = _currentItem.ThiccImage;
            
            _previewImage.SetAllDirty();
            
            Debug.Log(_previewImage.sprite.name);

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