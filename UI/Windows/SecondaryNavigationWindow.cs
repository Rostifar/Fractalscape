using System;
using CurvedUI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Fractalscape
{
    public class SecondaryNavigationWindow : Window
    {
        public enum ButtonType
        {
            Default,
            Home,
            Custom
        }

        [SerializeField] private Image _backButton;
        [SerializeField] private Button _action;
        [SerializeField] private Sprite _defaultSprite;
        [SerializeField] private Sprite _homeSprite;

        private void Awake()
        {
            ActionOnPress = null;
            CurrentButton = ButtonType.Default;
        }

        public void GoBack()
        {
            if (ActionOnPress != null)
            {
                ActionOnPress();
                ActionOnPress = null;
            }
            WindowManager.Instance.Revert();
        }

        public void SwapBackButton(ButtonType type)
        {
            if (type == CurrentButton) return;
            CurrentButton = type;
            switch (type)
            {
                case ButtonType.Default:
                    _backButton.sprite = _defaultSprite;
                    break;
                case ButtonType.Home:
                    _backButton.sprite = _homeSprite;
                    break;
            }
        }

        public void SwapBackButton(Sprite sprite)
        {
            CurrentButton = ButtonType.Custom;
            _backButton.sprite = sprite;
        }

        public ButtonType CurrentButton { private set; get; }

        public Action ActionOnPress { set; get; }
    }
}