using System;
using System.IO;
using Ionic.Zip;
using UnityEngine;

namespace Fractalscape
{
    public sealed class StorageUtils
    {
        public bool UnzipFinished;
        public bool Error;
        public ReadProgressEventArgs ReadProgressEventArgs;

        public static int FreeStorage(bool external)
        {
            var jc = new AndroidJavaClass("com.Torus.Fractalscape.utils.DiskUtils");

            return jc.CallStatic<int>("freeSpace", external);
        }

        public static int TotalStorage(bool external)
        {
            var androidJc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            var jo = androidJc.GetStatic<AndroidJavaObject>("currentActivity");
            var jc = new AndroidJavaClass("com.Torus.Fractalscape.utils.DiskUtils");
            return jc.CallStatic<int>("totalSpace", jo, external);
        }

        public static bool SpaceAvailable(bool external, int objectSize)
        {
            return FreeStorage(external) - objectSize >= 0;
        }

        public void UnzipPackage(string file, string outputDir)
        {
            try
            {
                ExpansiveSearch(outputDir, outputDir);

                using (var zip = ZipFile.Read(file, new ReadOptions()))
                {
                    foreach (var etity in zip)
                    {
                        zip.ReadProgress += (sender, args) => ReadProgressEventArgs = args;
                        etity.Extract(outputDir);
                    }
                }
                UnzipFinished = true;
                File.Delete(file);
            }
            catch (Exception e)
            {
                Error = true;
                UnzipFinished = true;
            }
        }

        public static void CopyTo(Stream input, Stream output)
        {
            byte[] buffer = new byte[16 * 1024]; // Fairly arbitrary size
            int bytesRead;

            while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, bytesRead);
            }
        }

        public static void ExpansiveSearch(string file, string dir)
        {
            if (File.Exists(file))
            {
                File.Delete(file);
                return;
            }
            if (Directory.Exists(dir))
            {
                Directory.Delete(dir, true);
            }
        }
    }
}