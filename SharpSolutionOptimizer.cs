using System;
using System.Collections.Generic;

namespace SharpSolutionOptimizer
{
    public abstract class Optimization<T> where T : ISolution
    {
        public delegate bool Constraint(T sol);
        public static Random Mutation = new Random();

        public IList<T> CompletedGoals { get; } = new List<T>();
        public IList<Constraint> Constraints { get; } = new List<Constraint>();

        public abstract T CreateSolution();
        public abstract T GetBestSolution();

        public void Add(Constraint sol)
        {
            Constraints.Add(sol);
        }

        public IList<bool> ValidateSolution(T sol)
        {
            var validationlist = new List<bool>();

            foreach (var constraint in Constraints)
            {
                validationlist.Add(constraint(sol));
            }

            return validationlist;
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
