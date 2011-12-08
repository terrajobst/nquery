using System.Collections.Generic;

namespace NQuery.Compilation
{
	internal abstract class AstElement
	{
		public AstElement()
		{
		}

		public AstElement Clone()
		{
			Dictionary<AstElement,AstElement> alreadyClonedElements = new Dictionary<AstElement, AstElement>();
			return Clone(alreadyClonedElements);
		}

		public abstract AstElement Clone(Dictionary<AstElement,AstElement> alreadyClonedElements);
	}
}