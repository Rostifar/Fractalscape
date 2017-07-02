using System;
using System.Collections;
using System.Collections.Generic;
using CurvedUI;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.VR;

namespace Fractalscape
{
    public class WindowManager : MonoBehaviour
    {
        private GameObject _openNavigationWindow;
        private static WindowManager _instance;
        private Stack<GameObject> _windowStack;
        private Dictionary<string, GameObject> _cachedWindows;
        private List<string> _downloadedItems;
        private List<string> _availableItems;
        private Dictionary<string, GameObject> _testCacheWindow;
        private bool _windowIsStackable = true;
        private const string ExplorationMenuItem = "Exploration";
        private float _repositionTime;

        public enum CanvasType
        {
            God,
            Navigation
        }

        public static WindowManager Instance
        {
            get {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<WindowManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("AudioManager");
                        DontDestroyOnLoad(go);
                        _instance = go.AddComponent<WindowManager>();
                    }
                }
                return _instance;
            }
        }

        private void Awake()
        {
            OVRTouchpad.TouchHandler += OvrTouchpadOnTouchHandler;
            _windowStack = new Stack<GameObject>();
            _repositionTime = -1;
        }

        private void OvrTouchpadOnTouchHandler(object sender, OVRTouchpad.TouchArgs eventArgs)
        {
            var yRotation = InputTracking.GetLocalRotation(VRNode.CenterEye).eulerAngles.y;
            Debug.Log(yRotation );
            if (eventArgs.TouchType == OVRTouchpad.TouchEvent.SingleTap && (yRotation > 90 && yRotation < 270)
                && !AppSession.InViewer && !AppSession.OnMenu)
            {
                RepositionCanvases();
            }
        }

        public void ChangeWindow(string newWindowName, bool addToStack = true)
        {
            var newGo = _cachedWindows[newWindowName];
            NewWindow = GetWindow(newGo);

            if (CurrentWindow != null) ClosePreviousWindow(CurrentWindow);
            if (CurrentWindow == null ||
                NewWindow.PartnerNavigationWindow() != GetWindow(CurrentWindow).PartnerNavigationWindow())
            {
                ChangeNavigationWindow(NewWindow.PartnerNavigationWindow());
            }
            AppData.Ref.BlackBackground.SetActive(NewWindow.UseBlackBackground());
            AppSession.CurrentBackButtonBehavior = NewWindow.SetBackButtonBehavior();
            CurrentWindow = newGo;
            NewWindow.Setup();
            newGo.SetActive(true);
            _windowIsStackable = addToStack;
        }

        public void ClosePreviousWindow(GameObject previousWindow)
        {
            var window = GetWindow(previousWindow);
            if (_windowIsStackable) _windowStack.Push(previousWindow);
            window.Cleanup();
            previousWindow.SetActive(false);
        }

        public void ChangeWindow(GameObject newWindow, bool addToStack = true)
        {
            var window = GetWindow(newWindow);
            Assert.IsNotNull(window);
            ChangeWindow(window.WindowName, addToStack);
        }

        public void ChangeWindowNotNav(string newWindow, bool addToStack)
        {
            var newGo = _cachedWindows[newWindow];
            NewWindow = GetWindow(newGo);

            if (CurrentWindow != null)
            {
                ClosePreviousWindow(CurrentWindow);
            }
            CurrentWindow = NewWindow.gameObject;
            NewWindow.gameObject.SetActive(true);
            _windowIsStackable = addToStack;
        }


        public void ChangeNavigationWindow(string newWindowName)
        {
            var newGo = _cachedWindows[newWindowName];

            if (_openNavigationWindow != null)
            {
                _openNavigationWindow.SetActive(false);
            }
            _openNavigationWindow = newGo;
            newGo.gameObject.SetActive(true);
        }

        //Special case when exit button is hit.
        public void Revert()
        {
            if (CurrentWindow != null)
            {
                CurrentWindow.SetActive(false);
            }
            if (_windowStack.Count == 0)
            {
                ChangeWindow(AppSession.InViewer ? WindowNames.ViewerWindow : WindowNames.LibraryWindow);
            }
            else
            {
                for (;;)
                {
                    var window = _windowStack.Pop();
                    if (window == null) continue;
                    ChangeWindow(window);
                    return;
                }
            }
        }

        public void Revert(GameObject refWindow)
        {
            if (CurrentWindow != null)
            {
                CurrentWindow.SetActive(false);
            }
            if (_windowStack.Count == 0)
            {
                ChangeWindow(WindowNames.LibraryWindow);
            }
            else
            {
                for (;;)
                {
                    var window = _windowStack.Pop();
                    if (window == null || window != refWindow) continue;
                    ChangeWindow(window);
                    return;
                }
            }
        }

        private IEnumerator SetupGodCanvas()
        {
            var libraryWindow = GetWindow<PrimaryWindow>(WindowNames.LibraryWindow);
            var storeWindow = GetWindow<PrimaryWindow>(WindowNames.StoreWindow);
            var items = ListIntersection(_availableItems, _downloadedItems);
            yield return null;

            libraryWindow.gameObject.SetActive(true);
            storeWindow.gameObject.SetActive(true);
            AppData.Ref.StaticBackground.SetActive(true);
            AppData.Ref.NavigationPanel.SetActive(true);
            yield return null;

            if (AppSession.IsConnectedToInternet)
            {
                yield return StartCoroutine(storeWindow.Populate(items[0]));
                items[1].Add(ExplorationMenuItem);
            }
            yield return StartCoroutine(libraryWindow.Populate(items[1]));

            if (AppSession.FirstTimeUser)
            {
                ChangeWindow(WindowNames.IntroductionWindow, false);
            }
            else if (AppSession.UpdateAvailable)
            {
                GetWindow<AppUpdateWindow>(WindowNames.AppUpdateWindow).Setup(AppSession.AppUpdate);
                ChangeWindow(WindowNames.AppUpdateWindow, false);
            }
            else
            {
                ChangeWindow(WindowNames.LibraryWindow);
            }
            storeWindow.gameObject.SetActive(false);
            AppSession.AppCamera.cullingMask = -1;
        }
        private static List<string>[] ListIntersection(List<string> l1, List<string> l2)
        {
            var newL1 = new List<string>(); //Where l2 is a subset of l1
            var newL2 = new List<string>();

            for (var i = 0; i < l1.Count; i++)
            {
                var val = l1[i];
                if (l2.Contains(val))
                {
                    newL2.Add(val);
                }
                else
                {
                    newL1.Add(val);
                }
            }
            return new[] {newL1, newL2};
        }

        public void SetupMainMenu()
        {
            _downloadedItems = AppSession.DownloadedFractals;
            _availableItems = AppSession.AvailableFractals;
            _cachedWindows = AppData.Ref.Windows;
            StartCoroutine(SetupGodCanvas());
        }

        public T GetWindow<T>(string windowName) where T : Window
        {
            return _cachedWindows[windowName].GetComponent<T>();
        }

        public T GetWindow<T>(GameObject go) where T : Window
        {
            return go.GetComponent<T>();
        }

        public Window GetWindow(GameObject go)
        {
            return go.GetComponent<Window>();
        }

        public void CacheWindow(GameObject window, string name)
        {
            _cachedWindows[name] = window;
        }

        public bool IsCached(string windowName)
        {
            return _cachedWindows.ContainsKey(windowName);
        }

        public GameObject PopPreviousWindow()
        {
            return _windowStack.Pop();
        }

        public int WindowStackCount {
            get
            {
                return _windowStack.Count;
            }
        }

        public void EmptyStack()
        {
            _windowStack = new Stack<GameObject>();
        }

        public void SetupForVideoPlayback()
        {
            CurrentWindow.SetActive(false);
            GetWindow(CurrentWindow).DisableSiblings();
            CurrentWindow = null;
            _openNavigationWindow.SetActive(false);
            _openNavigationWindow = null;
            EmptyStack();
            _windowStack.Push(_cachedWindows[WindowNames.ViewerWindow]);
            DeactivateCanvases();
        }

        public void OverrideWindow()
        {
            CurrentWindow.SetActive(false);
            CurrentWindow = null;
        }

        public void ActivateCanvases()
        {
            AppData.Ref.GodCanvasController.gameObject.SetActive(true);
            AppData.Ref.NavigationCanvasController.gameObject.SetActive(true);
        }

        public void DeactivateCanvases()
        {
            AppData.Ref.GodCanvasController.gameObject.SetActive(false);
            AppData.Ref.NavigationCanvasController.gameObject.SetActive(false);
        }

        public void RepositionCanvases()
        {
            InputTracking.Recenter();
        }

        public GameObject CurrentWindow { get; private set; }

        public Window NewWindow { get; set; }
    }
}