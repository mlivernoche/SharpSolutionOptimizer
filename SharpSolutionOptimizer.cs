using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpSolutionOptimizer
{
    public abstract class Optimization<T> where T : ISolution
    {
        public delegate bool Constraint(T sol);
        public static Random Mutation = new Random();

        public IList<T> CompletedSolutions { get; } = new List<T>();
        public IList<Constraint> Constraints { get; } = new List<Constraint>();

        public abstract T CreateSolution();
        public abstract T GetBestSolution();

        public void Add(Constraint con)
        {
            Constraints.Add(con);
        }

        public void Add(T sol)
        {
            CompletedSolutions.Add(sol);
        }

        public IList<bool> ValidateSolution(T sol)
        {
            return Constraints.Select(x => x(sol)).ToList();
        }
    }

    public interface ISolution
    {
        bool IsValid { get; }
        IList<bool> Constraints { get; set; }
    }

    public interface ISolution<T> : ISolution
    {
        T Goal { get; }
    }
}
