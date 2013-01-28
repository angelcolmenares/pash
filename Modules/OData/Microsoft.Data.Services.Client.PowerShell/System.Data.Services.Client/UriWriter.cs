namespace System.Data.Services.Client
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
	using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;

    internal class UriWriter : DataServiceALinqExpressionVisitor
    {
        private readonly DataServiceContext context;
        private ResourceSetExpression leafResourceSet;
        private readonly StringBuilder uriBuilder;
        private Version uriVersion;

        private UriWriter(DataServiceContext context)
        {
            this.context = context;
            this.uriBuilder = new StringBuilder();
            this.uriVersion = Util.DataServiceVersion1;
        }

        private string ExpressionToString(Expression expression)
        {
            return ExpressionWriter.ExpressionToString(this.context, expression, ref this.uriVersion);
        }

        internal static void Translate(DataServiceContext context, bool addTrailingParens, Expression e, out Uri uri, out Version version)
        {
            UriWriter writer = new UriWriter(context) {
                leafResourceSet = addTrailingParens ? (e as ResourceSetExpression) : null
            };
            writer.Visit(e);
            uri = Util.CreateUri(writer.uriBuilder.ToString(), UriKind.Absolute);
            version = writer.uriVersion;
        }

        internal override Expression VisitBinary(BinaryExpression b)
        {
            throw new NotSupportedException(System.Data.Services.Client.Strings.ALinq_BinaryNotSupported(b.NodeType.ToString()));
        }

        internal override Expression VisitConditional(ConditionalExpression c)
        {
            throw new NotSupportedException(System.Data.Services.Client.Strings.ALinq_ConditionalNotSupported);
        }

        internal override Expression VisitConstant(ConstantExpression c)
        {
            throw new NotSupportedException(System.Data.Services.Client.Strings.ALinq_ConstantNotSupported(c.Value));
        }

        internal void VisitCountOptions()
        {
            this.uriBuilder.Append('$');
            this.uriBuilder.Append("inlinecount");
            this.uriBuilder.Append('=');
            this.uriBuilder.Append("allpages");
            WebUtil.RaiseVersion(ref this.uriVersion, Util.DataServiceVersion2);
        }

        internal void VisitCustomQueryOptions(Dictionary<ConstantExpression, ConstantExpression> options)
        {
            List<ConstantExpression> list = options.Keys.ToList<ConstantExpression>();
            List<ConstantExpression> list2 = options.Values.ToList<ConstantExpression>();
            int num = 0;
            while (true)
            {
                this.uriBuilder.Append(list[num].Value);
                this.uriBuilder.Append('=');
                this.uriBuilder.Append(list2[num].Value);
                string str = list[num].Value.ToString();
                if (str.Equals('$' + "inlinecount", StringComparison.Ordinal) || str.Equals('$' + "select", StringComparison.Ordinal))
                {
                    WebUtil.RaiseVersion(ref this.uriVersion, Util.DataServiceVersion2);
                }
                if (++num == list.Count)
                {
                    return;
                }
                this.uriBuilder.Append('&');
            }
        }

        internal void VisitExpandOptions(List<string> paths)
        {
            this.uriBuilder.Append('$');
            this.uriBuilder.Append("expand");
            this.uriBuilder.Append('=');
            int num = 0;
            while (true)
            {
                this.uriBuilder.Append(paths[num]);
                if (++num == paths.Count)
                {
                    return;
                }
                this.uriBuilder.Append(',');
            }
        }

        internal override Expression VisitInvocation(InvocationExpression iv)
        {
            throw new NotSupportedException(System.Data.Services.Client.Strings.ALinq_InvocationNotSupported);
        }

        internal override Expression VisitLambda(LambdaExpression lambda)
        {
            throw new NotSupportedException(System.Data.Services.Client.Strings.ALinq_LambdaNotSupported);
        }

        internal override Expression VisitListInit(ListInitExpression init)
        {
            throw new NotSupportedException(System.Data.Services.Client.Strings.ALinq_ListInitNotSupported);
        }

        internal override Expression VisitMemberAccess(MemberExpression m)
        {
            throw new NotSupportedException(System.Data.Services.Client.Strings.ALinq_MemberAccessNotSupported(m.Member.Name));
        }

        internal override Expression VisitMemberInit(MemberInitExpression init)
        {
            throw new NotSupportedException(System.Data.Services.Client.Strings.ALinq_MemberInitNotSupported);
        }

        internal override Expression VisitMethodCall(MethodCallExpression m)
        {
            throw System.Data.Services.Client.Error.MethodNotSupported(m);
        }

        internal override Expression VisitNavigationPropertySingletonExpression(NavigationPropertySingletonExpression npse)
        {
            this.Visit(npse.Source);
            this.uriBuilder.Append('/').Append(this.ExpressionToString(npse.MemberExpression));
            this.VisitQueryOptions(npse);
            return npse;
        }

        internal override NewExpression VisitNew(NewExpression nex)
        {
            throw new NotSupportedException(System.Data.Services.Client.Strings.ALinq_NewNotSupported);
        }

        internal override Expression VisitNewArray(NewArrayExpression na)
        {
            throw new NotSupportedException(System.Data.Services.Client.Strings.ALinq_NewArrayNotSupported);
        }

        internal override Expression VisitParameter(ParameterExpression p)
        {
            throw new NotSupportedException(System.Data.Services.Client.Strings.ALinq_ParameterNotSupported);
        }

        internal void VisitProjectionPaths(List<string> paths)
        {
            this.uriBuilder.Append('$');
            this.uriBuilder.Append("select");
            this.uriBuilder.Append('=');
            int num = 0;
            while (true)
            {
                string str = paths[num];
                this.uriBuilder.Append(str);
                if (++num == paths.Count)
                {
                    break;
                }
                this.uriBuilder.Append(',');
            }
            WebUtil.RaiseVersion(ref this.uriVersion, Util.DataServiceVersion2);
        }

        internal void VisitQueryOptionExpression(FilterQueryOptionExpression fqoe)
        {
            this.uriBuilder.Append('$');
            this.uriBuilder.Append("filter");
            this.uriBuilder.Append('=');
            this.uriBuilder.Append(this.ExpressionToString(fqoe.Predicate));
        }

        internal void VisitQueryOptionExpression(OrderByQueryOptionExpression oboe)
        {
            this.uriBuilder.Append('$');
            this.uriBuilder.Append("orderby");
            this.uriBuilder.Append('=');
            int num = 0;
            while (true)
            {
                OrderByQueryOptionExpression.Selector selector = oboe.Selectors[num];
                this.uriBuilder.Append(this.ExpressionToString(selector.Expression));
                if (selector.Descending)
                {
                    this.uriBuilder.Append(' ');
                    this.uriBuilder.Append("desc");
                }
                if (++num == oboe.Selectors.Count)
                {
                    return;
                }
                this.uriBuilder.Append(',');
            }
        }

        internal void VisitQueryOptionExpression(SkipQueryOptionExpression sqoe)
        {
            this.uriBuilder.Append('$');
            this.uriBuilder.Append("skip");
            this.uriBuilder.Append('=');
            this.uriBuilder.Append(this.ExpressionToString(sqoe.SkipAmount));
        }

        internal void VisitQueryOptionExpression(TakeQueryOptionExpression tqoe)
        {
            this.uriBuilder.Append('$');
            this.uriBuilder.Append("top");
            this.uriBuilder.Append('=');
            this.uriBuilder.Append(this.ExpressionToString(tqoe.TakeAmount));
        }

        internal void VisitQueryOptions(ResourceExpression re)
        {
            bool flag = false;
            if (re.HasQueryOptions)
            {
                this.uriBuilder.Append('?');
                ResourceSetExpression expression = re as ResourceSetExpression;
                if (expression != null)
                {
                    IEnumerator enumerator = expression.SequenceQueryOptions.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        if (flag)
                        {
                            this.uriBuilder.Append('&');
                        }
                        Expression current = (Expression) enumerator.Current;
                        switch (current.NodeType)
                        {
                            case ((ExpressionType) 0x2713):
                                this.VisitQueryOptionExpression((TakeQueryOptionExpression) current);
                                break;

                            case ((ExpressionType) 0x2714):
                                this.VisitQueryOptionExpression((SkipQueryOptionExpression) current);
                                break;

                            case ((ExpressionType) 0x2715):
                                this.VisitQueryOptionExpression((OrderByQueryOptionExpression) current);
                                break;

                            case ((ExpressionType) 0x2716):
                                this.VisitQueryOptionExpression((FilterQueryOptionExpression) current);
                                break;
                        }
                        flag = true;
                    }
                }
                if (re.ExpandPaths.Count > 0)
                {
                    if (flag)
                    {
                        this.uriBuilder.Append('&');
                    }
                    this.VisitExpandOptions(re.ExpandPaths);
                    flag = true;
                }
                if ((re.Projection != null) && (re.Projection.Paths.Count > 0))
                {
                    if (flag)
                    {
                        this.uriBuilder.Append('&');
                    }
                    this.VisitProjectionPaths(re.Projection.Paths);
                    flag = true;
                }
                if (re.CountOption == CountOption.InlineAll)
                {
                    if (flag)
                    {
                        this.uriBuilder.Append('&');
                    }
                    this.VisitCountOptions();
                    flag = true;
                }
                if (re.CustomQueryOptions.Count > 0)
                {
                    if (flag)
                    {
                        this.uriBuilder.Append('&');
                    }
                    this.VisitCustomQueryOptions(re.CustomQueryOptions);
                    flag = true;
                }
            }
        }

        internal override Expression VisitResourceSetExpression(ResourceSetExpression rse)
        {
            if (rse.NodeType == ((ExpressionType) 0x2711))
            {
                this.Visit(rse.Source);
                this.uriBuilder.Append('/').Append(this.ExpressionToString(rse.MemberExpression));
            }
            else
            {
                string entitySetName = (string) ((ConstantExpression) rse.MemberExpression).Value;
                this.uriBuilder.Append(this.context.BaseUriResolver.GetEntitySetUri(entitySetName));
            }
            WebUtil.RaiseVersion(ref this.uriVersion, rse.UriVersion);
            if (rse.ResourceTypeAs != null)
            {
                this.uriBuilder.Append('/');
                this.uriBuilder.Append(System.Data.Services.Client.UriHelper.GetEntityTypeNameForUriAndValidateMaxProtocolVersion(rse.ResourceTypeAs, this.context, ref this.uriVersion));
            }
            if (rse.KeyPredicate != null)
            {
                this.uriBuilder.Append('(');
                if (rse.KeyPredicate.Count == 1)
                {
                    this.uriBuilder.Append(this.ExpressionToString(rse.KeyPredicate.Values.First<ConstantExpression>()));
                }
                else
                {
                    bool flag = false;
                    foreach (KeyValuePair<PropertyInfo, ConstantExpression> pair in rse.KeyPredicate)
                    {
                        if (flag)
                        {
                            this.uriBuilder.Append(',');
                        }
                        this.uriBuilder.Append(pair.Key.Name);
                        this.uriBuilder.Append('=');
                        this.uriBuilder.Append(this.ExpressionToString(pair.Value));
                        flag = true;
                    }
                }
                this.uriBuilder.Append(')');
            }
            else if (rse == this.leafResourceSet)
            {
                this.uriBuilder.Append('(');
                this.uriBuilder.Append(')');
            }
            if (rse.CountOption == CountOption.ValueOnly)
            {
                this.uriBuilder.Append('/').Append('$').Append("count");
                WebUtil.RaiseVersion(ref this.uriVersion, Util.DataServiceVersion2);
            }
            this.VisitQueryOptions(rse);
            return rse;
        }

        internal override Expression VisitTypeIs(TypeBinaryExpression b)
        {
            throw new NotSupportedException(System.Data.Services.Client.Strings.ALinq_TypeBinaryNotSupported);
        }

        internal override Expression VisitUnary(UnaryExpression u)
        {
            throw new NotSupportedException(System.Data.Services.Client.Strings.ALinq_UnaryNotSupported(u.NodeType.ToString()));
        }
    }
}

