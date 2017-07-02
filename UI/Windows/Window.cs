using UnityEngine;

namespace Fractalscape
{
    public class Window : MonoBehaviour
    {
        [SerializeField] public string WindowName;

        public virtual AppSession.BackButtonBehavior SetBackButtonBehavior()
        {
             return AppSession.BackButtonBehavior.Revert;
        }

        public virtual void Cleanup() {}

        public virtual void Setup() {}

        public virtual void EnableSiblings() {}

        public virtual void DisableSiblings() {}

        public virtual string PartnerNavigationWindow() {return "SecondaryNavigationWindow";}

        public virtual bool UseBlackBackground() {return true;}
    }
}