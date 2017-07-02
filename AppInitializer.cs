using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Oculus.Platform;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fractalscape
{   
    /***TODO
        
        *Correctly set-iap.    []
        *Embed free experience.[]
    ***/
    
    //Entry point to app. Checks entitlements, opens json files, and looks for app updates.
    //Then merges the loading scene with the main menu. 
    public class AppInitializer : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private GameObject _warningScreen;
        [SerializeField] private GameObject _introScreen;
        
        [SerializeField] private string _accessKey;
        [SerializeField] private string _secretKey;
        [SerializeField] private string _defaultBucket;
        [SerializeField] private List<string> _embeddedExps;
        [SerializeField] private bool _updateIsGreedy;

        private bool _sceneLoadingStarted;
        private bool _updateInfoRequestFinished;
        private bool _logFilesLoaded;
        private List<GameObject> _preloadWindows;
        
        private void Awake()
        {
            Debug.Log("Starting app.");
            DontDestroyOnLoad(gameObject);
            DontDestroyOnLoad(_camera);

            AppSession.AccessKey = _accessKey;
            AppSession.SecretKey = _secretKey;
            AppSession.AppCamera = _camera;
            AppSession.DefaultBucket = _defaultBucket;
            
            //entitlements checks.
            Debug.Log("Setting up Oculus Sdk.");
            BeginOculusSetup();

            AppSession.FirstTimeUser = IsFirstTimeUser();
        }

        private void Start()
        {
            StartCoroutine(Init());
        }
        
        private void BeginOculusSetup()
        {
            Core.AsyncInitialize().OnComplete(message =>
            {
                if (message.IsError)
                {
                    Debug.Log("Oculus Sdk failed to initialize!");
                    OVRManager.PlatformUIGlobalMenu();
                }
                else
                {
                    Debug.Log("Oculus Sdk initialized.");
                    Entitlements.IsUserEntitledToApplication().OnComplete(EntitlementCallback);
                }
            });
        }

        private void EntitlementCallback(Message message)
        {
            if (message.IsError)
            {
                Debug.Log("Entitlement check failed! App closing.");
                OnEntitlementFailure();
            }
            else
            {
                Debug.Log("Entitlement check passed.");
                StartThreads();
                LoadLogFiles();
            }
        }

        private void GreedyUpdate() //Free embedded video must remain installed in app. Otherwise i have to go
        {                            //through a ton of bs.
            var version = UnityEngine.Application.version;
            if (AppSession.FirstTimeUser || PlayerPrefs.GetString(IoRes.KVersion) != version)
            {
                AppSession.UpdateAvailable = true;
                var text = Resources.Load<TextAsset>(IoRes.VersionLog);
                AppSession.AppUpdate = JsonUtility.FromJson<AppUpdate>(text.text);
                PlayerPrefs.SetString(IoRes.KVersion, version);
                PlayerPrefs.Save();
            }
            _updateInfoRequestFinished = true;
        }
        
        private void RegularUpdate() //expensive.
        {
            if (!AppSession.FirstTimeUser)
            {
                RequestProcessor.Instance.MakeRequest(new AppUpdateRequest(), (update, req) =>
                {
                    req.FinalizeRequest(update);
                    _updateInfoRequestFinished = true;
                });
            }
            else
            {
                _updateInfoRequestFinished = true;
            }
        }

        private bool IsFirstTimeUser()
        {
            if (!PlayerPrefs.HasKey(IoRes.KNewUser))
            {
                PlayerPrefs.SetInt(IoRes.KNewUser, 1);
                PlayerPrefs.Save();
                return true;
            }
            return false;
        }

        private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            _camera.cullingMask = 1 << 12;
            WindowManager.Instance.SetupMainMenu();
            enabled = false;
        }

        private void Update()
        {
            if (!_sceneLoadingStarted && _logFilesLoaded && _updateInfoRequestFinished)
            {
                _sceneLoadingStarted = true;
                SceneManager.LoadSceneAsync("FractalScape", LoadSceneMode.Single);
            }
        }

        private IEnumerator Init()
        {
            if (AppSession.FirstTimeUser)
            {
                _warningScreen.SetActive(true);
                yield return new WaitForSeconds(8f);
                _warningScreen.SetActive(false);
            }
            _introScreen.SetActive(true);
        }

        private void StartThreads()
        {
            AppSession.IsConnectedToInternet = NetworkUtils.IsConnected();
            
            if (AppSession.IsConnectedToInternet && !_updateIsGreedy) RegularUpdate();
            else GreedyUpdate();
            
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void LoadLogFiles()
        {
            Debug.Log("Loading AvailableFractals.json");
            var availableList = Resources.Load<TextAsset>(LogNames.AvailableFractals);
            AppSession.AvailableFractals = JsonUtility.FromJson<FractalLog>(availableList.text).Fractals;

            if (AppSession.FirstTimeUser) AddPreinstalledExperiences();
            else
            {
                var str = PlayerPrefs.GetString(LogNames.DownloadedFractals);
                AppSession.DownloadedFractals = str == ""
                    ? new List<string>()
                    : JsonUtility.FromJson<FractalLog>(str).Fractals;
            }
            _logFilesLoaded = true;

            IAP.GetViewerPurchases().OnComplete(message =>
            {
                if (message.IsError && PlayerPrefs.HasKey(LogNames.PurchasedFractals))
                {
                    AppSession.PurchasedFractals = JsonUtility.FromJson<FractalLog>(PlayerPrefs.GetString(LogNames.PurchasedFractals))
                        .Fractals;
                    AppSession.OculusDown = true;
                }
                else
                {
                    //AppSession.PurchasedFractals = PurchaseRequest.ListToFractalLog(message.GetPurchaseList()).Fractals;
                }
            });
        }

        private void AddPreinstalledExperiences()
        {
            for (var i = 0; i < _embeddedExps.Count; i++)
            {
                AppSession.DownloadedFractals.Add(_embeddedExps[i]);
            }
            var log = new FractalLog();
            log.Fractals = AppSession.DownloadedFractals;
            
            var str = JsonUtility.ToJson(log);
            PlayerPrefs.SetString(LogNames.DownloadedFractals, str);
            PlayerPrefs.Save();
        }

        private void OnEntitlementFailure()
        {
            OVRManager.instance.ReturnToLauncher();
        }

        private void OnApplicationQuit()
        {
            PlayerPrefs.SetString(IoRes.KDownloadedFractals, JsonUtility.ToJson(AppSession.DownloadedFractals));
            PlayerPrefs.Save();
        }
    }
}