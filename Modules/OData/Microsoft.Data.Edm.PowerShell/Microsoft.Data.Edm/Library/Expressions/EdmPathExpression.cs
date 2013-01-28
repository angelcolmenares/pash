using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Expressions;
using Microsoft.Data.Edm.Library;
using System;
using System.Collections.Generic;

namespace Microsoft.Data.Edm.Library.Expressions
{
	internal class EdmPathExpression : EdmElement, IEdmPathExpression, IEdmExpression, IEdmElement
	{
		private readonly IEnumerable<string> path;

		public EdmExpressionKind ExpressionKind
		{
			get
			{
				return EdmExpressionKind.Path;
			}
		}

		public IEnumerable<string> Path
		{
			get
			{
				return this.path;
			}
		}

		public EdmPathExpression(string path)
			: this(EdmUtil.CheckArgumentNull<string>(path, "path").Split(new char[] { '/' }))
		{

		}

		public EdmPathExpression(string[] path) : this((IEnumerable<string>)path)
		{
		}

		public EdmPathExpression(IEnumerable<string> path)
		{
			EdmUtil.CheckArgumentNull<IEnumerable<string>>(path, "path");
			foreach (string str in path)
			{
				if (!str.Contains("/"))
				{
					continue;
				}
				throw new ArgumentException(Strings.PathSegmentMustNotContainSlash);
			}
			this.path = path;
		}
	}
}