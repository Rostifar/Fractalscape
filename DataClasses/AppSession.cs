using System.Collections.Generic;
using Amazon.SecurityToken.Model;
using Oculus.Platform.Models;
using UnityEngine;

namespace Fractalscape
{
    //Local reference holder, per app launch.
    public sealed class AppSession
    {
        private static AppSession _instance;
        public static bool IsConnectedToInternet { get; set; }
        public static List<Fractal> AvailableFractals { get; set; }
        public static List<Fractal> DownloadedFractals { get; set; }
        public static List<Fractal> PurchasedFractals { get; set; }

        public Recticle Recticle { get; set; }
        public FractalscapeWebClient WebClient { get; set; }
        public static BackButtonBehavior CurrentBackButtonBehavior;

        public static bool InViewer = false;
        public static bool UpdateAvailable;
        public static bool FirstTimeUser = false;
        public static bool AppInitialized = false;

        public static MenuItem SelectedItem { get; set; }
        public static Camera AppCamera { get; set; }
        public static Skybox Skybox { get; set; }

        public static AppUpdate AppUpdate { get; set; }

        public static string AccessKey { get; set; }
        public static string SecretKey { get; set; }
        public static string DefaultBucket { get; set; }

        public static bool OculusDown;
        public static bool OnMenu;

        public enum BackButtonBehavior
        {
            ExitApp,
            Revert
        }

        public static AppSession Instance {
            get
            {
                if (_instance == null) _instance = new AppSession();
                return _instance;
            }
        }
    }
}