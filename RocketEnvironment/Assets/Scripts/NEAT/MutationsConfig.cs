using System;
using System.Collections.Generic;
using System.Linq;

namespace NEAT
{
    public struct MutationPick
    {
        public double Limit;
        public IMutator Mutator;
    }

    public class MutationConfig
    {
        private List<(float weight, IMutator mutator)> mutators = new List<(float weight, IMutator mutator)>();

        public MutationConfig AddMutator(float weight, IMutator mutator)
        {
            mutators.Add((weight, mutator));
            return this;
        }

        public List<MutationPick> GetMutationDistribution()
        {
            var exp = mutators.Select(x => (Math.Exp(x.weight), x.mutator));
            var sum = exp.Sum(x => x.Item1);
            var x = 0.0;
            var result = new List<MutationPick>();
            foreach (var e in exp)
            {
                x += e.Item1 / sum;
                result.Add(new MutationPick() { Limit = x, Mutator = e.mutator });
            }

            return result;
        }
    }
}