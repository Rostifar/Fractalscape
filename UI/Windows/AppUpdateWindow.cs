using System;
using System.Collections;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace Fractalscape
{
    public class AppUpdateWindow : Window
    {
        [SerializeField] private Image _updateImage;
        [SerializeField] private Text _updateMessage;
        [SerializeField] private Text _header;
        private Thread _loadImageThread;

        public void Setup(UsableUpdate update)
        {
            if (update.UpdateType == AppUpdate.Type.Normal)
            {
                Debug.Log(Path.Combine(Application.persistentDataPath, "AppFeed/") + update.Update.Image);
                byte[] b = File.ReadAllBytes(Path.Combine(Application.persistentDataPath, "AppFeed/") + update.Update.Image);
                Debug.Log(b.Length);
                var texture = new Texture2D(512, 512);
                texture.LoadImage(b);
                _updateImage.sprite = Sprite.Create(texture, new Rect(0, 0, 512, 512), new Vector2(0, 0), 100f);
            }
            else
            {
                _updateImage.sprite = Resources.Load<Sprite>(update.Update.Image);
            }

            _header.text = update.Update.Header;
            _updateMessage.text = update.Update.Message;
        }

        public override void Setup()
        {
            WindowManager.Instance.GetWindow<SecondaryNavigationWindow>(PartnerNavigationWindow()).
                SwapBackButton(SecondaryNavigationWindow.ButtonType.Home);
        }

        public override void Cleanup()
        {
            WindowManager.Instance.GetWindow<SecondaryNavigationWindow>(PartnerNavigationWindow()).
                SwapBackButton(SecondaryNavigationWindow.ButtonType.Default);

            if (AppSession.AppUpdate.UpdateType == AppUpdate.Type.Normal)
            {
                Directory.Delete(Application.persistentDataPath + "/" + "AppFeed", true);
            }
        }
    }
}