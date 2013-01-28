namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Management.Automation;

    [Cmdlet("Set", "StrictMode", DefaultParameterSetName="Version", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113450")]
    public class SetStrictModeCommand : PSCmdlet
    {
        private SwitchParameter off;
        private System.Version version;

        protected override void EndProcessing()
        {
            if (this.off.IsPresent)
            {
                this.version = new System.Version(0, 0);
            }
            base.Context.EngineSessionState.CurrentScope.StrictModeVersion = this.version;
        }

        [Parameter(ParameterSetName="Off", Mandatory=true)]
        public SwitchParameter Off
        {
            get
            {
                return this.off;
            }
            set
            {
                this.off = value;
            }
        }

        [Alias(new string[] { "v" }), ValidateVersion, Parameter(ParameterSetName="Version", Mandatory=true), ArgumentToVersionTransformation]
        public System.Version Version
        {
            get
            {
                return this.version;
            }
            set
            {
                this.version = value;
            }
        }

        private sealed class ArgumentToVersionTransformationAttribute : ArgumentTransformationAttribute
        {
            public override object Transform(EngineIntrinsics engineIntrinsics, object inputData)
            {
                int num;
                object valueToConvert = PSObject.Base(inputData);
                string str = valueToConvert as string;
                if (str != null)
                {
                    if (str.Equals("latest", StringComparison.OrdinalIgnoreCase))
                    {
                        return PSVersionInfo.PSVersion;
                    }
                    if (str.Contains("."))
                    {
                        return inputData;
                    }
                }
                if (!(valueToConvert is double) && LanguagePrimitives.TryConvertTo<int>(valueToConvert, out num))
                {
                    return new Version(num, 0);
                }
                return inputData;
            }
        }

        private sealed class ValidateVersionAttribute : ValidateArgumentsAttribute
        {
            protected override void Validate(object arguments, EngineIntrinsics engineIntrinsics)
            {
                Version version = arguments as Version;
                if ((version == null) || !PSVersionInfo.IsValidPSVersion(version))
                {
                    throw new ValidationMetadataException("InvalidPSVersion", null, Metadata.ValidateVersionFailure, new object[] { arguments });
                }
            }
        }
    }
}

