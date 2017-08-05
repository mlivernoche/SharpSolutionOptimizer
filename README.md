# SharpSolutionOptimizer
C# repository for linear/nonlinear programming, optimization, modeling, etc.

# Optimization\<T\>

Optimization\<T\> is an abstract class that allows one to implement a simple optimization model. A user creates a list of constraints using the Constraint(T sol) delegate. The user can then create solutions that are generated with the System.Random class. These solutions can then be evaluated as either IsValid = true (i.e., this solution satisfies the constraints) or IsValid = false (i.e., this solution does not satisfy the constraints).

In order to utitlize this, the user must create two classes:
* A producer class (i.e., ProfitMaximization) that inherits Optimization\<T\>
* An output class (i.e., Solution) that inherits ISolution\<T2\>

All generics T must implement the interface ISolution, whereas all generics T2 can be any type (although preferrably a primitive type).

Let's start with a simple scenario. A factory wants to maximize their profits for the next month. They produce two products, which we will simply call X1 and X2. They can produce whatever mixture they want, but they can only produce 200 units of them in one month. They also have labor and resource constraints (we'll assume that those have been allocated already). ProfitMaximization might start like this,

```
public class ProfitMaximization : Optimization<Solution>
{
    public const int Minimum = 0;
    public const int Maximum = 174;

    public ProfitMaximization()
    {
        // We can make whatever mixture we want, but we have a maximum producable amount of 200 units.
        Add(x => x.X1 + x.X2 <= 200);
        
        // Let's say these are the man hours available.
        Add(x => (9 * x.X1) + (6 * x.X2) <= 1566);
        
        // Let's say this is the resources available.
        Add(x => (12 * x.X1) + (16 * x.X2) <= 2880);
    }
}
```

The ProfitMaximization implements a method from Optimization\<T\> called CreateSolution, which is an T factory that produces instances of the output class. As CreateSolution is an abstract method, the implementation does not matter, but in this example output classes are created pseudo-randomly with the System.Random class. (If you are wondering why Maximum = 174: that was deduced from the other two constraints. If they decide to only produce X1, they can only produce 174 of them; this is the lowest amount that can be produced by maximizing the man hours [for resources, that is 180]). CreateSolution can look like this:

```
public override Solution CreateSolution()
{
    // Create a solution.
    var solution = new Solution(Mutation.Next(Minimum, Maximum), Mutation.Next(Minimum, Maximum));
    
    // Check whether or not the solution is valid.
    solution.Constraints = ValidateSolution(solution);
    
    // Add it to CompletedSolutions.
    Add(solution);

    return solution;
}
```

These output classes are then added to the CompletedSolutions property of ProfitMaximization. Each entry has the solution and whether or not it is a valid solution. Once the solutions are created, once can find the best valid solution available.

In order to find the best solution, the user must implement the abstract method GetBestSolution from Optimization\<T\>. Implementation does not matter; however, what does matter, is that this function must return the best solution among the ones created. This depends on what the user is aiming to accomplish. For this example, this method would find the solution with the highest profit. It would look something like this:

```
public override Solution GetBestSolution()
{
    return CompletedSolutions.Where(x => x.IsValid).OrderByDescending(x => x.Goal).FirstOrDefault();
}
```

So long as CompletedSolutions.Count > 0, then this method will always return a solution. The final class would look like this,

```
public class ProfitMaximization : Optimization<Solution>
{
    public const int Minimum = 0;
    public const int Maximum = 174;

    public ProfitMaximization()
    {
        // We can make whatever mixture we want, but we have a maximum producable amount of 200 units.
        Add(x => x.X1 + x.X2 <= 200);
        
        // Let's say these are the man hours available.
        Add(x => (9 * x.X1) + (6 * x.X2) <= 1566);
        
        // Let's say this is the resources available.
        Add(x => (12 * x.X1) + (16 * x.X2) <= 2880);
    }

    public override Solution CreateSolution()
    {
        // Create a solution.
        var solution = new Solution(Mutation.Next(Minimum, Maximum), Mutation.Next(Minimum, Maximum));
        
        // Check whether or not the solution is valid.
        solution.Constraints = ValidateSolution(solution);
        
        // Add it to CompletedSolutions.
        Add(solution);

        return solution;
    }

    public override Solution GetBestSolution()
    {
        return CompletedSolutions.Where(x => x.IsValid).OrderByDescending(x => x.Goal).FirstOrDefault();
    }
}
```

Having all of that, what does the Solution class look like? It could look like this:

```
public class Solution : ISolution<int>
{
    public int X1 { get; }
    public int X2 { get; }
    public int Goal => (350 * X1) + (300 * X2);
    public IList<bool> Constraints { get; set; }
    public bool IsValid => !Constraints.Any(x => x == false);

    public Solution(int x1, int x2)
    {
        X1 = x1;
        X2 = x2;
    }
}
```

The profits produced by each solution is determined by the int Goal property. This class is intended to be immutable, but right now that is not possible; the IList\<bool\> property needs to be created after the class is constructed. As it stands, it simply stores the bool values of each constraint, rather than the delegate itself, in order to save time on exectuing the delegates.

This is it for the model. To use it is quite simple:

```
public class Program
{
    static void Main(string[] args)
    {
        // Create the model.
        var testOptimization = new ProfitMaximization();

        // Run it 100,000 times.
        for (int i = 0; i < 100_000; i++)
        {
            testOptimization.CreateSolution();
        }

        // Get the best solution.
        var solution = testOptimization.GetBestSolution();
        
        // Output the solution to the console.
        var outputstring = new StringBuilder();
        outputstring.AppendLine("Solution:");
        outputstring.AppendFormat("X1 = {0}, X2 = {1}, Profit = {2}\n", solution.X1, solution.X2, solution.Goal);

        outputstring.AppendLine("Is Valid Solution: " + solution.IsValid);

        foreach (var constraint in testOptimization.Constraints)
        {
            outputstring.AppendLine("Constraint: " + constraint(solution).ToString());
        }

        Console.WriteLine(outputstring.ToString());
    }
}
```

The answer should be,

```
Solution:
X1 = 122, X2 = 78, Profit = 66100
Is Valid Solution: True
Constraint: True
Constraint: True
Constraint: True
```

Because this answer was produced through 100,000 pseudo-random solutions, your answer may be slightly off.
