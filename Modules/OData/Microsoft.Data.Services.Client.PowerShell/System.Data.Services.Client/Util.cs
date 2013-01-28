namespace System.Data.Services.Client
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Services.Common;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Xml;

    internal static class Util
    {
        internal const string CodeGeneratorToolName = "System.Data.Services.Design";
        internal static readonly Version DataServiceVersion1 = new Version(1, 0);
        internal static readonly Version DataServiceVersion2 = new Version(2, 0);
        internal static readonly Version DataServiceVersion3 = new Version(3, 0);
        internal static readonly Version DataServiceVersionEmpty = new Version(0, 0);
        internal const string ExecuteMethodName = "Execute";
        internal const string ExecuteMethodNameForVoidResults = "ExecuteVoid";
        internal static readonly char[] ForwardSlash = new char[] { '/' };
        internal const string LoadPropertyMethodName = "LoadProperty";
        internal const string SaveChangesMethodName = "SaveChanges";
        internal static readonly Version[] SupportedResponseVersions = new Version[] { DataServiceVersion1, DataServiceVersion2, DataServiceVersion3 };
        internal const string VersionSuffix = ";NetFx";
        private static char[] whitespaceForTracing = new char[] { '\r', '\n', ' ', ' ', ' ', ' ', ' ' };

        internal static object ActivatorCreateInstance(Type type, params object[] arguments)
        {
            return Activator.CreateInstance(type, arguments);
        }

        internal static Uri AppendBaseUriAndRelativeUri(Uri baseUri, Uri relativeUri)
        {
            string str = CommonUtil.UriToString(baseUri);
            string str2 = CommonUtil.UriToString(relativeUri);
            if (str.EndsWith("/", StringComparison.Ordinal))
            {
                if (str2.StartsWith("/", StringComparison.Ordinal))
                {
                    relativeUri = new Uri(baseUri, CreateUri(str2.TrimStart(ForwardSlash), UriKind.Relative));
                    return relativeUri;
                }
                relativeUri = new Uri(baseUri, relativeUri);
                return relativeUri;
            }
            relativeUri = CreateUri(str + "/" + str2.TrimStart(ForwardSlash), UriKind.Absolute);
            return relativeUri;
        }

        internal static void CheckArgumentNotEmpty<T>(T[] value, string parameterName) where T: class
        {
            CheckArgumentNull<T[]>(value, parameterName);
            if (value.Length == 0)
            {
                throw Error.Argument(Strings.Util_EmptyArray, parameterName);
            }
            for (int i = 0; i < value.Length; i++)
            {
                if (object.ReferenceEquals(value[i], null))
                {
                    throw Error.Argument(Strings.Util_NullArrayElement, parameterName);
                }
            }
        }

        internal static T CheckArgumentNull<T>([ValidatedNotNull] T value, string parameterName) where T: class
        {
            if (value == null)
            {
                throw Error.ArgumentNull(parameterName);
            }
            return value;
        }

        internal static void CheckArgumentNullAndEmpty([ValidatedNotNull] string value, string parameterName)
        {
            CheckArgumentNull<string>(value, parameterName);
            if (value.Length == 0)
            {
                throw Error.Argument(Strings.Util_EmptyString, parameterName);
            }
        }

        internal static MergeOption CheckEnumerationValue(MergeOption value, string parameterName)
        {
            switch (value)
            {
                case MergeOption.AppendOnly:
                case MergeOption.OverwriteChanges:
                case MergeOption.PreserveChanges:
                case MergeOption.NoTracking:
                    return value;
            }
            throw Error.ArgumentOutOfRange(parameterName);
        }

        internal static DataServiceProtocolVersion CheckEnumerationValue(DataServiceProtocolVersion value, string parameterName)
        {
            switch (value)
            {
                case DataServiceProtocolVersion.V1:
                case DataServiceProtocolVersion.V2:
                case DataServiceProtocolVersion.V3:
                    return value;
            }
            throw Error.ArgumentOutOfRange(parameterName);
        }

        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        internal static object ConstructorInvoke(ConstructorInfo constructor, object[] arguments)
        {
            if (constructor == null)
            {
                throw new MissingMethodException();
            }
            return constructor.Invoke(arguments);
        }

        internal static Uri CreateUri(string value, UriKind kind)
        {
            if (value != null)
            {
                return new Uri(value, kind);
            }
            return null;
        }

        internal static Uri CreateUri(Uri baseUri, Uri requestUri)
        {
            CheckArgumentNull<Uri>(requestUri, "requestUri");
            if (!requestUri.IsAbsoluteUri)
            {
                requestUri = AppendBaseUriAndRelativeUri(baseUri, requestUri);
            }
            return requestUri;
        }

        [Conditional("DEBUG")]
        internal static void DebugInjectFault(string state)
        {
        }

        internal static void Dispose<T>(ref T disposable) where T: class, IDisposable
        {
            Dispose<T>((T) disposable);
            disposable = default(T);
        }

        internal static void Dispose<T>(T disposable) where T: class, IDisposable
        {
            if (disposable != null)
            {
                disposable.Dispose();
            }
        }

        internal static bool DoesNullAttributeSayTrue(XmlReader reader)
        {
            string attribute = reader.GetAttribute("null", "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata");
            return ((attribute != null) && XmlConvert.ToBoolean(attribute));
        }

        internal static Uri ForceNonSlashTerminatedUri(Uri uri)
        {
            string str = CommonUtil.UriToString(uri);
            if (str.EndsWith("/", StringComparison.Ordinal))
            {
                return CreateUri(str.Substring(0, str.Length - 1), UriKind.Absolute);
            }
            return uri;
        }

        internal static Uri ForceSlashTerminatedUri(Uri uri)
        {
            string str = CommonUtil.UriToString(uri);
            if (!str.EndsWith("/", StringComparison.Ordinal))
            {
                return CreateUri(str + "/", UriKind.Absolute);
            }
            return uri;
        }

        internal static IEnumerable<T> GetEnumerable<T>(IEnumerable enumerable, Func<object, T> valueConverter)
        {
            List<T> list = new List<T>();
            foreach (object obj2 in enumerable)
            {
                list.Add(valueConverter(obj2));
            }
            return list;
        }

        internal static Version GetVersionFromMaxProtocolVersion(DataServiceProtocolVersion maxProtocolVersion)
        {
            switch (maxProtocolVersion)
            {
                case DataServiceProtocolVersion.V1:
                    return DataServiceVersion1;

                case DataServiceProtocolVersion.V2:
                    return DataServiceVersion2;

                case DataServiceProtocolVersion.V3:
                    return DataServiceVersion3;
            }
            return DataServiceVersion2;
        }

        internal static char[] GetWhitespaceForTracing(int depth)
        {
            char[] whitespaceForTracing = Util.whitespaceForTracing;
            while (whitespaceForTracing.Length <= depth)
            {
                char[] chArray2 = new char[2 * whitespaceForTracing.Length];
                chArray2[0] = '\r';
                chArray2[1] = '\n';
                for (int i = 2; i < chArray2.Length; i++)
                {
                    chArray2[i] = ' ';
                }
                Interlocked.CompareExchange<char[]>(ref Util.whitespaceForTracing, chArray2, whitespaceForTracing);
                whitespaceForTracing = chArray2;
            }
            return whitespaceForTracing;
        }

        internal static bool IncludeLinkState(EntityStates x)
        {
            if (EntityStates.Modified != x)
            {
                return (EntityStates.Unchanged == x);
            }
            return true;
        }

        internal static bool IsFlagSet(SaveChangesOptions options, SaveChangesOptions flag)
        {
            return ((options & flag) == flag);
        }

        internal static bool IsKnownClientExcption(Exception ex)
        {
            return (((ex is DataServiceClientException) || (ex is DataServiceQueryException)) || (ex is DataServiceRequestException));
        }

        internal static T NullCheck<T>(T value, InternalError errorcode) where T: class
        {
            if (object.ReferenceEquals(value, null))
            {
                Error.ThrowInternalError(errorcode);
            }
            return value;
        }

        internal static void SetNextLinkForCollection(object collection, DataServiceQueryContinuation continuation)
        {
            foreach (PropertyInfo info in collection.GetType().GetPublicProperties(true))
            {
                if ((!(info.Name != "Continuation") && info.CanWrite) && typeof(DataServiceQueryContinuation).IsAssignableFrom(info.PropertyType))
                {
                    info.SetValue(collection, continuation, null);
                }
            }
        }

        internal static Version ToVersion(this DataServiceProtocolVersion protocolVersion)
        {
            switch (protocolVersion)
            {
                case DataServiceProtocolVersion.V1:
                    return DataServiceVersion1;

                case DataServiceProtocolVersion.V2:
                    return DataServiceVersion2;
            }
            return DataServiceVersion3;
        }

        [Conditional("TRACE")]
        internal static void TraceElement(XmlReader reader, TextWriter writer)
        {
            if (writer != null)
            {
                writer.Write(GetWhitespaceForTracing(2 + reader.Depth), 0, 2 + reader.Depth);
                writer.Write("<{0}", reader.Name);
                if (reader.MoveToFirstAttribute())
                {
                    do
                    {
                        writer.Write(" {0}=\"{1}\"", reader.Name, reader.Value);
                    }
                    while (reader.MoveToNextAttribute());
                    reader.MoveToElement();
                }
                writer.Write(reader.IsEmptyElement ? " />" : ">");
            }
        }

        [Conditional("TRACE")]
        internal static void TraceEndElement(XmlReader reader, TextWriter writer, bool indent)
        {
            if (writer != null)
            {
                if (indent)
                {
                    writer.Write(GetWhitespaceForTracing(2 + reader.Depth), 0, 2 + reader.Depth);
                }
                writer.Write("</{0}>", reader.Name);
            }
        }

        [Conditional("TRACE")]
        internal static void TraceText(TextWriter writer, string value)
        {
            if (writer != null)
            {
                writer.Write(value);
            }
        }

        private sealed class ValidatedNotNullAttribute : Attribute
        {
        }
    }
}

