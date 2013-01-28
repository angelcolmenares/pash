namespace Microsoft.PowerShell.Commands
{
    using Microsoft.PowerShell.Commands.Internal.Format;
    using System;
    using System.Collections;
    using System.Management.Automation;

    internal class OriginalColumnInfo : Microsoft.PowerShell.Commands.ColumnInfo
    {
        private string liveObjectPropertyName;
        private OutGridViewCommand parentCmdlet;

        internal OriginalColumnInfo(string staleObjectPropertyName, string displayName, string liveObjectPropertyName, OutGridViewCommand parentCmdlet) : base(staleObjectPropertyName, displayName)
        {
            this.liveObjectPropertyName = liveObjectPropertyName;
            this.parentCmdlet = parentCmdlet;
        }

        internal override object GetValue(PSObject liveObject)
        {
            object obj4 = null;
            try
            {
                PSPropertyInfo info = liveObject.Properties[this.liveObjectPropertyName];
                if (info == null)
                {
                    return null;
                }
                object src = info.Value;
                if (src is ICollection)
                {
                    src = this.parentCmdlet.ConvertToString(PSObjectHelper.AsPSObject(info.Value));
                }
                else
                {
                    PSObject obj3 = src as PSObject;
                    if (obj3 != null)
                    {
                        if (obj3.BaseObject is IComparable)
                        {
                            src = obj3;
                        }
                        else
                        {
                            src = this.parentCmdlet.ConvertToString(obj3);
                        }
                    }
                }
                obj4 = Microsoft.PowerShell.Commands.ColumnInfo.LimitString(src);
            }
            catch (GetValueException)
            {
            }
            catch (ExtendedTypeSystemException)
            {
            }
            return obj4;
        }
    }
}

