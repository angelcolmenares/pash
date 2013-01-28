using System;
using System.Collections.Generic;

namespace System.Management.Automation
{
	public static class CollectionExtensions
	{
		public static IEnumerable<T> Traverse<T>(
			this IEnumerable<T> source,
			Func<T, IEnumerable<T>> recurse)
		{
			foreach (T item in source)
			{
				yield return item;
				
				foreach (T nestedItem in Traverse(recurse(item), recurse))
				{
					yield return nestedItem;
				}
			}
		}
	}
}

