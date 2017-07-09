using System;
using System.Collections.Generic;
using UnityEngine;

namespace Fractalscape
{
    //Serializable class for lists of fractals (ie. downloaded, purchased, available, etc.)
    public class FractalLog
    {
        private static Queue<LogOperation> _queuedLogs = new Queue<LogOperation>();
        private const int MaxQueueSize = 3;

        public enum OpLocation
        {
            Resources,
            PlayerPrefs
        }

        public struct LogOperation
        {
            public string Name;
            public bool Urgent;
            public FractalLog Log;
        }
        
        public List<Fractal> Fractals;
        
        public static FractalLog LoadLog(string id, OpLocation location = OpLocation.PlayerPrefs)
        {
            switch (location)
            {
                case OpLocation.PlayerPrefs:
                    return PlayerPrefs.HasKey(id)
                        ? new FractalLog()
                        : JsonUtility.FromJson<FractalLog>(PlayerPrefs.GetString(id));
                case OpLocation.Resources:
                    return JsonUtility.FromJson<FractalLog>(Resources.Load<TextAsset>(id).text);
            }
            return null;
        }

        public static void FlushQueuedLogs()
        {
            foreach (var log in _queuedLogs)
            {
                PlayerPrefs.GetString(log.Name, JsonUtility.ToJson(log.Log));
            }
            PlayerPrefs.Save();
        }

        public static void SaveLog(LogOperation log)
        {
            _queuedLogs.Enqueue(log);
            if (log.Urgent || _queuedLogs.Count >= MaxQueueSize)
            {
                FlushQueuedLogs();
            }
        }

        public static bool Contains(List<Fractal> list, string s)
        {
            foreach (var element in list)
            {
                if (element.Name == s) return true;
            }
            return false;
        }

        public static Fractal GetElementBySku(List<Fractal> list, string sku)
        {
            foreach (var fractal in list)
            {
                if (sku == fractal.Sku) return fractal;
            }
            return null;
        }
    }
}