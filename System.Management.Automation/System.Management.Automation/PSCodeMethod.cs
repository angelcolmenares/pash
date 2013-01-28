namespace System.Management.Automation
{
    using System;
    using System.Collections.ObjectModel;
    using System.Reflection;
    using System.Text;

    public class PSCodeMethod : PSMethodInfo
    {
        private MethodInfo[] codeReference;
        private string[] codeReferenceDefinition;
        private MethodInformation[] codeReferenceMethodInformation;

        internal PSCodeMethod(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw PSTraceSource.NewArgumentException("name");
            }
            base.name = name;
        }

        public PSCodeMethod(string name, MethodInfo codeReference)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw PSTraceSource.NewArgumentException("name");
            }
            if (codeReference == null)
            {
                throw PSTraceSource.NewArgumentNullException("codeReference");
            }
            CheckMethodInfo(codeReference);
            base.name = name;
            this.codeReference = new MethodInfo[] { codeReference };
            this.codeReferenceDefinition = new string[] { DotNetAdapter.GetMethodInfoOverloadDefinition(null, this.codeReference[0], 0) };
            this.codeReferenceMethodInformation = DotNetAdapter.GetMethodInformationArray(this.codeReference);
        }

        private static void CheckMethodInfo(MethodInfo method)
        {
            ParameterInfo[] parameters = method.GetParameters();
            if ((!method.IsStatic || !method.IsPublic) || ((parameters.Length == 0) || !parameters[0].ParameterType.Equals(typeof(PSObject))))
            {
                throw new ExtendedTypeSystemException("WrongMethodFormat", null, ExtendedTypeSystem.CodeMethodMethodFormat, new object[0]);
            }
        }

        public override PSMemberInfo Copy()
        {
            PSCodeMethod destiny = new PSCodeMethod(base.name, this.codeReference[0]);
            base.CloneBaseProperties(destiny);
            return destiny;
        }

        public override object Invoke(params object[] arguments)
        {
            object[] objArray2;
            if (arguments == null)
            {
                throw PSTraceSource.NewArgumentNullException("arguments");
            }
            object[] objArray = new object[arguments.Length + 1];
            objArray[0] = base.instance;
            for (int i = 0; i < arguments.Length; i++)
            {
                objArray[i + 1] = arguments[i];
            }
            Adapter.GetBestMethodAndArguments(this.codeReference[0].Name, this.codeReferenceMethodInformation, objArray, out objArray2);
            return DotNetAdapter.AuxiliaryMethodInvoke(null, objArray2, this.codeReferenceMethodInformation[0], objArray);
        }

        internal void SetCodeReference(Type type, string methodName)
        {
            MemberInfo[] infoArray = type.GetMember(methodName, MemberTypes.Method, BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase);
            if (infoArray.Length != 1)
            {
                throw new ExtendedTypeSystemException("WrongMethodFormatFromTypeTable", null, ExtendedTypeSystem.CodeMethodMethodFormat, new object[0]);
            }
            this.codeReference = new MethodInfo[] { (MethodInfo) infoArray[0] };
            this.codeReferenceDefinition = new string[] { DotNetAdapter.GetMethodInfoOverloadDefinition(null, this.codeReference[0], 0) };
            this.codeReferenceMethodInformation = DotNetAdapter.GetMethodInformationArray(this.codeReference);
            CheckMethodInfo(this.codeReference[0]);
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            foreach (string str in this.OverloadDefinitions)
            {
                builder.Append(str);
                builder.Append(", ");
            }
            builder.Remove(builder.Length - 2, 2);
            return builder.ToString();
        }

        public MethodInfo CodeReference
        {
            get
            {
                return this.codeReference[0];
            }
        }

        public override PSMemberTypes MemberType
        {
            get
            {
                return PSMemberTypes.CodeMethod;
            }
        }

        public override Collection<string> OverloadDefinitions
        {
            get
            {
                return new Collection<string> { this.codeReferenceDefinition[0] };
            }
        }

        public override string TypeNameOfValue
        {
            get
            {
                return typeof(PSCodeMethod).FullName;
            }
        }
    }
}

