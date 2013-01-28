namespace System.Management.Automation
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Text;

    internal class MergedCommandParameterMetadata
    {
        private string _defaultParameterSetName;
        private IDictionary<string, MergedCompiledCommandParameter> aliasedParameters = new Dictionary<string, MergedCompiledCommandParameter>(StringComparer.OrdinalIgnoreCase);
        private IDictionary<string, MergedCompiledCommandParameter> bindableParameters = new Dictionary<string, MergedCompiledCommandParameter>(StringComparer.OrdinalIgnoreCase);
        private static Func<IDictionary<string, MergedCompiledCommandParameter>, IDictionary<string, MergedCompiledCommandParameter>> MakeReadOnlyHelper;
        private static CompareInfo nameCompareInfo = CompareInfo.GetCompareInfo(CultureInfo.InvariantCulture.LCID);
        private int nextAvailableParameterSetIndex;
        private IList<string> parameterSetMap = new Collection<string>();

        static MergedCommandParameterMetadata()
        {
            Type type = Type.GetType("System.Collections.ObjectModel.ReadOnlyDictionary`2");
            if (type == null)
            {
                MakeReadOnlyHelper = dictionary => dictionary;
            }
            else
            {
                Type type2 = type.MakeGenericType(new Type[] { typeof(string), typeof(MergedCompiledCommandParameter) });
                Type type3 = typeof(IDictionary<string, MergedCompiledCommandParameter>);
                ConstructorInfo constructor = type2.GetConstructor(new Type[] { type3 });
                ParameterExpression expression = Expression.Parameter(type3);
                MakeReadOnlyHelper = Expression.Lambda<Func<IDictionary<string, MergedCompiledCommandParameter>, IDictionary<string, MergedCompiledCommandParameter>>>(Expression.Convert(Expression.New(constructor, new Expression[] { expression }), type3), new ParameterExpression[] { expression }).Compile();
            }
        }

        internal Collection<MergedCompiledCommandParameter> AddMetadataForBinder(InternalParameterMetadata parameterMetadata, ParameterBinderAssociation binderAssociation)
        {
            if (parameterMetadata == null)
            {
                throw PSTraceSource.NewArgumentNullException("parameterMetadata");
            }
            Collection<MergedCompiledCommandParameter> collection = new Collection<MergedCompiledCommandParameter>();
            foreach (KeyValuePair<string, CompiledCommandParameter> pair in parameterMetadata.BindableParameters)
            {
                if (this.bindableParameters.ContainsKey(pair.Key))
                {
                    System.Management.Automation.MetadataException exception = new System.Management.Automation.MetadataException("ParameterNameAlreadyExistsForCommand", null, Metadata.ParameterNameAlreadyExistsForCommand, new object[] { pair.Key });
                    throw exception;
                }
                if (this.aliasedParameters.ContainsKey(pair.Key))
                {
                    System.Management.Automation.MetadataException exception2 = new System.Management.Automation.MetadataException("ParameterNameConflictsWithAlias", null, Metadata.ParameterNameConflictsWithAlias, new object[] { pair.Key, RetrieveParameterNameForAlias(pair.Key, this.aliasedParameters) });
                    throw exception2;
                }
                MergedCompiledCommandParameter parameter = new MergedCompiledCommandParameter(pair.Value, binderAssociation);
                this.bindableParameters.Add(pair.Key, parameter);
                collection.Add(parameter);
                foreach (string str in pair.Value.Aliases)
                {
                    if (this.aliasedParameters.ContainsKey(str))
                    {
                        System.Management.Automation.MetadataException exception3 = new System.Management.Automation.MetadataException("AliasParameterNameAlreadyExistsForCommand", null, Metadata.AliasParameterNameAlreadyExistsForCommand, new object[] { str });
                        throw exception3;
                    }
                    if (this.bindableParameters.ContainsKey(str))
                    {
                        System.Management.Automation.MetadataException exception4 = new System.Management.Automation.MetadataException("ParameterNameConflictsWithAlias", null, Metadata.ParameterNameConflictsWithAlias, new object[] { RetrieveParameterNameForAlias(str, this.bindableParameters), pair.Value.Name });
                        throw exception4;
                    }
                    this.aliasedParameters.Add(str, parameter);
                }
            }
            return collection;
        }

        private int AddParameterSetToMap(string parameterSetName)
        {
            int index = -1;
            if (!string.IsNullOrEmpty(parameterSetName))
            {
                index = this.parameterSetMap.IndexOf(parameterSetName);
                if (index != -1)
                {
                    return index;
                }
                if (this.nextAvailableParameterSetIndex == int.MaxValue)
                {
                    ParsingMetadataException exception = new ParsingMetadataException("ParsingTooManyParameterSets", null, Metadata.ParsingTooManyParameterSets, new object[0]);
                    throw exception;
                }
                this.parameterSetMap.Add(parameterSetName);
                index = this.parameterSetMap.IndexOf(parameterSetName);
                this.nextAvailableParameterSetIndex++;
            }
            return index;
        }

        internal int GenerateParameterSetMappingFromMetadata(string defaultParameterSetName)
        {
            this.parameterSetMap.Clear();
            this.nextAvailableParameterSetIndex = 0;
            int num = 0;
            if (!string.IsNullOrEmpty(defaultParameterSetName))
            {
                this._defaultParameterSetName = defaultParameterSetName;
                int num2 = this.AddParameterSetToMap(defaultParameterSetName);
                num = ((int) 1) << num2;
            }
            foreach (MergedCompiledCommandParameter parameter in this.BindableParameters.Values)
            {
                int num3 = 0;
                foreach (KeyValuePair<string, ParameterSetSpecificMetadata> pair in parameter.Parameter.ParameterSetData)
                {
                    string key = pair.Key;
                    ParameterSetSpecificMetadata metadata = pair.Value;
                    if (string.Equals(key, "__AllParameterSets", StringComparison.OrdinalIgnoreCase))
                    {
                        metadata.ParameterSetFlag = 0;
                        metadata.IsInAllSets = true;
                        parameter.Parameter.IsInAllSets = true;
                    }
                    else
                    {
                        int num4 = this.AddParameterSetToMap(key);
                        int num5 = ((int) 1) << num4;
                        num3 |= num5;
                        metadata.ParameterSetFlag = num5;
                    }
                }
                parameter.Parameter.ParameterSetFlags = num3;
            }
            return num;
        }

        internal MergedCompiledCommandParameter GetMatchingParameter(string name, bool throwOnParameterNotFound, bool tryExactMatching, InvocationInfo invocationInfo)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw PSTraceSource.NewArgumentException("name");
            }
            Collection<MergedCompiledCommandParameter> collection = new Collection<MergedCompiledCommandParameter>();
            if ((name.Length > 0) && SpecialCharacters.IsDash(name[0]))
            {
                name = name.Substring(1);
            }
            foreach (string str in this.bindableParameters.Keys)
            {
                if (nameCompareInfo.IsPrefix(str, name, CompareOptions.IgnoreCase))
                {
                    if (tryExactMatching && string.Equals(str, name, StringComparison.OrdinalIgnoreCase))
                    {
                        return this.bindableParameters[str];
                    }
                    collection.Add(this.bindableParameters[str]);
                }
            }
            foreach (string str2 in this.aliasedParameters.Keys)
            {
                if (nameCompareInfo.IsPrefix(str2, name, CompareOptions.IgnoreCase))
                {
                    if (tryExactMatching && string.Equals(str2, name, StringComparison.OrdinalIgnoreCase))
                    {
                        return this.aliasedParameters[str2];
                    }
                    if (!collection.Contains(this.aliasedParameters[str2]))
                    {
                        collection.Add(this.aliasedParameters[str2]);
                    }
                }
            }
            if (collection.Count > 1)
            {
                StringBuilder builder = new StringBuilder();
                foreach (MergedCompiledCommandParameter parameter in collection)
                {
                    builder.AppendFormat(" -{0}", parameter.Parameter.Name);
                }
                ParameterBindingException exception = new ParameterBindingException(ErrorCategory.InvalidArgument, invocationInfo, null, name, null, null, "ParameterBinderStrings", "AmbiguousParameter", new object[] { builder });
                throw exception;
            }
            if ((collection.Count == 0) && throwOnParameterNotFound)
            {
                ParameterBindingException exception2 = new ParameterBindingException(ErrorCategory.InvalidArgument, invocationInfo, null, name, null, null, "ParameterBinderStrings", "NamedParameterNotFound", new object[0]);
                throw exception2;
            }
            MergedCompiledCommandParameter parameter2 = null;
            if (collection.Count > 0)
            {
                parameter2 = collection[0];
            }
            return parameter2;
        }

        internal string GetParameterSetName(int parameterSet)
        {
            string str = this._defaultParameterSetName;
            if (string.IsNullOrEmpty(str))
            {
                str = "__AllParameterSets";
            }
            if ((parameterSet == int.MaxValue) || (parameterSet == 0))
            {
                return str;
            }
            int num = 0;
            while (((parameterSet >> num) & 1) == 0)
            {
                num++;
            }
            if (((parameterSet >> (num + 1)) & 1) == 0)
            {
                if (num < this.parameterSetMap.Count)
                {
                    return this.parameterSetMap[num];
                }
                return string.Empty;
            }
            return string.Empty;
        }

        internal Collection<MergedCompiledCommandParameter> GetParametersInParameterSet(int parameterSetFlag)
        {
            Collection<MergedCompiledCommandParameter> collection = new Collection<MergedCompiledCommandParameter>();
            foreach (MergedCompiledCommandParameter parameter in this.BindableParameters.Values)
            {
                if (((parameterSetFlag & parameter.Parameter.ParameterSetFlags) != 0) || parameter.Parameter.IsInAllSets)
                {
                    collection.Add(parameter);
                }
            }
            return collection;
        }

        internal void MakeReadOnly()
        {
            this.bindableParameters = MakeReadOnlyHelper(this.bindableParameters);
            this.aliasedParameters = MakeReadOnlyHelper(this.aliasedParameters);
            this.parameterSetMap = new ReadOnlyCollection<string>(this.parameterSetMap);
        }

        internal ICollection<MergedCompiledCommandParameter> ReplaceMetadata(MergedCommandParameterMetadata metadata)
        {
            ICollection<MergedCompiledCommandParameter> is2 = new Collection<MergedCompiledCommandParameter>();
            this.bindableParameters.Clear();
            foreach (KeyValuePair<string, MergedCompiledCommandParameter> pair in metadata.BindableParameters)
            {
                this.bindableParameters.Add(pair.Key, pair.Value);
                is2.Add(pair.Value);
            }
            this.aliasedParameters.Clear();
            foreach (KeyValuePair<string, MergedCompiledCommandParameter> pair2 in metadata.AliasedParameters)
            {
                this.aliasedParameters.Add(pair2.Key, pair2.Value);
            }
            return is2;
        }

        private static string RetrieveParameterNameForAlias(string key, IDictionary<string, MergedCompiledCommandParameter> dict)
        {
            MergedCompiledCommandParameter parameter = dict[key];
            if (parameter != null)
            {
                CompiledCommandParameter parameter2 = parameter.Parameter;
                if ((parameter2 != null) && !string.IsNullOrEmpty(parameter2.Name))
                {
                    return parameter2.Name;
                }
            }
            return string.Empty;
        }

        internal IDictionary<string, MergedCompiledCommandParameter> AliasedParameters
        {
            get
            {
                return this.aliasedParameters;
            }
        }

        internal int AllParameterSetFlags
        {
            get
            {
                return (int) ((((int) 1) << this.ParameterSetCount) - 1);
            }
        }

        internal IDictionary<string, MergedCompiledCommandParameter> BindableParameters
        {
            get
            {
                return this.bindableParameters;
            }
        }

        internal int ParameterSetCount
        {
            get
            {
                return this.parameterSetMap.Count;
            }
        }
    }
}

