namespace NEAT
{
    public delegate int IdGetter(Connection connection);

    public interface IMutator
    {
        /// <summary>
        ///     Applies mutation to the genome
        /// </summary>
        /// <returns>new introduced connection genes</returns>
        public void Mutate(Genome genome);
    }
}