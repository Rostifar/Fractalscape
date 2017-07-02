using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Amazon;
using Amazon.Runtime;
using UnityEngine;
using Amazon.S3;
using Amazon.S3.Model;

namespace Fractalscape
{
    public struct AwsResponseData
    {
        public bool Error;
        public HttpStatusCode ResCode;
        public MetadataCollection MetaData;
    }

    public sealed class FractalscapeWebClient
    {
        private const string DefaultBucket = "fractalscape";
        private readonly string _defaultPath = Application.persistentDataPath;
        private static readonly RegionEndpoint DefaultEndpoint = RegionEndpoint.USEast1;
        public WriteObjectProgressArgs ProgressArgs { private set; get; }
        public AwsResponseData CurrentDataMeta;
        public AwsResponseData CurrentDataDownload;
        public bool MetaDataDownloadFinished = false;
        public bool DownloadFinished = false;

        public void GetObject(string key, string bucket = DefaultBucket, string path = "")
        {
            path = path == "" ? Path.Combine(_defaultPath, key) : path;
            var dir = Path.Combine(path, Path.GetFileNameWithoutExtension(key));

            using (var client = new AmazonS3Client(AppSession.AccessKey, AppSession.SecretKey, DefaultEndpoint))
            {
                ServicePointManager.ServerCertificateValidationCallback = MyRemoteCertificateValidationCallback;
                try
                {
                    using (var res = client.GetObject(new GetObjectRequest {BucketName = bucket, Key = key}))
                    {
                        StorageUtils.ExpansiveSearch(dir, path);
                        res.WriteObjectProgressEvent += (sender, args) => ProgressArgs = args;
                        res.WriteResponseStreamToFile(path == "" ? Path.Combine(_defaultPath, key) : path);
                        CurrentDataDownload = new AwsResponseData
                        {
                            Error = false,
                            MetaData = res.Metadata,
                            ResCode = res.HttpStatusCode
                        };
                    }
                    DownloadFinished = true;
                }
                catch (AmazonServiceException e)
                {
                    CurrentDataDownload = new AwsResponseData
                    {
                        Error = true,
                        MetaData = null,
                        ResCode = e.StatusCode
                    };
                    DownloadFinished = true;
                }
            }
        }


        public void GetObjectMetadata(string key, string bucket = DefaultBucket)
        {
            try
            {
                using (var client = new AmazonS3Client(AppSession.AccessKey, AppSession.SecretKey, DefaultEndpoint))
                {
                    var req = new GetObjectMetadataRequest {BucketName = bucket, Key = key};
                    ServicePointManager.ServerCertificateValidationCallback = MyRemoteCertificateValidationCallback;
                    var res = client.GetObjectMetadata(req);

                    CurrentDataMeta = new AwsResponseData
                    {
                        Error = false,
                        MetaData = res.Metadata,
                        ResCode = res.HttpStatusCode
                    };
                    MetaDataDownloadFinished = true;
                }
            }
            catch (AmazonServiceException e)
            {
                CurrentDataMeta = new AwsResponseData
                {
                    Error = true,
                    MetaData = null,
                    ResCode = e.StatusCode
                };
                MetaDataDownloadFinished = true;
            }
        }

        private static bool MyRemoteCertificateValidationCallback(System.Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) {
            var isOk = true;
            // If there are errors in the certificate chain, look at each error to determine the cause.
            if (sslPolicyErrors == SslPolicyErrors.None) return true;
            for (var i=0; i<chain.ChainStatus.Length; i++) {
                if (chain.ChainStatus[i].Status == X509ChainStatusFlags.RevocationStatusUnknown) continue;
                chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
                chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan (0, 1, 0);
                chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
                var chainIsValid = chain.Build ((X509Certificate2)certificate);
                if (!chainIsValid) {
                    isOk = false;
                }
            }
            return isOk;
        }
    }
}