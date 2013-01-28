namespace System.Data.Services.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Data.Services;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [DebuggerDisplay("{PathExpression}")]
    internal class ResourceSetPathExpression
    {
        private OperationParameter bindingParameter;
        private readonly string pathExpression;
        private PathSegment[] pathSegments;
        internal const char PathSeparator = '/';

        public ResourceSetPathExpression(string pathExpression)
        {
            WebUtil.CheckStringArgumentNullOrEmpty(pathExpression, "pathExpression");
            this.pathExpression = pathExpression;
        }

        internal ResourceSetWrapper GetTargetSet(DataServiceProviderWrapper provider, ResourceSetWrapper bindingSet)
        {
            ResourceSetWrapper sourceContainer = bindingSet;
            for (int i = 0; (sourceContainer != null) && (i < this.pathSegments.Length); i++)
            {
                PathSegment segment = this.pathSegments[i];
                if (segment.Property != null)
                {
                    sourceContainer = provider.GetContainer(sourceContainer, segment.SourceType, segment.Property);
                }
            }
            return sourceContainer;
        }

        internal void InitializePathSegments(DataServiceProviderWrapper provider)
        {
            if (this.pathSegments == null)
            {
                string[] strArray = this.pathExpression.Split(new char[] { '/' });
                ResourceType parameterType = this.bindingParameter.ParameterType;
                if (parameterType.ResourceTypeKind == ResourceTypeKind.EntityCollection)
                {
                    parameterType = ((EntityCollectionResourceType) this.bindingParameter.ParameterType).ItemType;
                }
                List<PathSegment> list = new List<PathSegment>();
                PathSegment item = new PathSegment {
                    SourceType = parameterType
                };
                bool flag = false;
                int length = strArray.Length;
                if (length == 1)
                {
                    list.Add(item);
                }
                else
                {
                    for (int i = 1; i < length; i++)
                    {
                        string str = strArray[i];
                        if (string.IsNullOrEmpty(str))
                        {
                            throw new InvalidOperationException(Strings.ResourceSetPathExpression_EmptySegment(this.pathExpression));
                        }
                        ResourceProperty property = item.SourceType.TryResolvePropertyName(str);
                        if (property == null)
                        {
                            bool previousSegmentIsTypeSegment = flag;
                            ResourceType type2 = WebUtil.ResolveTypeIdentifier(provider, str, item.SourceType, previousSegmentIsTypeSegment);
                            if (type2 == null)
                            {
                                throw new InvalidOperationException(Strings.ResourceSetPathExpression_PropertyNotFound(this.pathExpression, str, item.SourceType.FullName));
                            }
                            item.SourceType = type2;
                            flag = true;
                            if (i == (length - 1))
                            {
                                throw new InvalidOperationException(Strings.ResourceSetPathExpression_PathCannotEndWithTypeIdentifier(this.pathExpression, strArray[length - 1]));
                            }
                        }
                        else
                        {
                            flag = false;
                            item.Property = property;
                            list.Add(item);
                            item = new PathSegment {
                                SourceType = property.ResourceType
                            };
                        }
                        if (item.SourceType.ResourceTypeKind != ResourceTypeKind.EntityType)
                        {
                            throw new InvalidOperationException(Strings.ResourceSetPathExpression_PropertyMustBeEntityType(this.pathExpression, str, item.SourceType.FullName));
                        }
                    }
                }
                this.pathSegments = list.ToArray();
            }
        }

        internal void SetBindingParameter(OperationParameter parameter)
        {
            if ((this.PathExpression != parameter.Name) && !this.PathExpression.StartsWith(parameter.Name + '/', StringComparison.Ordinal))
            {
                throw new InvalidOperationException(Strings.ResourceSetPathExpression_PathExpressionMustStartWithBindingParameterName(this.PathExpression, parameter.Name));
            }
            this.bindingParameter = parameter;
        }

        public string PathExpression
        {
            get
            {
                return this.pathExpression;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct PathSegment
        {
            public ResourceType SourceType { get; set; }
            public ResourceProperty Property { get; set; }
        }
    }
}

