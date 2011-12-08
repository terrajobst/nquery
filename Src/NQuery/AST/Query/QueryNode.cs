namespace NQuery.Compilation
{
	internal abstract class QueryNode : AstNode
	{
		public abstract SelectColumn[] GetColumns();
	}
}