using System;
using Oculus.Platform.Models;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace Fractalscape
{
    public sealed class MenuItem : MonoBehaviour
    {
        public string Sku;
        public float Cost;
        public string Description;
        public Sprite Image;
        [SerializeField] private Image _titleImage;
        public bool IsSetup { get; private set; }
        public Material SkyboxMat;
        private RectTransform _itemTransform;

        public const int ActivatedColourIndx = 1;
        public const int DefaultColourIndx = 0;
        private static readonly Color32[] TitleColours =
        {
            new Color32(0x44, 0x44, 0x44, 0xAA),
            new Color32(0x22, 0xF2, 0x82, 0xAA)
        };

        //width, height
        public static readonly Vector2 Size = new Vector2(500, 450);

        //x, y, z
        public static readonly Vector3[] Positions =
        {
            new Vector3(-5f, 0f, 0f),
            new Vector3(0f, 0f, 0f),
            new Vector3(5f, 0f, 0f)
        };
        public static readonly Vector3 Scalars = new Vector3(0.0095f, 0.0095f, .01f);

        private void Awake()
        {
            IsSetup = false;
            gameObject.SetActive(false);

        }

        public void Select()
        {
            var newMenuType = Menu.Type.Download;
            var menu = WindowManager.Instance.GetWindow<Menu>(WindowNames.Menu);

            if (Menu.IsItemOpen(Sku))
            {
                Menu.GetOpenItem(Sku);
            }
            else
            {
                if (AppSession.DownloadedFractals.Contains(Sku))
                {
                    newMenuType = Menu.Type.View;
                }
                else if ((Cost <= 0 || PurchaseRequest.Purchased(Sku)) && AppSession.IsConnectedToInternet)
                {
                    newMenuType = Menu.Type.Download;
                }
                else if (Cost > 0 && AppSession.IsConnectedToInternet)
                {
                    newMenuType = Menu.Type.Purchase;
                }
                else if (!AppSession.IsConnectedToInternet)
                {
                    WindowManager.Instance.ChangeWindow(WindowNames.OfflineWindow);
                    return;
                }
                WindowManager.Instance.ChangeWindow(menu.gameObject);
                menu.Setup(this, newMenuType);
            }
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void Reveal()
        {
            gameObject.SetActive(true);
        }

        public void Setup(Transform parent)
        {
            Assert.IsNotNull(parent);
            _itemTransform.GetComponent<RectTransform>();
            _itemTransform.SetParent(parent);

        }

        public void Setup(int indx, Transform parent)
        {
            _itemTransform = gameObject.GetComponent<RectTransform>();
            _itemTransform.SetParent(parent);
            _itemTransform.anchoredPosition3D = Positions[indx];
            _itemTransform.sizeDelta = Size;
            _itemTransform.localScale = Scalars;
            IsSetup = true;
        }

        public void UpdatePosition(int newIndx)
        {
            _itemTransform.anchoredPosition3D = Positions[newIndx];
        }

        public void ChangeTitleImageColour(int indx = ActivatedColourIndx)
        {
            if (indx == ActivatedColourIndx || indx == DefaultColourIndx)
            {
                _titleImage.color = TitleColours[indx];
            }
        }

        public void GoToStore()
        {
            WindowManager.Instance.ChangeWindow(WindowNames.StoreWindow);
            WindowManager.Instance.GetWindow<NavigationWindow>
                (WindowNames.DefaultNavigationWindow).ActivateIcon(NavigationWindow.ButtonType.Store);
        }

        private string GetFileLocation()
        {
            return "";
        }

        public string File { private set; get; }
    }
}