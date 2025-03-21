using UnityEngine;

namespace NEAT
{
    public class NewNodeMutator : IMutator
    {
        private IdGetter getNodeId;
        private IdGetter getInnovationNumber;

        public NewNodeMutator(IdGetter idGetter, IdGetter innovationGetter)
        {
            getNodeId = idGetter;
            getInnovationNumber = innovationGetter;
        }

        public void Mutate(Genome genome)
        {
            var pickedConnection = genome.GetRandomActiveConnectionGene();
            pickedConnection.Status = ConnectionStatus.Disabled;
            var newNode = new NodeGene(getNodeId(pickedConnection.Connection), NodeType.Hidden);
            genome.NodeGenes.Add(newNode);
            var connIn = new Connection(pickedConnection.Connection.Input, newNode.Id);
            var connOut = new Connection(newNode.Id, pickedConnection.Connection.Output);

            genome.ConnectionGenes.Add(new ConnectionGene(connIn, getInnovationNumber(connIn)) { Weight = 1 });
            genome.ConnectionGenes.Add(new ConnectionGene(connOut, getInnovationNumber(connOut)) { Weight = pickedConnection.Weight });

            Debug.Log($"New Node mutation: genome {genome.Id} between {pickedConnection.Connection.Input} and {pickedConnection.Connection.Output} node {newNode.Id}");
        }
    }
}