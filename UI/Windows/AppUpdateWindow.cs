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

        public void Setup(AppUpdate data)
        {
            Debug.Log(Path.Combine(Application.persistentDataPath, "AppFeed/") + data.Image);
            byte[] b = File.ReadAllBytes(Path.Combine(Application.persistentDataPath, "AppFeed/") + data.Image);
            Debug.Log(b.Length);
            var texture = new Texture2D(512, 512);
            texture.LoadImage(b);
            _updateImage.sprite = Sprite.Create(texture, new Rect(0, 0, 512, 512), new Vector2(0, 0), 100f);
            _header.text = data.Header;
            _updateMessage.text = data.Message;
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
            Directory.Delete(Application.persistentDataPath + "/" + "AppFeed", true);
        }
    }
}