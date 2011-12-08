using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;

namespace NQuery.Compilation
{
	internal sealed class Compiler
	{
		private const string PHASE_NORMALIZATION = "Normalization";
		private const string PHASE_RESOLUTION = "Resolution";
		private const string PHASE_AGGREGATE_BINDING = "AggregateBinding";
		private const string PHASE_CONSTANT_FOLDING = "ConstantFolding";
		private const string PHASE_VALIDATION = "Validation";
		private const string PHASE_ALGEBRAIZATION = "Algebraization";
		private const string PHASE_SEMI_JOIN_SIMPLIFICATION = "SemiJoinSimplification";
		private const string PHASE_DECORRELATION = "Decorrelation";
		private const string PHASE_SELECTION_PUSHING = "SelectionPushing";
		private const string PHASE_JOIN_LINEARIZATION = "JoinLinearization";
		private const string PHASE_OUTER_JOIN_REORDERING = "OuterJoinReordering";
		private const string PHASE_OUTER_REFERENCE_LABELING = "OuterReferenceLabeling";
		private const string PHASE_JOIN_ORDER_OPTIMIZATION = "JoinOrderOptimization";
		private const string PHASE_AT_MOST_ONE_ROW_REORDERING = "AtMostOneRowReordering";
		private const string PHASE_PUSH_COMPUTATIONS = "PushComputations";
		private const string PHASE_OUTPUTLIST_GENERATION = "OutputListGeneration";
		private const string PHASE_NULL_SCAN_OPTIMIZATION = "NullScanOptimization";
		private const string PHASE_OUTPUTLIST_OPTIMIZATION = "OutputListOptimization";
		private const string PHASE_ROW_BUFFER_ENTRY_NAMING = "RowBufferEntryNaming";
		private const string PHASE_COLUMN_AND_AGGREGATE_EXPRESSION_REPLACEMENT = "ColumnAndAggregateExpressionReplacement";
		private const string PHASE_ROW_BUFFER_ENTRY_INLINING = "RowBufferEntryInlining";
		private const string PHASE_OUTER_JOIN_REMOVAL = "OuterJoinRemoval";
		private const string PHASE_SPOOL_INSERTION = "SpoolInsertion";
		private const string PHASE_PHYSICAL_JOIN_OP_CHOOSING = "PhysicalJoinOperationChoosing";
		private const string PHASE_FULL_OUTER_JOIN_EXPANSION = "FullOuterJoinExpansion";
		private const string PHASE_CONVERSION_TO_TARGET_TYPE = "ConversionToTargetType";

		private IErrorReporter _errorReporter;

		public Compiler(IErrorReporter errorReporter)
		{
			_errorReporter = errorReporter;
		}

		#region Phase Runner

		private delegate AstNode PhaseOutputHandler(AstNode input);

		private class Phase
		{
			private string _name;
			private PhaseOutputHandler _outputHandler;
			private AstNode _result;
			private bool _logResult;

			public Phase(string name, PhaseOutputHandler outputHandler, bool logResult)
			{
				_name = name;
				_outputHandler = outputHandler;
				_logResult = logResult;
			}

			public string Name
			{
				get { return _name; }
			}

			public PhaseOutputHandler OutputHandler
			{
				get { return _outputHandler; }
			}

			public AstNode Result
			{
				get { return _result; }
				set { _result = value; }
			}

			public bool LogResult
			{
				get { return _logResult; }
			}
		}

		private class PhaseCollection : Collection<Phase>
		{
			public void Add(string name, PhaseOutputHandler outputHandler)
			{
				Add(new Phase(name, outputHandler, false));
			}

			public void Add(string name, StandardVisitor visitor)
			{
				Add(new Phase(name, delegate(AstNode input)
										{
											return visitor.Visit(input);
										}, false));
			}
		}

		private class PhaseRunner
		{
			private IErrorReporter _errorReporter;
			private AstNode _astNode;
			private PhaseCollection _phases = new PhaseCollection();

			public PhaseRunner(IErrorReporter errorReporter, AstNode astNode)
			{
				_errorReporter = errorReporter;
				_astNode = astNode;
			}

			public AstNode Run()
			{
				AstNode currentNode = _astNode;
				int phaseNumber = 1;
				foreach (Phase phase in _phases)
				{
					phase.Result = phase.OutputHandler(currentNode);
					if (phase.LogResult)
						WriteAstToFile(phase.Result, phase.Name, phaseNumber++);

					if (_errorReporter.ErrorsSeen)
						return null;

					currentNode = phase.Result;
				}

				return currentNode;
			}

			private static void WriteAstToFile(AstNode node, string phaseName, int phaseNumber)
			{
				string numberedPhaseName = String.Format(CultureInfo.InvariantCulture, "{0}-{1}", phaseNumber, phaseName);
				string fileName = Path.ChangeExtension(numberedPhaseName, ".xml");
				fileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
				XmlProducer.ProduceFile(fileName, node);
			}

			public PhaseCollection Phases
			{
				get { return _phases; }
			}
		}

		#endregion

