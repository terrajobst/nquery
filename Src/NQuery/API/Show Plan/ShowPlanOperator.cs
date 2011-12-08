using System;

namespace NQuery
{
	/// <summary>
	/// Represents the physical operator of a <see cref="ShowPlanElement"/>.
	/// </summary>
	public enum ShowPlanOperator
	{
		/// <summary>
		/// Indicates that the <see cref="ShowPlanElement"/> represents the 
		/// <a href="\Execution Plan Reference\Select.html">Select</a> operator.
		/// </summary>
		Select,

		/// <summary>
		/// Indicates that the <see cref="ShowPlanElement"/> represents the 
		/// <a href="\Execution Plan Reference\Table Scan.html">Table Scan</a> operator.
		/// </summary>
		TableScan,

		/// <summary>
		/// Indicates that the <see cref="ShowPlanElement"/> represents the 
		/// <a href="\Execution Plan Reference\Nested Loops.html">Nested Loops</a> operator.
		/// </summary>
		NestedLoops,

		/// <summary>
		/// Indicates that the <see cref="ShowPlanElement"/> represents the 
		/// <a href="\Execution Plan Reference\Constant Scan.html">Constant Scan</a> operator.
		/// </summary>
		ConstantScan,

		/// <summary>
		/// Indicates that the <see cref="ShowPlanElement"/> represents the 
		/// <a href="\Execution Plan Reference\Compute Scalar.html">Compute Scalar</a> operator.
		/// </summary>
		ComputeScalar,

		/// <summary>
		/// Indicates that the <see cref="ShowPlanElement"/> represents the 
		/// <a href="\Execution Plan Reference\Concatenation.html">Concatenation</a> operator.
		/// </summary>
		Concatenation,

		/// <summary>
		/// Indicates that the <see cref="ShowPlanElement"/> represents the 
		/// <a href="\Execution Plan Reference\Sort.html">Sort</a> operator.
		/// </summary>
		Sort,

		/// <summary>
		/// Indicates that the <see cref="ShowPlanElement"/> represents the 
		/// <a href="\Execution Plan Reference\Stream Aggregate.html">Stream Aggregate</a> operator.
		/// </summary>
		StreamAggregate,

		/// <summary>
		/// Indicates that the <see cref="ShowPlanElement"/> represents the 
		/// <a href="\Execution Plan Reference\Top.html">Top</a> operator.
		/// </summary>
		Top,

		/// <summary>
		/// Indicates that the <see cref="ShowPlanElement"/> represents the 
		/// <a href="\Execution Plan Reference\Filter.html">Filter</a> operator.
		/// </summary>
		Filter,

		/// <summary>
		/// Indicates that the <see cref="ShowPlanElement"/> represents the 
		/// <a href="\Execution Plan Reference\Assert.html">Assert</a> operator.
		/// </summary>
		Assert,

		/// <summary>
		/// Indicates that the <see cref="ShowPlanElement"/> represents the 
		/// <a href="\Execution Plan Reference\Table Spool.html">Table Spool</a> operator.
		/// </summary>
		TableSpool,

		/// <summary>
		/// Indicates that the <see cref="ShowPlanElement"/> represents the 
		/// <a href="\Execution Plan Reference\Index Spool.html">Index Spool</a> operator.
		/// </summary>
		IndexSpool,

		/// <summary>
		/// Indicates that the <see cref="ShowPlanElement"/> represents the 
		/// <a href="\Execution Plan Reference\Hash Match.html">Hash Match</a> operator.
		/// </summary>
		HashMatch,
	}
}