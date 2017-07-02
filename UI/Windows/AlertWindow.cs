using UnityEngine;
using UnityEngine.UI;

namespace Fractalscape
{
    public class AlertWindow : Window
    {
        [SerializeField] private Text _displayedMsg;

        public override void Setup()
        {
            if (AppSession.InViewer)
            {
                WindowManager.Instance.OverrideWindow();
            }
        }


        public void DisplayMessage(string msg)
        {
            if (!AppSession.InViewer)
            {
                _displayedMsg.text = msg; 
            }
        }
    }
}