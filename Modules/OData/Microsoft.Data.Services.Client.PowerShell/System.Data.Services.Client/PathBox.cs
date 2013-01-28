namespace System.Data.Services.Client
{
    using System;
    using System.Collections.Generic;
    using System.Data.Services.Client.Metadata;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Text;

    internal class PathBox
    {
        private readonly Dictionary<ParameterExpression, string> basePaths = new Dictionary<ParameterExpression, string>(ReferenceEqualityComparer<ParameterExpression>.Instance);
        private const char EntireEntityMarker = '*';
        private readonly List<StringBuilder> expandPaths = new List<StringBuilder>();
        private readonly Stack<ParameterExpression> parameterExpressions = new Stack<ParameterExpression>();
        private readonly List<StringBuilder> projectionPaths = new List<StringBuilder>();
        private Version uriVersion;

        internal PathBox()
        {
            this.projectionPaths.Add(new StringBuilder());
            this.uriVersion = Util.DataServiceVersion1;
        }

        private static void AddEntireEntityMarker(StringBuilder sb)
        {
            if (sb.Length > 0)
            {
                sb.Append('/');
            }
            sb.Append('*');
        }

        internal void AppendPropertyToPath(PropertyInfo pi, Type convertedSourceType, DataServiceContext context)
        {
            bool flag = ClientTypeUtil.TypeOrElementTypeIsEntity(pi.PropertyType);
            string name = (convertedSourceType == null) ? null : System.Data.Services.Client.UriHelper.GetEntityTypeNameForUriAndValidateMaxProtocolVersion(convertedSourceType, context, ref this.uriVersion);
            if (flag)
            {
                if (convertedSourceType != null)
                {
                    this.AppendToExpandPath(name);
                }
                this.AppendToExpandPath(pi.Name);
            }
            StringBuilder sb = null;
            if (convertedSourceType != null)
            {
                this.AppendToProjectionPath(name, false);
            }
            sb = this.AppendToProjectionPath(pi.Name, false);
            if (flag)
            {
                AddEntireEntityMarker(sb);
            }
        }

        private void AppendToExpandPath(string name)
        {
            StringBuilder builder = this.expandPaths.Last<StringBuilder>();
            if (builder.Length > 0)
            {
                builder.Append('/');
            }
            builder.Append(name);
        }

        private StringBuilder AppendToProjectionPath(string name, bool replaceEntityMarkerIfPresent)
        {
            StringBuilder sb = this.projectionPaths.Last<StringBuilder>();
            bool flag = RemoveEntireEntityMarkerIfPresent(sb);
            if (sb.Length > 0)
            {
                sb.Append('/');
            }
            sb.Append(name);
            if (flag && replaceEntityMarkerIfPresent)
            {
                AddEntireEntityMarker(sb);
            }
            return sb;
        }

        internal void PopParamExpression()
        {
            this.parameterExpressions.Pop();
        }

        internal void PushParamExpression(ParameterExpression pe)
        {
            StringBuilder item = this.projectionPaths.Last<StringBuilder>();
            this.basePaths.Add(pe, item.ToString());
            this.projectionPaths.Remove(item);
            this.parameterExpressions.Push(pe);
        }

        private static bool RemoveEntireEntityMarkerIfPresent(StringBuilder sb)
        {
            bool flag = false;
            if ((sb.Length > 0) && (sb[sb.Length - 1] == '*'))
            {
                sb.Remove(sb.Length - 1, 1);
                flag = true;
            }
            if ((sb.Length > 0) && (sb[sb.Length - 1] == '/'))
            {
                sb.Remove(sb.Length - 1, 1);
            }
            return flag;
        }

        internal void StartNewPath()
        {
            StringBuilder sb = new StringBuilder(this.basePaths[this.ParamExpressionInScope]);
            RemoveEntireEntityMarkerIfPresent(sb);
            this.expandPaths.Add(new StringBuilder(sb.ToString()));
            AddEntireEntityMarker(sb);
            this.projectionPaths.Add(sb);
        }

        internal IEnumerable<string> ExpandPaths
        {
            get
            {
                return (from s in this.expandPaths
                    where s.Length > 0
                    select s.ToString()).Distinct<string>();
            }
        }

        internal ParameterExpression ParamExpressionInScope
        {
            get
            {
                return this.parameterExpressions.Peek();
            }
        }

        internal IEnumerable<string> ProjectionPaths
        {
            get
            {
                return (from s in this.projectionPaths
                    where s.Length > 0
                    select s.ToString()).Distinct<string>();
            }
        }

        internal Version UriVersion
        {
            get
            {
                return this.uriVersion;
            }
        }
    }
}

