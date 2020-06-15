using System;
using System.Collections.Generic;

namespace CSS.Connector.FileProcessors.Vanguard
{
	internal static class LinqExtentions
	{
		// To allow ForEach action with generic collections
		public static void ForEach<T>(this IEnumerable<T> source, Action<T> func)
		{
			foreach (var item in source) func(item);
		}
	}
}
