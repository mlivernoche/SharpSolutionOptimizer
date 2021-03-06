using System;
using System.Collections.Concurrent;
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

            for (int i = 0; i < amount; i++)
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
        /// Creates an enumerable object of the best solution from multiple parallel runs of CreateMultipleSolutions and returns the best of that enumerable object. 
        /// </summary>
        /// <param name="solutionspertask">The amount of solutions each task should find.</param>
        /// <param name="numoftasks">The amount of tasks to submit to the thread pool.</param>
        /// <returns>The best of the best of the best, sir! With honors.</returns>
        public T GetBestSolutionInParallel(int solutionspertask, int numoftasks)
        {
            // Elements are added sequentially, so we can use an array and avoid having to cast this later.
            var tasks = new Action[numoftasks];

            // Not using a System.Collections.Concurrent collection (such as using T[] or List<T>) leads to problems. This solves it.
            var possibleSolutions = new ConcurrentBag<T>();

            for (int i = 0; i < numoftasks; i++)
            {
                tasks[i] = new Action(
                    () =>
                    {
                        var solution = GetBestSolution(CreateMultipleSolutions(solutionspertask));
                        possibleSolutions.Add(solution);
                    });
            }

            Parallel.Invoke(new ParallelOptions() { MaxDegreeOfParallelism = numoftasks }, tasks);
            return GetBestSolution(possibleSolutions);
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
