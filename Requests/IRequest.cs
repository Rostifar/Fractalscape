using System;
using UnityEngine;

namespace Fractalscape
{
    public interface IRequest
    {
        void Trigger(Action<bool, IRequest> callback);
        void FinalizeRequest(bool success);
        string Status();
        bool IsRunning();
        void Update();
    }
}