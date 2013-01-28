namespace Microsoft.PowerShell.Commands
{
    using Microsoft.PowerShell.Commands.Utility;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Management.Automation;
    using System.Management.Automation.Runspaces;
    using System.Threading;

    [Cmdlet("Update", "FormatData", SupportsShouldProcess=true, DefaultParameterSetName="FileSet", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113420")]
    public class UpdateFormatDataCommand : UpdateData
    {
        protected override void BeginProcessing()
        {
            if (base.Context.FormatDBManager.isShared)
            {
                InvalidOperationException exception = new InvalidOperationException(FormatAndOutXmlLoadingStrings.SharedFormatTableCannotBeUpdated);
                base.ThrowTerminatingError(new ErrorRecord(exception, "CannotUpdateSharedFormatTable", ErrorCategory.InvalidOperation, null));
            }
        }

        protected override void ProcessRecord()
        {
            Collection<string> collection = UpdateData.Glob(base.PrependPath, "FormatPrependPathException", this);
            Collection<string> collection2 = UpdateData.Glob(base.AppendPath, "FormatAppendPathException", this);
            if (((base.PrependPath.Length <= 0) && (base.AppendPath.Length <= 0)) || ((collection.Count != 0) || (collection2.Count != 0)))
            {
                string updateFormatDataAction = UpdateDataStrings.UpdateFormatDataAction;
                string updateTarget = UpdateDataStrings.UpdateTarget;
                if (base.Context.RunspaceConfiguration != null)
                {
                    for (int i = collection.Count - 1; i >= 0; i--)
                    {
                        string target = string.Format(Thread.CurrentThread.CurrentCulture, updateTarget, new object[] { collection[i] });
                        if (base.ShouldProcess(target, updateFormatDataAction))
                        {
                            base.Context.RunspaceConfiguration.Formats.Prepend(new FormatConfigurationEntry(collection[i]));
                        }
                    }
                    foreach (string str4 in collection2)
                    {
                        string str5 = string.Format(Thread.CurrentThread.CurrentCulture, updateTarget, new object[] { str4 });
                        if (base.ShouldProcess(str5, updateFormatDataAction))
                        {
                            base.Context.RunspaceConfiguration.Formats.Append(new FormatConfigurationEntry(str4));
                        }
                    }
                    try
                    {
                        base.Context.CurrentRunspace.RunspaceConfiguration.Formats.Update(true);
                        return;
                    }
                    catch (RuntimeException exception)
                    {
                        base.WriteError(new ErrorRecord(exception, "FormatXmlUpdateException", ErrorCategory.InvalidOperation, null));
                        return;
                    }
                }
                if (base.Context.InitialSessionState != null)
                {
                    if (base.Context.InitialSessionState.DisableFormatUpdates)
                    {
                        throw new PSInvalidOperationException(UpdateDataStrings.FormatUpdatesDisabled);
                    }
                    HashSet<string> set = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);
                    Collection<SessionStateFormatEntry> collection3 = new Collection<SessionStateFormatEntry>();
                    for (int j = collection.Count - 1; j >= 0; j--)
                    {
                        string str6 = string.Format(Thread.CurrentThread.CurrentCulture, updateTarget, new object[] { collection[j] });
                        if (base.ShouldProcess(str6, updateFormatDataAction) && !set.Contains(collection[j]))
                        {
                            set.Add(collection[j]);
                            collection3.Add(new SessionStateFormatEntry(collection[j]));
                        }
                    }
                    foreach (SessionStateFormatEntry entry in base.Context.InitialSessionState.Formats)
                    {
                        if (entry.FileName != null)
                        {
                            if (!set.Contains(entry.FileName))
                            {
                                set.Add(entry.FileName);
                                collection3.Add(entry);
                            }
                        }
                        else
                        {
                            collection3.Add(entry);
                        }
                    }
                    foreach (string str7 in collection2)
                    {
                        string str8 = string.Format(Thread.CurrentThread.CurrentCulture, updateTarget, new object[] { str7 });
                        if (base.ShouldProcess(str8, updateFormatDataAction) && !set.Contains(str7))
                        {
                            set.Add(str7);
                            collection3.Add(new SessionStateFormatEntry(str7));
                        }
                    }
                    try
                    {
                        base.Context.InitialSessionState.Formats.Clear();
                        Collection<PSSnapInTypeAndFormatErrors> mshsnapins = new Collection<PSSnapInTypeAndFormatErrors>();
                        foreach (SessionStateFormatEntry entry2 in collection3)
                        {
                            string fileName = entry2.FileName;
                            PSSnapInInfo pSSnapIn = entry2.PSSnapIn;
                            if ((pSSnapIn != null) && !string.IsNullOrEmpty(pSSnapIn.Name))
                            {
                                fileName = pSSnapIn.Name;
                            }
                            if (entry2.Formattable != null)
                            {
                                PSInvalidOperationException exception2 = new PSInvalidOperationException(UpdateDataStrings.CannotUpdateFormatWithFormatTable);
                                base.WriteError(new ErrorRecord(exception2, "CannotUpdateFormatWithFormatTable", ErrorCategory.InvalidOperation, null));
                            }
                            else
                            {
                                if (entry2.FormatData != null)
                                {
                                    mshsnapins.Add(new PSSnapInTypeAndFormatErrors(fileName, entry2.FormatData));
                                }
                                else
                                {
                                    mshsnapins.Add(new PSSnapInTypeAndFormatErrors(fileName, entry2.FileName));
                                }
                                base.Context.InitialSessionState.Formats.Add(entry2);
                            }
                        }
                        if (mshsnapins.Count > 0)
                        {
                            base.Context.FormatDBManager.UpdateDataBase(mshsnapins, base.Context.AuthorizationManager, base.Context.EngineHostInterface, false);
                            FormatAndTypeDataHelper.ThrowExceptionOnError("ErrorsUpdatingFormats", null, mshsnapins, RunspaceConfigurationCategory.Formats);
                        }
                    }
                    catch (RuntimeException exception3)
                    {
                        base.WriteError(new ErrorRecord(exception3, "FormatXmlUpdateException", ErrorCategory.InvalidOperation, null));
                    }
                }
            }
        }
    }
}

