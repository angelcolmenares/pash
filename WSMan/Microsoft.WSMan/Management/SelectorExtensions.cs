using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.WSMan.Management
{
	/// <summary>
	/// Selector extensions.
	/// </summary>
	public static class SelectorExtensions
	{
		/// <summary>
		/// 
		/// </summary>
		/// <returns>
		/// The selector.
		/// </returns>
		/// <param name='obj'>
		/// Object.
		/// </param>
		/// <param name='name'>
		/// Name.
		/// </param>
		public static Selector GetSelector(this IEnumerable<Selector> obj, string name)
		{
			return obj.FirstOrDefault (x => x.Name.Equals (name));
		}

		/// <summary>
		/// /
		/// </summary>
		/// <returns>
		/// The selector value as string.
		/// </returns>
		/// <param name='obj'>
		/// Object.
		/// </param>
		/// <param name='name'>
		/// Name.
		/// </param>
		public static string GetSelectorValueAsString(this IEnumerable<Selector> obj, string name)
		{
			var target = obj.GetSelector (name);
			if (target == null) return null;
			if (target.IsSimpleValue) return target.SimpleValue;
			return null;
		}

		/// <summary>
		/// Gets the selector value as GUID.
		/// </summary>
		/// <returns>
		/// The selector value as GUID.
		/// </returns>
		/// <param name='obj'>
		/// Object.
		/// </param>
		/// <param name='name'>
		/// Name.
		/// </param>
		public static Guid GetSelectorValueAsGuid (this IEnumerable<Selector> obj, string name)
		{
			string value = obj.GetSelectorValueAsString (name);
			if (!string.IsNullOrEmpty (value)) {
				Guid result;
				if (Guid.TryParse (value, out result))
				{
					return result;
				}
			}
			return Guid.Empty;
		}

		/// <summary>
		/// Gets the selector value as byte array.
		/// </summary>
		/// <returns>
		/// The selector value as byte array.
		/// </returns>
		/// <param name='obj'>
		/// Object.
		/// </param>
		/// <param name='name'>
		/// Name.
		/// </param>
		public static byte[] GetSelectorValueAsByteArray(this IEnumerable<Selector> obj, string name)
		{
			string value = obj.GetSelectorValueAsString (name);
			if (!string.IsNullOrEmpty (value)) {
				return Convert.FromBase64String (value);
			}
			return new byte[0];
		}

	}
}

