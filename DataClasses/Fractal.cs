using System;

namespace Fractalscape
{
    [Serializable]
    public class Fractal
    {
        public string Name;
        public string Sku = "-";
        public int Type; //0 is free, 1 is paid
    }
}