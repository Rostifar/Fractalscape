using System;
using UnityEngine;

namespace Fractalscape
{
    public class TutorialWindow : Window
    {
        [SerializeField] private Sprite _newButtonImg;

        public override void Setup()
        {
            WindowManager.Instance.GetWindow<SecondaryNavigationWindow>(PartnerNavigationWindow()).
                SwapBackButton(_newButtonImg);
        }

        public override void Cleanup()
        {
            WindowManager.Instance.GetWindow<SecondaryNavigationWindow>(PartnerNavigationWindow()).
                SwapBackButton(SecondaryNavigationWindow.ButtonType.Default);
        }
    }
}