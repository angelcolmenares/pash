namespace Microsoft.PowerShell.Cmdletization.Cim
{
    using Microsoft.Management.Infrastructure;
    using Microsoft.Management.Infrastructure.Options;
    using Microsoft.PowerShell.Cim;
    using Microsoft.PowerShell.Cmdletization;
    using Microsoft.PowerShell.Commands.Management;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Management.Automation;

    internal abstract class MethodInvocationJobBase<T> : CimChildJobBase<T>
    {
        private readonly MethodInvocationInfo _methodInvocationInfo;
        private readonly string _methodSubject;
        private readonly bool _passThru;
        private const string CustomOperationOptionPrefix = "cim:operationOption:";

        internal MethodInvocationJobBase(CimJobContext jobContext, bool passThru, string methodSubject, MethodInvocationInfo methodInvocationInfo)
            : base(jobContext)
        {
            this._passThru = passThru;
            this._methodSubject = methodSubject;
            this._methodInvocationInfo = methodInvocationInfo;
        }

        internal override CimCustomOptionsDictionary CalculateJobSpecificCustomOptions()
        {
            IDictionary<string, object> wrappedDictionary = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            foreach (MethodParameter parameter in this.GetMethodInputParametersCore(p => p.Name.StartsWith("cim:operationOption:", StringComparison.OrdinalIgnoreCase)))
            {
                if (parameter.Value != null)
                {
                    wrappedDictionary.Add(parameter.Name.Substring("cim:operationOption:".Length), parameter.Value);
                }
            }
            return CimCustomOptionsDictionary.Create(wrappedDictionary);
        }

        internal IEnumerable<CimInstance> GetCimInstancesFromArguments()
        {
            return this._methodInvocationInfo.GetArgumentsOfType<CimInstance>();
        }

        internal IEnumerable<MethodParameter> GetMethodInputParameters()
        {
            return (from p in this.GetMethodInputParametersCore(p => !p.Name.StartsWith("cim:operationOption:", StringComparison.OrdinalIgnoreCase))
                    where p.IsValuePresent
                    select p);
        }

        private IEnumerable<MethodParameter> GetMethodInputParametersCore(Func<MethodParameter, bool> filter)
        {
            IEnumerable<MethodParameter> enumerable = this._methodInvocationInfo.Parameters.Where<MethodParameter>(filter);
            List<MethodParameter> list = new List<MethodParameter>();
            foreach (MethodParameter parameter in enumerable)
            {
                object obj2 = CimValueConverter.ConvertFromDotNetToCim(parameter.Value);
                Type cimType = CimValueConverter.GetCimType(parameter.ParameterType);
                MethodParameter item = new MethodParameter
                {
                    Name = parameter.Name,
                    ParameterType = cimType,
                    Bindings = parameter.Bindings,
                    Value = obj2,
                    IsValuePresent = parameter.IsValuePresent
                };
                list.Add(item);
            }
            return list;
        }

        internal IEnumerable<MethodParameter> GetMethodOutputParameters()
        {
            IEnumerable<MethodParameter> parameters = this._methodInvocationInfo.Parameters;
            if (this._methodInvocationInfo.ReturnValue != null)
            {
                parameters = parameters.Append<MethodParameter>(this._methodInvocationInfo.ReturnValue);
            }
            return (from p in parameters
                    where 0 != (p.Bindings & (MethodParameterBindings.Error | MethodParameterBindings.Out))
                    select p);
        }

        internal bool IsPassThruObjectNeeded()
        {
            return ((this._passThru && !base.DidUserSuppressTheOperation) && !base.JobHadErrors);
        }

        public override void OnCompleted()
        {
            base.ExceptionSafeWrapper(delegate
            {
                if (this.IsPassThruObjectNeeded())
                {
                    object outputObject = this.PassThruObject;
                    if (outputObject != null)
                    {
                        this.WriteObject(outputObject);
                    }
                }
            });
            base.OnCompleted();
        }

        internal bool ShouldProcess()
        {
            bool flag;
            if (!base.JobContext.CmdletInvocationContext.CmdletDefinitionContext.ClientSideShouldProcess)
            {
                return true;
            }
            if (!base.JobContext.SupportsShouldProcess)
            {
                flag = true;
                base.WriteVerboseStartOfCimOperation();
            }
            else
            {
                string methodSubject = this.MethodSubject;
                string methodName = this.MethodName;
                switch (base.ShouldProcess(methodSubject, methodName))
                {
                    case CimResponseType.Yes:
                    case CimResponseType.YesToAll:
                        flag = true;
                        goto Label_0067;
                }
                flag = false;
            }
        Label_0067:
            if (!flag)
            {
                base.SetCompletedJobState(JobState.Completed, null);
            }
            return flag;
        }

        internal override string Description
        {
            get
            {
                return string.Format(CultureInfo.InvariantCulture, CmdletizationResources.CimJob_MethodDescription, new object[] { this.MethodSubject, this.MethodName });
            }
        }

        internal override string FailSafeDescription
        {
            get
            {
                return string.Format(CultureInfo.InvariantCulture, CmdletizationResources.CimJob_SafeMethodDescription, new object[] { base.JobContext.CmdletizationClassName, base.JobContext.Session.ComputerName, this.MethodName });
            }
        }

        internal string MethodName
        {
            get
            {
                return this._methodInvocationInfo.MethodName;
            }
        }

        internal string MethodSubject
        {
            get
            {
                return this._methodSubject;
            }
        }

        internal abstract object PassThruObject { get; }
    }
}

