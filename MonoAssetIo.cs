using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Oculus.Platform;
using UnityEngine;
using Application = UnityEngine.Application;
using Object = UnityEngine.Object;

namespace Fractalscape
{
    public class MonoAssetIo : MonoBehaviour
    {
        public static string DefaultPath;
        public bool Success;
        public bool ProcessFinished;
        public string Message;
        private static MonoAssetIo _instance;

        public static MonoAssetIo Instance
        {
            get {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<MonoAssetIo>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("MonoAssetIo");
                        DontDestroyOnLoad(go);
                        _instance = go.AddComponent<MonoAssetIo>();
                    }
                }
                return _instance;
            }
        }

        private void Awake()
        {
            DefaultPath = Application.persistentDataPath;
        }

        public void UninstallExperience(string expName)
        {
            try
            {
                Directory.Delete(Path.Combine(DefaultPath, expName), true);
                Success = true;
                ProcessFinished = true;
            }
            catch (Exception e)
            {
                Message = e.Message;
                Success = false;
                ProcessFinished = true;
            }
        }


        public static void SaveToPlayerPrefs (string key, string value)
        {
            var obj = PlayerPrefs.GetString(key);
            List<string> objList;

            if (obj != "")
            {
                objList = JsonUtility.FromJson<FractalLog>(obj).Fractals;
                objList.Add(value);
            }
            else
            {
                objList = new List<string> {value};
            }
            PlayerPrefs.SetString(key, JsonUtility.ToJson(objList));
            PlayerPrefs.Save();
        }

        public static void DeleteFromPlayerPrefs(string key, string value) //After uninstall
        {
            var json = JsonUtility.FromJson<FractalLog>(PlayerPrefs.GetString(key));
            json.Fractals.Remove(value);
            PlayerPrefs.SetString(key, JsonUtility.ToJson(json));
        }
    }
}