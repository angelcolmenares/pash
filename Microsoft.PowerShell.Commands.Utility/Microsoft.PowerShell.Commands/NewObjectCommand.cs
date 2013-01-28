namespace Microsoft.PowerShell.Commands
{
    using Microsoft.PowerShell.Commands.Utility;
    using System;
    using System.Collections;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Security;
    using System.Management.Automation.Sqm;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Threading;

    [Cmdlet("New", "Object", DefaultParameterSetName="Net", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113355")]
    public sealed class NewObjectCommand : PSCmdlet
    {
        private object[] arguments;
        private string comObject;
        private Guid comObjectClsId = Guid.Empty;
        private ComCreateInfo createInfo;
        private const string netSetName = "Net";
        private IDictionary property;
        private SwitchParameter strict;
        private const string stringsBaseName = "NewObjectStrings";
        private string typeName;

        protected override void BeginProcessing()
        {
            Type result = null;
            PSArgumentException exception = null;
            if (string.Compare(base.ParameterSetName, "Net", StringComparison.Ordinal) == 0)
            {
                object o = null;
                if (!LanguagePrimitives.TryConvertTo<Type>(this.typeName, out result))
                {
                    exception = PSTraceSource.NewArgumentException("TypeName", "NewObjectStrings", "TypeNotFound", new object[] { this.typeName });
                    base.ThrowTerminatingError(new ErrorRecord(exception, "TypeNotFound", ErrorCategory.InvalidType, null));
                }
                if ((base.Context.LanguageMode == PSLanguageMode.ConstrainedLanguage) && !CoreTypes.Contains(result))
                {
                    base.ThrowTerminatingError(new ErrorRecord(new PSNotSupportedException(NewObjectStrings.CannotCreateTypeConstrainedLanguage), "CannotCreateTypeConstrainedLanguage", ErrorCategory.PermissionDenied, null));
                }
                if (WinRTHelper.IsWinRTType(result) && (typeof(Attribute).IsAssignableFrom(result) || typeof(Delegate).IsAssignableFrom(result)))
                {
                    base.ThrowTerminatingError(new ErrorRecord(new InvalidOperationException(NewObjectStrings.CannotInstantiateWinRTType), "CannotInstantiateWinRTType", ErrorCategory.InvalidOperation, null));
                }
                if ((this.arguments == null) || (this.arguments.Length == 0))
                {
                    ConstructorInfo constructor = result.GetConstructor(Type.EmptyTypes);
                    if ((constructor != null) && constructor.IsPublic)
                    {
                        o = this.CallConstructor(result, new ConstructorInfo[] { constructor }, new object[0]);
                        if ((o != null) && (this.property != null))
                        {
                            o = LanguagePrimitives.SetObjectProperties(o, this.Property, result, new LanguagePrimitives.MemberNotFoundError(this.CreateMemberNotFoundError), new LanguagePrimitives.MemberSetValueError(this.CreateMemberSetValueError), true);
                        }
                        base.WriteObject(o);
                        return;
                    }
                    if (result.IsValueType)
                    {
                        try
                        {
                            o = Activator.CreateInstance(result, false);
                            if ((o != null) && (this.property != null))
                            {
                                o = LanguagePrimitives.SetObjectProperties(o, this.Property, result, new LanguagePrimitives.MemberNotFoundError(this.CreateMemberNotFoundError), new LanguagePrimitives.MemberSetValueError(this.CreateMemberSetValueError), true);
                            }
                        }
                        catch (TargetInvocationException exception2)
                        {
                            base.ThrowTerminatingError(new ErrorRecord((exception2.InnerException == null) ? exception2 : exception2.InnerException, "ConstructorCalledThrowException", ErrorCategory.InvalidOperation, null));
                        }
                        base.WriteObject(o);
                        return;
                    }
                }
                else
                {
                    ConstructorInfo[] constructors = result.GetConstructors();
                    if (constructors.Length != 0)
                    {
                        o = this.CallConstructor(result, constructors, this.arguments);
                        if ((o != null) && (this.property != null))
                        {
                            o = LanguagePrimitives.SetObjectProperties(o, this.Property, result, new LanguagePrimitives.MemberNotFoundError(this.CreateMemberNotFoundError), new LanguagePrimitives.MemberSetValueError(this.CreateMemberSetValueError), true);
                        }
                        base.WriteObject(o);
                        return;
                    }
                }
                exception = PSTraceSource.NewArgumentException("TypeName", "NewObjectStrings", "CannotFindAppropriateCtor", new object[] { this.typeName });
                base.ThrowTerminatingError(new ErrorRecord(exception, "CannotFindAppropriateCtor", ErrorCategory.ObjectNotFound, null));
            }
            else
            {
                NewObjectNativeMethods.CLSIDFromProgID(this.comObject, out this.comObjectClsId);
                if (base.Context.LanguageMode == PSLanguageMode.ConstrainedLanguage)
                {
                    bool flag2 = false;
                    if ((SystemPolicy.GetSystemLockdownPolicy() == SystemEnforcementMode.Enforce) && SystemPolicy.IsClassInApprovedList(this.comObjectClsId))
                    {
                        flag2 = true;
                    }
                    if (!flag2)
                    {
                        base.ThrowTerminatingError(new ErrorRecord(new PSNotSupportedException(NewObjectStrings.CannotCreateTypeConstrainedLanguage), "CannotCreateComTypeConstrainedLanguage", ErrorCategory.PermissionDenied, null));
                        return;
                    }
                }
                PSSQMAPI.IncrementDataPoint((int) 0x2099);
                object targetObject = this.CreateComObject();
                string fullName = targetObject.GetType().FullName;
                if (!fullName.Equals("System.__ComObject"))
                {
                    exception = PSTraceSource.NewArgumentException("TypeName", "NewObjectStrings", "ComInteropLoaded", new object[] { fullName });
                    base.WriteVerbose(exception.Message);
                    if (this.Strict != 0)
                    {
                        base.WriteError(new ErrorRecord(exception, "ComInteropLoaded", ErrorCategory.InvalidArgument, targetObject));
                    }
                }
                if ((targetObject != null) && (this.property != null))
                {
                    targetObject = LanguagePrimitives.SetObjectProperties(targetObject, this.Property, result, new LanguagePrimitives.MemberNotFoundError(this.CreateMemberNotFoundError), new LanguagePrimitives.MemberSetValueError(this.CreateMemberSetValueError), true);
                }
                base.WriteObject(targetObject);
            }
        }

        private object CallConstructor(Type type, ConstructorInfo[] constructors, object[] args)
        {
            object obj2 = null;
            try
            {
                obj2 = DotNetAdapter.ConstructorInvokeDotNet(type, constructors, args);
            }
            catch (MethodException exception)
            {
                base.ThrowTerminatingError(new ErrorRecord(exception, "ConstructorInvokedThrowException", ErrorCategory.InvalidOperation, null));
            }
            return obj2;
        }

        private object CreateComObject()
        {
            Type t = null;
            PSArgumentException exception = null;
            try
            {
                t = Type.GetTypeFromCLSID(this.comObjectClsId);
                if (t == null)
                {
                    exception = PSTraceSource.NewArgumentException("ComObject", "NewObjectStrings", "CannotLoadComObjectType", new object[] { this.comObject });
                    base.ThrowTerminatingError(new ErrorRecord(exception, "CannotLoadComObjectType", ErrorCategory.InvalidType, null));
                }
                return this.SafeCreateInstance(t, this.arguments);
            }
            catch (COMException exception2)
            {
                if (exception2.ErrorCode == -2147417850)
                {
                    this.createInfo = new ComCreateInfo();
                    Thread thread = new Thread(new ParameterizedThreadStart(this.STAComCreateThreadProc));
                    thread.SetApartmentState(ApartmentState.STA);
                    thread.Start(this.createInfo);
                    thread.Join();
                    if (this.createInfo.success)
                    {
                        return this.createInfo.objectCreated;
                    }
                    base.ThrowTerminatingError(new ErrorRecord(this.createInfo.e, "NoCOMClassIdentified", ErrorCategory.ResourceUnavailable, null));
                    return null;
                }
                base.ThrowTerminatingError(new ErrorRecord(exception2, "NoCOMClassIdentified", ErrorCategory.ResourceUnavailable, null));
                return null;
            }
        }

        private void CreateMemberNotFoundError(PSObject pso, DictionaryEntry property, Type resultType)
        {
            object[] o = new object[3];
            o[1] = property.Key.ToString();
            o[2] = ParameterSet2ResourceString(base.ParameterSetName);
            string message = StringUtil.Format(NewObjectStrings.MemberNotFound, o);
            base.ThrowTerminatingError(new ErrorRecord(new InvalidOperationException(message), "InvalidOperationException", ErrorCategory.InvalidOperation, null));
        }

        private void CreateMemberSetValueError(SetValueException e)
        {
            Exception exception = new Exception(StringUtil.Format(NewObjectStrings.InvalidValue, e));
            base.ThrowTerminatingError(new ErrorRecord(exception, "SetValueException", ErrorCategory.InvalidData, null));
        }

        private static string ParameterSet2ResourceString(string parameterSet)
        {
            if (parameterSet.Equals("Net", StringComparison.OrdinalIgnoreCase))
            {
                return ".NET";
            }
            if (parameterSet.Equals("Com", StringComparison.OrdinalIgnoreCase))
            {
                return "COM";
            }
            return parameterSet;
        }

        private object SafeCreateInstance(Type t, object[] args)
        {
            object obj2 = null;
            try
            {
                obj2 = Activator.CreateInstance(t, args);
            }
            catch (ArgumentException exception)
            {
                base.ThrowTerminatingError(new ErrorRecord(exception, "CannotNewNonRuntimeType", ErrorCategory.InvalidOperation, null));
            }
            catch (NotSupportedException exception2)
            {
                base.ThrowTerminatingError(new ErrorRecord(exception2, "CannotNewTypeBuilderTypedReferenceArgIteratorRuntimeArgumentHandle", ErrorCategory.InvalidOperation, null));
            }
            catch (MethodAccessException exception3)
            {
                base.ThrowTerminatingError(new ErrorRecord(exception3, "CtorAccessDenied", ErrorCategory.PermissionDenied, null));
            }
            catch (MissingMethodException exception4)
            {
                base.ThrowTerminatingError(new ErrorRecord(exception4, "NoPublicCtorMatch", ErrorCategory.InvalidOperation, null));
            }
            catch (MemberAccessException exception5)
            {
                base.ThrowTerminatingError(new ErrorRecord(exception5, "CannotCreateAbstractClass", ErrorCategory.InvalidOperation, null));
            }
            catch (COMException exception6)
            {
                if (exception6.ErrorCode == -2147417850)
                {
                    throw;
                }
                base.ThrowTerminatingError(new ErrorRecord(exception6, "NoCOMClassIdentified", ErrorCategory.ResourceUnavailable, null));
            }
            return obj2;
        }

        private void STAComCreateThreadProc(object createstruct)
        {
            ComCreateInfo info = (ComCreateInfo) createstruct;
            try
            {
                Type t = null;
                PSArgumentException exception = null;
                t = Type.GetTypeFromCLSID(this.comObjectClsId);
                if (t == null)
                {
                    exception = PSTraceSource.NewArgumentException("ComObject", "NewObjectStrings", "CannotLoadComObjectType", new object[] { this.comObject });
                    info.e = exception;
                    info.success = false;
                }
                else
                {
                    info.objectCreated = this.SafeCreateInstance(t, this.arguments);
                    info.success = true;
                }
            }
            catch (Exception exception2)
            {
                CommandProcessorBase.CheckForSevereException(exception2);
                info.e = exception2;
                info.success = false;
            }
        }

        [Alias(new string[] { "Args" }), Parameter(ParameterSetName="Net", Mandatory=false, Position=1)]
        public object[] ArgumentList
        {
            get
            {
                return this.arguments;
            }
            set
            {
                this.arguments = value;
            }
        }

        [Parameter(ParameterSetName="Com", Mandatory=true, Position=0)]
        public string ComObject
        {
            get
            {
                return this.comObject;
            }
            set
            {
                this.comObject = value;
            }
        }

        [Parameter]
        public IDictionary Property
        {
            get
            {
                return this.property;
            }
            set
            {
                this.property = value;
            }
        }

        [Parameter(ParameterSetName="Com")]
        public SwitchParameter Strict
        {
            get
            {
                return this.strict;
            }
            set
            {
                this.strict = value;
            }
        }

        [Parameter(ParameterSetName="Net", Mandatory=true, Position=0)]
        public string TypeName
        {
            get
            {
                return this.typeName;
            }
            set
            {
                this.typeName = value;
            }
        }

        private class ComCreateInfo
        {
            public Exception e;
            public object objectCreated;
            public bool success;
        }
    }
}

