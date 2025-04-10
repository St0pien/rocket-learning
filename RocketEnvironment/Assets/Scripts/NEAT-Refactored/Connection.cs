using System;

namespace NEAT2
{
    [Serializable]
    public struct Connection
    {
        public int Input;
        public int Output;

        public Connection(int i, int o)
        {
            Input = i;
            Output = o;
        }
    }
}