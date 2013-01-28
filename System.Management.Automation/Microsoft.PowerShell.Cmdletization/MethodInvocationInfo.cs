namespace Microsoft.PowerShell.Cmdletization
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    public sealed class MethodInvocationInfo
    {
        private readonly string methodName;
        private readonly KeyedCollection<string, MethodParameter> parameters;
        private readonly MethodParameter returnValue;

        public MethodInvocationInfo(string name, IEnumerable<MethodParameter> parameters, MethodParameter returnValue)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (parameters == null)
            {
                throw new ArgumentNullException("parameters");
            }
            this.methodName = name;
            this.returnValue = returnValue;
            KeyedCollection<string, MethodParameter> keyeds = new MethodParametersCollection();
            foreach (MethodParameter parameter in parameters)
            {
                keyeds.Add(parameter);
            }
            this.parameters = keyeds;
        }

        internal IEnumerable<T> GetArgumentsOfType<T>() where T: class
        {
            List<T> list = new List<T>();
            foreach (MethodParameter parameter in this.Parameters)
            {
                if (MethodParameterBindings.In == (parameter.Bindings & MethodParameterBindings.In))
                {
                    T item = parameter.Value as T;
                    if (item != null)
                    {
                        list.Add(item);
                    }
                    else
                    {
                        IEnumerable enumerable = parameter.Value as IEnumerable;
                        if (enumerable != null)
                        {
                            foreach (object obj2 in enumerable)
                            {
                                T local2 = obj2 as T;
                                if (local2 != null)
                                {
                                    list.Add(local2);
                                }
                            }
                        }
                    }
                }
            }
            return list;
        }

        public string MethodName
        {
            get
            {
                return this.methodName;
            }
        }

        public KeyedCollection<string, MethodParameter> Parameters
        {
            get
            {
                return this.parameters;
            }
        }

        public MethodParameter ReturnValue
        {
            get
            {
                return this.returnValue;
            }
        }
    }
}

