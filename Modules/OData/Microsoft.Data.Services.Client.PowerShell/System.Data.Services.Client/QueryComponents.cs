namespace System.Data.Services.Client
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    internal class QueryComponents
    {
        private readonly List<BodyOperationParameter> bodyOperationParameters;
        private readonly string httpMethod;
        private readonly Type lastSegmentType;
        private readonly Dictionary<Expression, Expression> normalizerRewrites;
        private readonly LambdaExpression projection;
        private readonly bool? singleResult;
        private System.Uri uri;
        private readonly List<UriOperationParameter> uriOperationParameters;
        private System.Version version;

        internal QueryComponents(System.Uri uri, System.Version version, Type lastSegmentType, LambdaExpression projection, Dictionary<Expression, Expression> normalizerRewrites)
        {
            this.projection = projection;
            this.normalizerRewrites = normalizerRewrites;
            this.lastSegmentType = lastSegmentType;
            this.uri = uri;
            this.version = version;
            this.httpMethod = "GET";
        }

        internal QueryComponents(System.Uri uri, System.Version version, Type lastSegmentType, LambdaExpression projection, Dictionary<Expression, Expression> normalizerRewrites, string httpMethod, bool? singleResult, List<BodyOperationParameter> bodyOperationParameters, List<UriOperationParameter> uriOperationParameters)
        {
            this.projection = projection;
            this.normalizerRewrites = normalizerRewrites;
            this.lastSegmentType = lastSegmentType;
            this.uri = uri;
            this.version = version;
            this.httpMethod = httpMethod;
            this.uriOperationParameters = uriOperationParameters;
            this.bodyOperationParameters = bodyOperationParameters;
            this.singleResult = singleResult;
        }

        internal List<BodyOperationParameter> BodyOperationParameters
        {
            get
            {
                return this.bodyOperationParameters;
            }
        }

        internal string HttpMethod
        {
            get
            {
                return this.httpMethod;
            }
        }

        internal Type LastSegmentType
        {
            get
            {
                return this.lastSegmentType;
            }
        }

        internal Dictionary<Expression, Expression> NormalizerRewrites
        {
            get
            {
                return this.normalizerRewrites;
            }
        }

        internal LambdaExpression Projection
        {
            get
            {
                return this.projection;
            }
        }

        internal bool? SingleResult
        {
            get
            {
                return this.singleResult;
            }
        }

        internal System.Uri Uri
        {
            get
            {
                return this.uri;
            }
            set
            {
                this.uri = value;
            }
        }

        internal List<UriOperationParameter> UriOperationParameters
        {
            get
            {
                return this.uriOperationParameters;
            }
        }

        internal System.Version Version
        {
            get
            {
                return this.version;
            }
        }
    }
}

