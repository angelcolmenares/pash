namespace System.Management.Automation
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Reflection;

    internal class InternalParameterMetadata
    {
        private Dictionary<string, CompiledCommandParameter> aliasedParameters;
        private Dictionary<string, CompiledCommandParameter> bindableParameters;
        internal static readonly BindingFlags metaDataBindingFlags = (BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        private static ConcurrentDictionary<string, InternalParameterMetadata> ParameterMetadataCache = new ConcurrentDictionary<string, InternalParameterMetadata>(StringComparer.Ordinal);
        private Type type;
        private string typeName;

        internal InternalParameterMetadata(Type type, bool processingDynamicParameters)
        {
            this.typeName = string.Empty;
            this.bindableParameters = new Dictionary<string, CompiledCommandParameter>(StringComparer.OrdinalIgnoreCase);
            this.aliasedParameters = new Dictionary<string, CompiledCommandParameter>(StringComparer.OrdinalIgnoreCase);
            if (type == null)
            {
                throw PSTraceSource.NewArgumentNullException("type");
            }
            this.type = type;
            this.typeName = type.Name;
            this.ConstructCompiledParametersUsingReflection(processingDynamicParameters);
        }

        internal InternalParameterMetadata(RuntimeDefinedParameterDictionary runtimeDefinedParameters, bool processingDynamicParameters, bool checkNames)
        {
            this.typeName = string.Empty;
            this.bindableParameters = new Dictionary<string, CompiledCommandParameter>(StringComparer.OrdinalIgnoreCase);
            this.aliasedParameters = new Dictionary<string, CompiledCommandParameter>(StringComparer.OrdinalIgnoreCase);
            if (runtimeDefinedParameters == null)
            {
                throw PSTraceSource.NewArgumentNullException("runtimeDefinedParameters");
            }
            this.ConstructCompiledParametersUsingRuntimeDefinedParameters(runtimeDefinedParameters, processingDynamicParameters, checkNames);
        }

        private void AddParameter(CompiledCommandParameter parameter, bool checkNames)
        {
            if (checkNames)
            {
                this.CheckForReservedParameter(parameter.Name);
            }
            this.bindableParameters.Add(parameter.Name, parameter);
            foreach (string str in parameter.Aliases)
            {
                if (this.aliasedParameters.ContainsKey(str))
                {
                    throw new System.Management.Automation.MetadataException("AliasDeclaredMultipleTimes", null, DiscoveryExceptions.AliasDeclaredMultipleTimes, new object[] { str });
                }
                this.aliasedParameters.Add(str, parameter);
            }
        }

        private void AddParameter(MemberInfo member, bool processingDynamicParameters)
        {
            bool flag = false;
            bool flag2 = false;
            this.CheckForReservedParameter(member.Name);
            if (this.bindableParameters.ContainsKey(member.Name))
            {
                CompiledCommandParameter parameter = this.bindableParameters[member.Name];
                Type declaringType = parameter.DeclaringType;
                if (declaringType == null)
                {
                    flag = true;
                }
                else if (declaringType.IsSubclassOf(member.DeclaringType))
                {
                    flag2 = true;
                }
                else if (member.DeclaringType.IsSubclassOf(declaringType))
                {
                    this.RemoveParameter(parameter);
                }
                else
                {
                    flag = true;
                }
            }
            if (flag)
            {
                throw new System.Management.Automation.MetadataException("DuplicateParameterDefinition", null, ParameterBinderStrings.DuplicateParameterDefinition, new object[] { member.Name });
            }
            if (!flag2)
            {
                CompiledCommandParameter parameter2 = new CompiledCommandParameter(member, processingDynamicParameters);
                this.AddParameter(parameter2, true);
            }
        }

        private void CheckForReservedParameter(string name)
        {
            if (name.Equals("SelectProperty", StringComparison.OrdinalIgnoreCase) || name.Equals("SelectObject", StringComparison.OrdinalIgnoreCase))
            {
                throw new System.Management.Automation.MetadataException("ReservedParameterName", null, DiscoveryExceptions.ReservedParameterName, new object[] { name });
            }
        }

        private void ConstructCompiledParametersUsingReflection(bool processingDynamicParameters)
        {
            PropertyInfo[] properties = this.type.GetProperties(metaDataBindingFlags);
            FieldInfo[] fields = this.type.GetFields(metaDataBindingFlags);
            foreach (PropertyInfo info in properties)
            {
                if (IsMemberAParameter(info))
                {
                    this.AddParameter(info, processingDynamicParameters);
                }
            }
            foreach (FieldInfo info2 in fields)
            {
                if (IsMemberAParameter(info2))
                {
                    this.AddParameter(info2, processingDynamicParameters);
                }
            }
        }

        private void ConstructCompiledParametersUsingRuntimeDefinedParameters(RuntimeDefinedParameterDictionary runtimeDefinedParameters, bool processingDynamicParameters, bool checkNames)
        {
            foreach (RuntimeDefinedParameter parameter in runtimeDefinedParameters.Values)
            {
                if (parameter != null)
                {
                    CompiledCommandParameter parameter2 = new CompiledCommandParameter(parameter, processingDynamicParameters);
                    this.AddParameter(parameter2, checkNames);
                }
            }
        }

        internal static InternalParameterMetadata Get(RuntimeDefinedParameterDictionary runtimeDefinedParameters, bool processingDynamicParameters, bool checkNames)
        {
            if (runtimeDefinedParameters == null)
            {
                throw PSTraceSource.NewArgumentNullException("runtimeDefinedParameter");
            }
            return new InternalParameterMetadata(runtimeDefinedParameters, processingDynamicParameters, checkNames);
        }

        internal static InternalParameterMetadata Get(Type type, ExecutionContext context, bool processingDynamicParameters)
        {
            if (type == null)
            {
                throw PSTraceSource.NewArgumentNullException("type");
            }
            InternalParameterMetadata metadata = null;
            if ((context != null) && ParameterMetadataCache.ContainsKey(type.AssemblyQualifiedName))
            {
                return ParameterMetadataCache[type.AssemblyQualifiedName];
            }
            metadata = new InternalParameterMetadata(type, processingDynamicParameters);
            if (context != null)
            {
                ParameterMetadataCache.TryAdd(type.AssemblyQualifiedName, metadata);
            }
            return metadata;
        }

        private static bool IsMemberAParameter(MemberInfo member)
        {
            object[] customAttributes;
            bool flag = false;
            try
            {
                customAttributes = member.GetCustomAttributes(typeof(ParameterAttribute), false);
            }
            catch (System.Management.Automation.MetadataException exception)
            {
                throw new System.Management.Automation.MetadataException("GetCustomAttributesMetadataException", exception, Metadata.MetadataMemberInitialization, new object[] { member.Name, exception.Message });
            }
            catch (ArgumentException exception2)
            {
                throw new System.Management.Automation.MetadataException("GetCustomAttributesArgumentException", exception2, Metadata.MetadataMemberInitialization, new object[] { member.Name, exception2.Message });
            }
            if ((customAttributes != null) && (customAttributes.Length > 0))
            {
                flag = true;
            }
            return flag;
        }

        private void RemoveParameter(CompiledCommandParameter parameter)
        {
            this.bindableParameters.Remove(parameter.Name);
            foreach (string str in parameter.Aliases)
            {
                this.aliasedParameters.Remove(str);
            }
        }

        internal Dictionary<string, CompiledCommandParameter> AliasedParameters
        {
            get
            {
                return this.aliasedParameters;
            }
        }

        internal Dictionary<string, CompiledCommandParameter> BindableParameters
        {
            get
            {
                return this.bindableParameters;
            }
        }

        internal string TypeName
        {
            get
            {
                return this.typeName;
            }
        }
    }
}

