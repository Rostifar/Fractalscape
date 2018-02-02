using JetBrains.Annotations;

namespace Fractalscape
{
    //Serializable class which represents a single app update.

    public struct UsableUpdate
    {
        public AppUpdate Update;
        public AppUpdate.Type UpdateType;
    }

    public class AppUpdate
    {
        public string Message;
        public string Image;
        public string Header;        

        public enum Type
        {
            Greedy, 
            Normal
        }
    }
}