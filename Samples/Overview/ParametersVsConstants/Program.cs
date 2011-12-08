using System;
using System.Diagnostics;

namespace NQuery.Samples.ParametersVsConstants
{
	internal static class Program
	{

		private static void ConstantScenario()
		{
			#region Constant Scenario

			const int numberOfRuns = 1000000;

			Console.WriteLine("Using constants and parameters in scenario where the value never changes.");
			Console.WriteLine("Executing scenario {0:N0} times.", numberOfRuns);

			int numberOfCompilations = 0;
			Stopwatch stopwatch;

			// Run with constants

			Console.WriteLine("Constants");
			Expression<string> expressionUsingConstant = new Expression<string>();
			expressionUsingConstant.DataContext.Constants.Add("MagicValue", 42);
			expressionUsingConstant.Text = "TO_STRING(MagicValue * 42 / 1000.0) + ' the result'";
			expressionUsingConstant.CompilationSucceeded += delegate { numberOfCompilations++; };

			numberOfCompilations = 0;
			stopwatch = Stopwatch.StartNew();
			for (int i = 0; i < numberOfRuns; i++)
				expressionUsingConstant.Evaluate();
			
			Console.WriteLine("\tNeeded Time  : {0:N0} msecs", stopwatch.ElapsedMilliseconds);
			Console.WriteLine("\tCompilations : {0:N0}", numberOfCompilations);

			// Run with parameters

			Console.WriteLine("Parameters");
			Expression<string> expressionUsingParameters = new Expression<string>();
			expressionUsingParameters.Parameters.Add("MagicValue", typeof(int), 42);
			expressionUsingParameters.Text = "TO_STRING(MagicValue * 42 / 1000.0) + ' the result'";
			expressionUsingParameters.CompilationSucceeded += delegate { numberOfCompilations++; };

			numberOfCompilations = 0;
			stopwatch = Stopwatch.StartNew();
			for (int i = 0; i < numberOfRuns; i++)
				expressionUsingParameters.Evaluate();

			Console.WriteLine("\tNeeded Time  : {0:N0} msecs", stopwatch.ElapsedMilliseconds);
			Console.WriteLine("\tCompilations : {0:N0}", numberOfCompilations);
			
			#endregion
		}

		private static void ParameterScenario()
		{
			#region Parameter Scenario

			const int numberOfRuns = 10000;

			Console.WriteLine("Using constants and parameters in scenario where the value frequently changes.");
			Console.WriteLine("Executing scenario {0:N0} times.", numberOfRuns);

			int numberOfCompilations = 0;
			Stopwatch stopwatch;

			// Testing with constants

			Console.WriteLine("Constants");
			Expression<string> expressionUsingConstant = new Expression<string>();
			expressionUsingConstant.Text = "TO_STRING(MagicValue * 42 / 1000.0) + ' the result'";
			expressionUsingConstant.CompilationSucceeded += delegate { numberOfCompilations++; };

			numberOfCompilations = 0;
			stopwatch = Stopwatch.StartNew();
			for (int i = 0; i < numberOfRuns; i++)
			{
				expressionUsingConstant.DataContext.Constants.Add("MagicValue", i);
				expressionUsingConstant.Evaluate();
				expressionUsingConstant.DataContext.Constants.Remove("MagicValue");
			}
			Console.WriteLine("\tNeeded Time  : {0:N0} msecs", stopwatch.ElapsedMilliseconds);
			Console.WriteLine("\tCompilations : {0:N0}", numberOfCompilations);

			// Testing with parameters

			Console.WriteLine("Parameters");
			Expression<string> expressionUsingParameters = new Expression<string>();
			expressionUsingParameters.Parameters.Add("MagicValue", typeof(int));
			expressionUsingParameters.Text = "TO_STRING(MagicValue * 42 / 1000.0) + ' the result'";
			expressionUsingParameters.CompilationSucceeded += delegate { numberOfCompilations++; };

			numberOfCompilations = 0;
			stopwatch = Stopwatch.StartNew();
			for (int i = 0; i < numberOfRuns; i++)
			{
				expressionUsingParameters.Parameters["MagicValue"].Value = i;
				expressionUsingParameters.Evaluate();
			}
			Console.WriteLine("\tNeeded Time  : {0:N0} msecs", stopwatch.ElapsedMilliseconds);
			Console.WriteLine("\tCompilations : {0:N0}", numberOfCompilations);

			#endregion
		}

		private static void Main()
		{
			// Ensure internal caches are initialized.
			new Expression<int>("42 * 2").Evaluate();

			ConstantScenario();
			Console.WriteLine();
			Console.WriteLine();

			ParameterScenario();
			Console.WriteLine();
			Console.WriteLine();
		}
	}
}
