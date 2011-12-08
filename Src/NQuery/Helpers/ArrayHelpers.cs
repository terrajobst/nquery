using System;
using System.Collections.Generic;

using NQuery.Compilation;

namespace NQuery
{
	internal static class ArrayHelpers
	{
		public static bool Contains<T>(T[] array, T value)
		{
			return Array.IndexOf(array, value) >= 0;
		}

		public static T[] Clone<T>(T[] source)
		{
			if (source == null)
				return null;

			return (T[]) source.Clone();
		}

		public static T[] CreateDeepCopyOfAstElementArray<T>(T[] source, Dictionary<AstElement, AstElement> alreadyClonedElements) where T : AstElement
		{
			if (source == null)	
				return null;

            return Array.ConvertAll<T, T>(source, delegate(T a)
                                                      {
                                                          return (T) a.Clone(alreadyClonedElements);
                                                      });		    
		}

		public static T[] JoinArrays<T>(T[] array1, T[] array2)
		{
			if (array1 == null || array2 == null)
				return null;
			
			int totalLength = array1.Length + array2.Length;
			T[] result = new T[totalLength];
			Array.Copy(array1, 0, result, 0, array1.Length);
			Array.Copy(array2, 0, result, array1.Length, array2.Length);
			return result;
		}
	}
}