		[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		public ResultAlgebraNode CompileQuery(string queryText, Scope scope)
		{
			// Algebraization

			Parser parser = new Parser(_errorReporter);
			QueryNode parsedQuery = parser.ParseQuery(queryText);

			// When the parser detected errors or could not even construct an AST it
			// does not make any sense to go ahead.

			if (_errorReporter.ErrorsSeen || parsedQuery == null)
				return null;

			PhaseRunner phaseRunner = new PhaseRunner(_errorReporter, parsedQuery);
			phaseRunner.Phases.Add(PHASE_NORMALIZATION, new Normalizer());
			phaseRunner.Phases.Add(PHASE_RESOLUTION, new Resolver(_errorReporter, scope));
			phaseRunner.Phases.Add(PHASE_AGGREGATE_BINDING, new AggregateBinder(_errorReporter));
			phaseRunner.Phases.Add(PHASE_VALIDATION, new Validator(_errorReporter, scope.DataContext.MetadataContext));
			phaseRunner.Phases.Add(PHASE_CONSTANT_FOLDING, new ConstantFolder(_errorReporter));
			phaseRunner.Phases.Add(PHASE_COLUMN_AND_AGGREGATE_EXPRESSION_REPLACEMENT, new ColumnAndAggregateExpressionReplacer());
			phaseRunner.Phases.Add(PHASE_ALGEBRAIZATION, delegate(AstNode input)
														   {
															   return Algebrizer.Convert((QueryNode)input);
														   });
			phaseRunner.Phases.Add(PHASE_ROW_BUFFER_ENTRY_INLINING, new RowBufferEntryInliner());
			phaseRunner.Phases.Add(PHASE_SEMI_JOIN_SIMPLIFICATION, new SemiJoinSimplifier());
			phaseRunner.Phases.Add(PHASE_DECORRELATION, new Decorrelator());
			phaseRunner.Phases.Add(PHASE_OUTER_JOIN_REMOVAL, new OuterJoinRemover());
			phaseRunner.Phases.Add(PHASE_SELECTION_PUSHING, new SelectionPusher());
			phaseRunner.Phases.Add(PHASE_JOIN_LINEARIZATION, new JoinLinearizer());
			phaseRunner.Phases.Add(PHASE_OUTER_JOIN_REORDERING, new OuterJoinReorderer());
			phaseRunner.Phases.Add(PHASE_JOIN_ORDER_OPTIMIZATION, new JoinOrderOptimizer());
			phaseRunner.Phases.Add(PHASE_AT_MOST_ONE_ROW_REORDERING, new AtMostOneRowReorderer());
			phaseRunner.Phases.Add(PHASE_OUTER_REFERENCE_LABELING, new OuterReferenceLabeler());
			phaseRunner.Phases.Add(PHASE_SPOOL_INSERTION, new SpoolInserter());
			phaseRunner.Phases.Add(PHASE_PUSH_COMPUTATIONS, new ComputationPusher());
			phaseRunner.Phases.Add(PHASE_PHYSICAL_JOIN_OP_CHOOSING, new PhysicalJoinOperationChooser());
			phaseRunner.Phases.Add(PHASE_OUTPUTLIST_GENERATION, new OutputListGenerator());
			phaseRunner.Phases.Add(PHASE_NULL_SCAN_OPTIMIZATION, new NullScanOptimizer());
			phaseRunner.Phases.Add(PHASE_FULL_OUTER_JOIN_EXPANSION, new FullOuterJoinExpander());
			phaseRunner.Phases.Add(PHASE_OUTPUTLIST_OPTIMIZATION, new OutputListOptimizer());
			phaseRunner.Phases.Add(PHASE_ROW_BUFFER_ENTRY_NAMING, new RowBufferEntryNamer());

			// TODO: Acutally, we should perform "PushComputations" after "NullScanOptimization" since
			//       we can merge some ComputeScalar nodes there. But currently the "PushComputations"
			//       visitor does not correctly update the OutputList so later phases will fail.

			// TODO: The phases "SpoolInsertion" and "PhysicalJoinOperationChoosing" should be the last
			//       entries but currently they must precede PHASE_OUTPUTLIST_GENERATION, PHASE_OUTPUTLIST_OPTIMIZATION,
			//       and PHASE_ROW_BUFFER_ENTRY_NAMING.

			return (ResultAlgebraNode)phaseRunner.Run();
		}

		public ExpressionNode CompileExpression(string expressionText, Type targetType, Scope scope)
		{
			// Parse the expression.

			Parser parser = new Parser(_errorReporter);
			ExpressionNode expressionNode = parser.ParseExpression(expressionText);

			// When the parser detected errors or could not even construct an AST it
			// does not make any sense to go ahead.

			if (_errorReporter.ErrorsSeen || expressionNode == null)
				return null;

			PhaseRunner phaseRunner = new PhaseRunner(_errorReporter, expressionNode);
			phaseRunner.Phases.Add(PHASE_NORMALIZATION, new Normalizer());
			phaseRunner.Phases.Add(PHASE_RESOLUTION, new Resolver(_errorReporter, scope));
			phaseRunner.Phases.Add(PHASE_CONSTANT_FOLDING, new ConstantFolder(_errorReporter));
			phaseRunner.Phases.Add(PHASE_VALIDATION, new Validator(_errorReporter, scope.DataContext.MetadataContext));
			phaseRunner.Phases.Add(PHASE_CONVERSION_TO_TARGET_TYPE, delegate(AstNode input)
																	{
																		if (input == null || _errorReporter.ErrorsSeen)
																			return null;

																		ExpressionNode expr = (ExpressionNode)input;
																		Binder binder = new Binder(_errorReporter);
																		return binder.ConvertExpressionIfRequired(expr, targetType);
																	});

			return (ExpressionNode)phaseRunner.Run();
		}
	}
}
