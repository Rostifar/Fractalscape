﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using Oculus.Platform;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

namespace Fractalscape
{   
    public class AppInitializer : MonoBehaviour
    {

        [SerializeField] private Camera _camera;
        [SerializeField] private GameObject _warningScreen;
        [SerializeField] private GameObject _entitlementErrorScreen;
        [SerializeField] private GameObject _introScreen;
        
        [SerializeField] private string _accessKey;
        [SerializeField] private string _secretKey;
        [SerializeField] private string _defaultBucket;
        
        [SerializeField] private bool _updateIsGreedy;

        private List<string> _embeddedExps;
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
            
            _embeddedExps = new List<string>{"Menger'sCrypt"};
            AppSession.FirstTimeUser = IsFirstTimeUser();
        }
        
        private void BeginOculusSetup()
        {
            Core.AsyncInitialize().OnComplete(message =>
            {
                if (message.IsError)
                {
                    Debug.Log("Oculus Sdk failed to initialize!");
                    OVRManager.instance.ReturnToLauncher();
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
            Assert.IsNotNull(message);
            if (message.IsError)
            {
                Debug.Log(message.GetError().Message);
                Debug.Log("Entitlement check failed! App closing.");
                StartCoroutine(OnEntitlementFailure());
            }
            else
            {
                Debug.Log("Entitlement check passed.");
                StartCoroutine(Init());
                StartThreads();
                LoadLogFiles();
            }
        }

        private void GreedyUpdate() //Free embedded video must remain installed in app. Otherwise i have to go
        {                            //through a ton of bs.
            var version = UnityEngine.Application.version;
            if (!AppSession.FirstTimeUser && PlayerPrefs.GetString(IoRes.KVersion) != version)
            {
                AppSession.UpdateAvailable = true;
                var text = Resources.Load<TextAsset>(IoRes.VersionLog);
                var update = JsonUtility.FromJson<AppUpdate>(text.text);
                AppSession.AppUpdate = new UsableUpdate {Update = update, UpdateType = AppUpdate.Type.Greedy};
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
            Debug.Log("SceneLoaded");
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
            AppSession.AvailableFractals = FractalLog.LoadLog(LogNames.AvailableFractals, FractalLog.OpLocation.Resources).Fractals;

            Debug.Log("Loading DownloadedFractals from playerprefs");
            AppSession.DownloadedFractals = FractalLog.LoadLog(LogNames.DownloadedFractals).Fractals;
            Debug.Log("Downloaded Fractal Count: " + AppSession.DownloadedFractals.Count);
            if (AppSession.FirstTimeUser || (!AppSession.FirstTimeUser && AppSession.DownloadedFractals.Count == 0))
            {
                AddPreinstalledExperiences();
            }
            Debug.Log("Downloaded Fractal Count: " + AppSession.DownloadedFractals.Count);

            IAP.GetViewerPurchases().OnComplete(message =>
            {
                if (message == null || message.IsError)
                {
                    Debug.Log("Player purchases could not be retrieved. Defaulting to local copy of purchases.");
                    AppSession.PurchasedFractals = PlayerPrefs.HasKey(LogNames.PurchasedFractals) 
                        ? FractalLog.LoadLog(LogNames.PurchasedFractals).Fractals
                        : new List<Fractal>();
                }
                else
                {
                    var list = new List<Fractal>();

                    for (var i = 0; i < message.Data.Count; i++)
                    {
                        var data = message.Data[i];
                        var fractal = new Fractal
                        {
                            Name = FractalLog.GetElementBySku(AppSession.AvailableFractals, data.Sku).Name,
                            Sku = data.Sku,
                            Type = 1
                        };
                        list.Add(fractal);
                    }
                    AppSession.PurchasedFractals = list;
                    Debug.Log("Purchased fractals length:" + list.Count);
                    PlayerPrefs.SetString(LogNames.PurchasedFractals, JsonUtility.ToJson(list));
                    PlayerPrefs.Save();
                }
            });
            _logFilesLoaded = true;
        }

        private void AddPreinstalledExperiences()
        {
            for (var i = 0; i < _embeddedExps.Count; i++)
            {
                var fractal = new Fractal {Name = _embeddedExps[i], Type = 0};
                AppSession.DownloadedFractals.Add(fractal);
            }
            var log = new FractalLog {Fractals = AppSession.DownloadedFractals};

            var str = JsonUtility.ToJson(log);
            PlayerPrefs.SetString(LogNames.DownloadedFractals, str);
            PlayerPrefs.Save();
        }

        private IEnumerator OnEntitlementFailure()
        {
            _entitlementErrorScreen.SetActive(true);
            yield return new WaitForSeconds(10f);
            UnityEngine.Application.Quit();
        }

        private void OnApplicationQuit()
        {
            PlayerPrefs.SetString(IoRes.KDownloadedFractals, JsonUtility.ToJson(AppSession.DownloadedFractals));
            PlayerPrefs.Save();
        }
    }
}
