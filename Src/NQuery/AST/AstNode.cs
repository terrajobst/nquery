using System;

namespace NQuery.Compilation
{
	internal abstract class AstNode : AstElement
	{
		public string GenerateSource()
		{
			SourceGenerator generator = new SourceGenerator();
			generator.Visit(this);
			return generator.ToString();
		}

		public bool IsStructuralEqualTo(AstNode astNode)
		{
			if (astNode == null)
				return false;
			
			if (ReferenceEquals(this, astNode))
				return true;
			
			return Comparer.Visit(this, astNode);
		}		

		public override sealed string ToString()
		{
			return GenerateSource();
		}

		public abstract AstNodeType NodeType { get; }
	}
}