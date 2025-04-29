using System;

namespace NEAT
{
    [Serializable]
    public enum ConnectionStatus
    {
        Enabled,
        Disabled
    }

    [Serializable]
    public class ConnectionGene
    {
        public int Id;
        public Connection Connection;
        public float Weight = 0f;
        public ConnectionStatus Status = ConnectionStatus.Enabled;

        public ConnectionGene(int id, Connection conn)
        {
            Id = id;
            Connection = conn;
        }

        public ConnectionGene Clone()
        {
            return new ConnectionGene(Id, Connection)
            {
                Weight = Weight,
                Status = Status
            };
        }
    }
}