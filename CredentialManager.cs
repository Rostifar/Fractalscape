using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Net;
using System.Threading;
using Amazon.Runtime;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;
using Oculus.Platform;
using Oculus.Platform.Models;
using RenderHeads.Media.AVProVideo;
using UnityEngine;

namespace Fractalscape
{
    public class CredentialManager
    {
        /*
        private Credentials _credentials;
        private HttpStatusCode _statusCode;

        public IEnumerator GenerateTemporaryCredentials(Action<bool, Credentials> callback)
        {
            var client = new AmazonSecurityTokenServiceClient();
            var request = new AssumeRoleRequest {DurationSeconds = 4000, RoleArn = "arn:aws:iam::772173841962:role/FractalscapeUser"};
            var job = new Job(new Thread(delegate()
            {
                var response = client.AssumeRole(request);
                _credentials = response.Credentials;
                _statusCode = response.HttpStatusCode;
            }));
            yield return RequestProcessor.Instance.StartCoroutine(job.Start());

            callback(_credentials != null && _statusCode == HttpStatusCode.Accepted
                     || _statusCode == HttpStatusCode.Created
                     || _statusCode == HttpStatusCode.OK, _credentials);
        }

        public void ThreadedGenerateTemporaryCredentials(Action<bool, AssumeRoleResponse> callback)
        {
            var client = new AmazonSecurityTokenServiceClient();
            var request = new AssumeRoleRequest {DurationSeconds = 4000, RoleArn = "arn:aws:iam::772173841962:role/FractalscapeUser"};
            var webRequest = new AssumeRoleWithWebIdentityRequest();
            var response = client.AssumeRoleWithWebIdentity();
            _credentials = response.Credentials;
            _statusCode = response.HttpStatusCode;


            callback(_credentials != null && _statusCode == HttpStatusCode.Accepted
                     || _statusCode == HttpStatusCode.Created
                     || _statusCode == HttpStatusCode.OK, response);
        }

        private static bool CredentialsValid(Credentials credentials)
        {
            return credentials != null && credentials.Expiration > DateTime.Now;
        }

        public static Credentials ThreadedManageCredentials(Credentials credentials)
        {
            if (!CredentialsValid(credentials))
            {
                new CredentialManager().ThreadedGenerateTemporaryCredentials((success, response) =>
                {
                    if (!success)
                    {
                        throw new AmazonServiceException {StatusCode = response.HttpStatusCode};
                    }
                    credentials = response.Credentials;
                });
            }
            return credentials;
        }

        public Credentials ManageCredentials(Credentials credentials)
        {
            if (!CredentialsValid(credentials))
            {
                RequestProcessor.Instance.StartCoroutine(GenerateTemporaryCredentials((success, response) =>
                {
                }));
            }
            return credentials;
        }
*/
    }
}