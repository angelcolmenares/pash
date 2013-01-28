using System;
using System.Globalization;
using System.Threading;

namespace System.Runtime
{
	internal class NameGenerator
	{
		private static NameGenerator nameGenerator;

		private long id;

		private string prefix;

		static NameGenerator()
		{
			NameGenerator.nameGenerator = new NameGenerator();
		}

		private NameGenerator()
		{
			Guid guid = Guid.NewGuid();
			this.prefix = string.Concat("_", guid.ToString().Replace('-', '\u005F'), "_");
		}

		public static string Next()
		{
			long num = Interlocked.Increment(ref NameGenerator.nameGenerator.id);
			return string.Concat(NameGenerator.nameGenerator.prefix, num.ToString(CultureInfo.InvariantCulture));
		}
	}
}