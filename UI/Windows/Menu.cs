using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fractalscape
{
    public sealed class Menu : Window
    {
        [SerializeField] private Image _thumbnail;
        [SerializeField] private Text _description;
        [SerializeField] private Text _title;
        [SerializeField] private Text _cost;
        [SerializeField] private Material _defaultSkybox;
        [SerializeField] private Image _costOverlay;
        [SerializeField] private Button _downloadButton;
        [SerializeField] private Button _uninstallButton;
        [SerializeField] private Button _purchaseButton;
        [SerializeField] private Button _viewButton;

        private string _sku;
        private static Dictionary<string, string> _openedItems = new Dictionary<string, string>();
        private ConfirmationWindow _confirmationWindow;
        private const string DownloadMessage = "Are you sure you would like to download this experience?";
        private const string PurchaseMessage = "Are you sure you would like to purchase this experience?";
        private const string UninstallMessage = "Are you sure you would like to uninstall this experience?";
        public GameObject MenuExtraneous;
        private Type _currentMenuType;

        public enum State
        {
            Normal,
            Redirect
        }

        public enum Type
        {
            Download,
            View,
            Purchase
        }

        public delegate void ExecutePendingRequest(IRequest req);

        public override void EnableSiblings()
        {
            MenuExtraneous.SetActive(true);
        }

        public override void DisableSiblings()
        {
            MenuExtraneous.SetActive(false);
        }

        public override bool UseBlackBackground()
        {
            return false;
        }

        private void Start()
        {
            _confirmationWindow = WindowManager.Instance.GetWindow<ConfirmationWindow>(WindowNames.ConfirmationWindow);
        }

        public override void Setup()
        {
            EnableSiblings();
        }

        public override void Cleanup()
        {
            if (WindowManager.Instance.NewWindow is PrimaryWindow)
            {
                AppSession.Skybox.LoadTexture(_defaultSkybox);
            }
            DisableSiblings();
        }

        public void Setup(MenuItem item, Type type)
        {
            switch (type)
            {
                case Type.Download:
                    _downloadButton.gameObject.SetActive(true);
                    _uninstallButton.gameObject.SetActive(false);
                    _purchaseButton.gameObject.SetActive(false);
                    _viewButton.gameObject.SetActive(false);
                    _costOverlay.gameObject.SetActive(false);
                    break;
                case Type.View:
                    _downloadButton.gameObject.SetActive(false);
                    _uninstallButton.gameObject.SetActive(true);
                    _purchaseButton.gameObject.SetActive(false);
                    _viewButton.gameObject.SetActive(true);
                    _costOverlay.gameObject.SetActive(false);
                    break;
                case Type.Purchase:
                    _downloadButton.gameObject.SetActive(false);
                    _uninstallButton.gameObject.SetActive(false);
                    _purchaseButton.gameObject.SetActive(false);
                    _viewButton.gameObject.SetActive(false);
                    _costOverlay.gameObject.SetActive(true);
                    _cost.text = "$" + item.Cost.ToString("R");
                    break;
            }
            _thumbnail.sprite = item.Image;
            _description.text = item.Description;
            _title.text = item.Sku;
            _sku = item.Sku;
            AppSession.Skybox.LoadTexture(item.SkyboxMat);
            AppSession.SelectedItem = item;
        }

        public void DownloadItem()
        {
            WindowManager.Instance.ChangeWindow(WindowNames.ConfirmationWindow, false);
            _confirmationWindow.SetPendingRequest(new DownloadRequest(_sku));
            _confirmationWindow.DisplayMessage(DownloadMessage);
        }

        public void PurchaseItem()
        {

            WindowManager.Instance.ChangeWindow(WindowNames.ConfirmationWindow, false);
            _confirmationWindow.SetPendingRequest(new PurchaseRequest(_sku));
            _confirmationWindow.DisplayMessage(PurchaseMessage);
        }

        public void UninstallItem()
        {

            WindowManager.Instance.ChangeWindow(WindowNames.ConfirmationWindow, false);
            _confirmationWindow.SetPendingRequest(new UninstallItemRequest(_sku));
            _confirmationWindow.DisplayMessage(UninstallMessage);
        }

        public void ViewItem()
        {
            AppSession.InViewer = true;
            var newRequestData = new RequestData(new ProgressMessage{Header = "Loading...", Body = "Loading Your Experience"});
            WindowManager.Instance.GetWindow<ProgressWindow>(WindowNames.ProgressWindow).AddProgressData(newRequestData);
            WindowManager.Instance.ChangeWindow(WindowNames.ProgressWindow);
            WindowManager.Instance.GetWindow<ConfirmationWindow>(WindowNames.ConfirmationWindow)
                .SetButtonState(ConfirmationWindow.ButtonState.Viewer);

            if (AppSession.FirstTimeUser)
            {
                AppSession.FirstTimeUser = false;
                WindowManager.Instance.ChangeWindow(WindowNames.ViewerTutorialWindow, false);
                WindowManager.Instance.GetWindow<SecondaryNavigationWindow>(WindowNames.SecondaryNavigationWindow)
                        .ActionOnPress =
                    () =>
                    {
                        VideoPlayerManager.Instance.LoadVideo(_sku + "/" + _sku + ".mp4");
                    };
            }
            else
            {
                VideoPlayerManager.Instance.LoadVideo(_sku + "/" + _sku + ".mp4");
            }
        }

        public static void AddOpenedItem(RequestData data, string sku)
        {
            _openedItems.Add(sku, data.Id);
        }

        public static void RemoveOpenItem(string itemName)
        {
            if (IsItemOpen(itemName))
            {
                _openedItems.Remove(itemName);
            }
        }

        public static void GetOpenItem(string itemName)
        {
            if (IsItemOpen(itemName))
            {
                var data = _openedItems[itemName];
                WindowManager.Instance.GetWindow<ProgressWindow>(WindowNames.ProgressWindow).SetCurrentDataSource(data);
                WindowManager.Instance.ChangeWindow(WindowNames.ProgressWindow, false);
            }
        }

        public static bool IsItemOpen(string itemName)
        {
            return _openedItems.ContainsKey(itemName);
        }
    }
}