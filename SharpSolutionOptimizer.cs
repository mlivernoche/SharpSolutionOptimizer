using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SharpSolutionOptimizer
{

    public abstract class Optimization<T> where T : ISolution
    {
        public delegate bool Constraint(T sol);

        [ThreadStatic]
        private static Random mutator;

        protected static Random Mutation => mutator ?? (mutator = new Random());

        public IEnumerable<T> CompletedSolutions { get; set; }
        public IEnumerable<Constraint> Constraints { get; protected set; }

        public abstract T CreateSolution();
        public abstract T GetBestSolution(IEnumerable<T> solutionlist);

        public Optimization()
        {

        }

        public Optimization(IEnumerable<Constraint> constraints)
        {
            Constraints = constraints;
        }

        /// <summary>
        /// Creates a specified amount of solutions.
        /// </summary>
        /// <param name="amount">The amount of solutions to create.</param>
        /// <returns>An enumerable object of the solutions.</returns>
        public IEnumerable<T> CreateMultipleSolutions(int amount)
        {
            var list = new List<T>(amount);

            for(int i = 0; i < amount; i++)
            {
                list.Add(CreateSolution());
            }

            return list;
        }

        /// <summary>
        /// Evaluates a solution subject to constraints and returns an enumerable object with the boolean value of each constraint.
        /// </summary>
        /// <param name="sol">The solution.</param>
        /// <returns>An enumerable object with the boolean value of each constraint</returns>
        public IEnumerable<bool> ValidateSolution(T sol)
        {
            return Constraints.Select(x => x(sol));
        }

        /// <summary>
        /// This will execute execute Task.Run, in which the tasks is to find the best solution in an IEnumerable of possible solutions. It will then return the best of those best.
        /// </summary>
        /// <param name="solutionspertask">The amount of solutions each task should find.</param>
        /// <param name="numoftasks">The amount of tasks to submit to the thread pool.</param>
        /// <returns>The best of the best of the best, sir! With honors.</returns>
        public async Task<T> RunParallelOptimization(int solutionspertask, int numoftasks)
        {
            var tasks = new Task<T>[numoftasks];

            for (int i = 0; i < numoftasks; i++)
            {
                tasks[i] = Task.Run(() => GetBestSolution(CreateMultipleSolutions(solutionspertask)));
            }

            await Task.WhenAll(tasks);
            return GetBestSolution(tasks.Select(x => x.Result));
        }
    }

    public interface ISolution
    {
        bool IsValid { get; }
        void Validate(IEnumerable<bool> constraints);
    }

    public interface ISolution<T> : ISolution
    {
        T Goal { get; }
    }
}
