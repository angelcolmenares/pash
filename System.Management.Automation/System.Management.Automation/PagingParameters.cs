namespace System.Management.Automation
{
    using System;
    using System.Globalization;
    using System.Runtime.CompilerServices;

    public sealed class PagingParameters
    {
        private ulong psFirst = ulong.MaxValue;

        internal PagingParameters(MshCommandRuntime commandRuntime)
        {
            if (commandRuntime == null)
            {
                throw PSTraceSource.NewArgumentNullException("commandRuntime");
            }
            commandRuntime.PagingParameters = this;
        }

        public PSObject NewTotalCount(ulong totalCount, double accuracy)
        {
            PSObject obj2 = new PSObject(totalCount);
            string script = string.Format(CultureInfo.CurrentCulture, "\r\n                    $totalCount = $this.PSObject.BaseObject\r\n                    switch ($this.Accuracy) {{\r\n                        {{ $_ -ge 1.0 }} {{ '{0}' -f $totalCount }}\r\n                        {{ $_ -le 0.0 }} {{ '{1}' -f $totalCount }}\r\n                        default          {{ '{2}' -f $totalCount }}\r\n                    }}\r\n                ", new object[] { CommandMetadata.EscapeSingleQuotedString(CommandBaseStrings.PagingSupportAccurateTotalCountTemplate), CommandMetadata.EscapeSingleQuotedString(CommandBaseStrings.PagingSupportUnknownTotalCountTemplate), CommandMetadata.EscapeSingleQuotedString(CommandBaseStrings.PagingSupportEstimatedTotalCountTemplate) });
            PSScriptMethod member = new PSScriptMethod("ToString", ScriptBlock.Create(script));
            obj2.Members.Add(member);
            accuracy = Math.Max(0.0, Math.Min(1.0, accuracy));
            PSNoteProperty property = new PSNoteProperty("Accuracy", accuracy);
            obj2.Members.Add(property);
            return obj2;
        }

        [Parameter]
        public ulong First
        {
            get
            {
                return this.psFirst;
            }
            set
            {
                this.psFirst = value;
            }
        }

        [Parameter]
        public SwitchParameter IncludeTotalCount { get; set; }

        [Parameter]
        public ulong Skip { get; set; }
    }
}

