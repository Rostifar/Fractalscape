using System.Collections.Generic;
using UnityEngine;

namespace Fractalscape
{
    //A class which holds references...because fuck unity and fuck optimizations.
    public class AppData : MonoBehaviour
    {
        private static AppData _instance;

        public Canvas DynamicCanvas;
        public Canvas ExtraneousUpdateCanvas;
        public Canvas UpdateCanvas;
        public Canvas StaticCanvas;
        public Canvas NavigationCanvas;
        public Canvas NavigationCanvasController;
        public Canvas GodCanvasController;
        public List<Canvas> Canvases;

        public Material DefaultSkyBox;
        public GameObject Recticle;
        public GameObject BlackBackground;
        public GameObject StaticBackground;
        public GameObject NavigationPanel;

        public Dictionary<string, GameObject> Windows;
        public GameObject ProgressWindow;
        public GameObject LibraryWindow;
        public GameObject StoreWindow;
        public GameObject Menu;
        public GameObject OfflineWindow;
        public GameObject SettingsWindow;
        public GameObject ConfirmationWindow;
        public GameObject DefaultNavigationWindow;
        public GameObject FailureWindow;
        public GameObject SuccessWindow;
        public GameObject RequestLimitWindow;
        public GameObject MusicWindow;
        public GameObject ViewerWindow;
        public GameObject IntroductionWindow;
        public GameObject ViewerTutorialWindow;
        public GameObject ViewerNavigationWindow;
        public GameObject SecondaryNavigationWindow;
        public GameObject AppUpdateWindow;
     
        public Dictionary<string, GameObject> Fractals;
        public GameObject MengersCrypt;
        public GameObject PoseidonsGate;
        public GameObject Exploration;

        public string AppId;

        private void Awake()
        {
            Canvases = new List<Canvas>
            {
                DynamicCanvas, ExtraneousUpdateCanvas, UpdateCanvas,
                StaticCanvas, NavigationCanvas, NavigationCanvasController,
                GodCanvasController
            };

            Windows = new Dictionary<string, GameObject>
            {
                {WindowNames.ProgressWindow, ProgressWindow},
                {WindowNames.LibraryWindow, LibraryWindow},
                {WindowNames.StoreWindow, StoreWindow},
                {WindowNames.Menu, Menu},
                {WindowNames.OfflineWindow, OfflineWindow},
                {WindowNames.SettingsWindow, SettingsWindow},
                {WindowNames.ConfirmationWindow, ConfirmationWindow},
                {WindowNames.DefaultNavigationWindow, DefaultNavigationWindow},
                {WindowNames.FailureWindow, FailureWindow},
                {WindowNames.SuccessWindow, SuccessWindow},
                {WindowNames.RequestLimitWindow, RequestLimitWindow},
                {WindowNames.MusicWindow, MusicWindow},
                {WindowNames.ViewerWindow, ViewerWindow},
                {WindowNames.IntroductionWindow, IntroductionWindow},
                {WindowNames.ViewerTutorialWindow, ViewerTutorialWindow},
                {WindowNames.ViewerNavigationWindow, ViewerNavigationWindow},
                {WindowNames.SecondaryNavigationWindow, SecondaryNavigationWindow},
                {WindowNames.AppUpdateWindow, AppUpdateWindow}
            };

            Fractals = new Dictionary<string, GameObject>
            {
                {"Menger'sCrypt", MengersCrypt},
                {"Poseidon'sGate", PoseidonsGate},
                {"Exploration", Exploration}
            };
        }
        public static AppData Ref
        {
            get {
                if (_instance == null)   //If _instance is null then we find it from the scene
                {
                    _instance = FindObjectOfType<AppData>();
                }
                return _instance;
            }
        }
    }
}