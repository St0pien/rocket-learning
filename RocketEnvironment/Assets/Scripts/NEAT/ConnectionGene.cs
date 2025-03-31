using System;

namespace NEAT
{
    public enum ConnectionStatus
    {
        Enabled,
        Disabled
    }

    [Serializable]
    public struct Connection
    {
        public int Input;
        public int Output;

        public Connection(int input, int output)
        {
            Input = input;
            Output = output;
        }
    }

    [Serializable]
    public class ConnectionGene
    {
        public Connection Connection;
        public float Weight = 0f;
        public ConnectionStatus Status = ConnectionStatus.Enabled;
        public int Innovation = 0;

        public ConnectionGene(int input, int output, int innovation = 0)
        {
            Connection = new Connection(input, output);
            Innovation = innovation;
        }

        public ConnectionGene(Connection connection, int innovation = 0)
        {
            Connection = connection;
            Innovation = innovation;
        }

        public ConnectionGene Clone()
        {
            return new ConnectionGene(Connection, Innovation) { Status = Status, Weight = Weight };
        }
    }
}