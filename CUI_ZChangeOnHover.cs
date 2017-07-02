using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

namespace CurvedUI
{
    //From a CURVEDUI demo. While it may appear inefficient, any modification to this class will only
    //result in disappointment, dispair, and frustration. TRUST ME.
    public class CUI_ZChangeOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public float restZ = 0;
        public float OnHoverZ = -50;
        public bool Zoomed = false;

        void Update()
        {
            if((transform as RectTransform).anchoredPosition3D.z == 0 && !Zoomed) return;
            (transform as RectTransform).anchoredPosition3D = (transform as RectTransform).anchoredPosition3D.ModifyZ(Mathf.Clamp((Zoomed ?
                (transform as RectTransform).anchoredPosition3D.z + Time.deltaTime * (OnHoverZ - restZ) * 6 :
                (transform as RectTransform).anchoredPosition3D.z - Time.deltaTime * (OnHoverZ - restZ) * 6), OnHoverZ, restZ));
        }

        public void FlattenObj()
        {
            (transform as RectTransform).anchoredPosition3D.ModifyZ(restZ);
            Zoomed = false;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            Zoomed = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {

            Zoomed = false;
        }
    }
}