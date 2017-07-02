using System;
using System.Collections.Generic;
using System.Runtime.Remoting;
using UnityEngine.UI;

namespace Fractalscape
{
    public struct ProgressMessage
    {
        public string Header;
        public string Body;
    }

    public class RequestData
    {
        private List<ProgressMessage> _messages;
        private int _indx;

        public enum DataType
        {
            Background,
            Instant
        }

        public RequestData(ProgressMessage message, string id, DataType type = DataType.Background)
        {
            Message = message;
            Id = id;
            Type = type;
        }

        public RequestData(ProgressMessage message, DataType type = DataType.Background)
        {
            Message = message;
            Id = Guid.NewGuid().ToString("N");
            Type = type;
        }

        public RequestData(List<ProgressMessage> messages, DataType type = DataType.Background)
        {
            _messages = new List<ProgressMessage>();
            _messages = messages;
            Id = Guid.NewGuid().ToString("N");
            Type = type;
        }

        public void ActivateMessage(Text uiHeader, Text uiBody)
        {
            uiBody.text = Message.Body;
            uiHeader.text = Message.Header;
        }

        public void ActivateNewMessage(Text uiHeader, Text uiBody)
        {
            if (_messages != null && _indx < _messages.Count - 1)
            {
                var newMsg = _messages[++_indx];
                Message = newMsg;
                uiHeader.text = newMsg.Header;
                uiBody.text = newMsg.Body;
            }
        }

        public void ActivateNewMessage()
        {
            if (_messages != null && _indx < _messages.Count - 1)
            {
                var newMsg = _messages[++_indx];
                Message = newMsg;
            }
        }

        public static List<ProgressMessage> ConstructMessages(List<string> headers, List<string> bodies)
        {
            var messages = new List<ProgressMessage>();
            if (headers.Count != bodies.Count) throw new Exception("YOU MUST HAVE AN EQUAL NUMBER OF HEADERS AND BODIES");
            for (var i = 0; i < headers.Count; i++)
            {
                messages.Add(new ProgressMessage
                {
                    Header = headers[i],
                    Body = bodies[i]
                });
            }
            return messages;
        }

        public ProgressMessage Message { get; set; }
        public string Id { get; set; }
        public DataType Type { get; }
    }
}