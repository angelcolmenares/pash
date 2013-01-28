namespace Microsoft.PowerShell.Commands
{
    using Microsoft.PowerShell.Commands.Utility;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Management.Automation;
    using System.Management.Automation.Runspaces;

    [Cmdlet("Remove", "TypeData", SupportsShouldProcess=true, DefaultParameterSetName="RemoveTypeDataSet", HelpUri="http://go.microsoft.com/fwlink/?LinkID=217038")]
    public class RemoveTypeDataCommand : PSCmdlet
    {
        private const string RemoveFileSet = "RemoveFileSet";
        private const string RemoveTypeDataSet = "RemoveTypeDataSet";
        private const string RemoveTypeSet = "RemoveTypeSet";
        private System.Management.Automation.Runspaces.TypeData typeData;
        private string[] typeFiles;
        private string typeName;

        private static void ConstructFileToIndexMap(string fileName, int index, Dictionary<string, List<int>> fileNameToIndexMap)
        {
            List<int> list;
            if (fileNameToIndexMap.TryGetValue(fileName, out list))
            {
                list.Add(index);
            }
            else
            {
                List<int> list2 = new List<int> {
                    index
                };
                fileNameToIndexMap[fileName] = list2;
            }
        }

        private ErrorRecord NewError(string errorId, string resourceId, object targetObject, params object[] args)
        {
            ErrorDetails details = new ErrorDetails(base.GetType().Assembly, "UpdateDataStrings", resourceId, args);
            return new ErrorRecord(new InvalidOperationException(details.Message), errorId, ErrorCategory.InvalidOperation, targetObject);
        }

        protected override void ProcessRecord()
        {
            if (base.ParameterSetName == "RemoveFileSet")
            {
                string removeTypeFileAction = UpdateDataStrings.RemoveTypeFileAction;
                string updateTarget = UpdateDataStrings.UpdateTarget;
                Collection<string> collection = UpdateData.Glob(this.typeFiles, "TypePathException", this);
                if (collection.Count != 0)
                {
                    Dictionary<string, List<int>> fileNameToIndexMap = new Dictionary<string, List<int>>(StringComparer.OrdinalIgnoreCase);
                    List<int> list = new List<int>();
                    if (base.Context.RunspaceConfiguration != null)
                    {
                        for (int i = 0; i < base.Context.RunspaceConfiguration.Types.Count; i++)
                        {
                            string fileName = base.Context.RunspaceConfiguration.Types[i].FileName;
                            if (fileName != null)
                            {
                                ConstructFileToIndexMap(fileName, i, fileNameToIndexMap);
                            }
                        }
                    }
                    else if (base.Context.InitialSessionState != null)
                    {
                        for (int j = 0; j < base.Context.InitialSessionState.Types.Count; j++)
                        {
                            string filePath = base.Context.InitialSessionState.Types[j].FileName;
                            if (filePath != null)
                            {
                                filePath = ModuleCmdletBase.ResolveRootedFilePath(filePath, base.Context) ?? filePath;
                                ConstructFileToIndexMap(filePath, j, fileNameToIndexMap);
                            }
                        }
                    }
                    foreach (string str5 in collection)
                    {
                        string target = string.Format(CultureInfo.InvariantCulture, updateTarget, new object[] { str5 });
                        if (base.ShouldProcess(target, removeTypeFileAction))
                        {
                            List<int> list2;
                            if (fileNameToIndexMap.TryGetValue(str5, out list2))
                            {
                                list.AddRange(list2);
                            }
                            else
                            {
                                base.WriteError(this.NewError("TypeFileNotExistsInCurrentSession", "TypeFileNotExistsInCurrentSession", null, new object[] { str5 }));
                            }
                        }
                    }
                    if (list.Count > 0)
                    {
                        list.Sort();
                        for (int k = list.Count - 1; k >= 0; k--)
                        {
                            if (base.Context.RunspaceConfiguration != null)
                            {
                                base.Context.RunspaceConfiguration.Types.RemoveItem(list[k]);
                            }
                            else if (base.Context.InitialSessionState != null)
                            {
                                base.Context.InitialSessionState.Types.RemoveItem(list[k]);
                            }
                        }
                        try
                        {
                            if (base.Context.RunspaceConfiguration != null)
                            {
                                base.Context.RunspaceConfiguration.Types.Update();
                            }
                            else if (base.Context.InitialSessionState != null)
                            {
                                bool refreshTypeAndFormatSetting = base.Context.InitialSessionState.RefreshTypeAndFormatSetting;
                                try
                                {
                                    base.Context.InitialSessionState.RefreshTypeAndFormatSetting = true;
                                    base.Context.InitialSessionState.UpdateTypes(base.Context, false, false);
                                }
                                finally
                                {
                                    base.Context.InitialSessionState.RefreshTypeAndFormatSetting = refreshTypeAndFormatSetting;
                                }
                            }
                        }
                        catch (RuntimeException exception)
                        {
                            base.WriteError(new ErrorRecord(exception, "TypesFileRemoveException", ErrorCategory.InvalidOperation, null));
                        }
                    }
                }
            }
            else
            {
                string removeTypeDataAction = UpdateDataStrings.RemoveTypeDataAction;
                string removeTypeDataTarget = UpdateDataStrings.RemoveTypeDataTarget;
                string typeName = null;
                if (base.ParameterSetName == "RemoveTypeDataSet")
                {
                    typeName = this.typeData.TypeName;
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(this.typeName))
                    {
                        base.ThrowTerminatingError(this.NewError("TargetTypeNameEmpty", "TargetTypeNameEmpty", this.typeName, new object[0]));
                    }
                    typeName = this.typeName;
                }
                System.Management.Automation.Runspaces.TypeData type = new System.Management.Automation.Runspaces.TypeData(typeName);
                string str10 = string.Format(CultureInfo.InvariantCulture, removeTypeDataTarget, new object[] { typeName });
                if (base.ShouldProcess(str10, removeTypeDataAction))
                {
                    try
                    {
                        Collection<string> errors = new Collection<string>();
                        base.Context.TypeTable.Update(type, errors, true, false);
                        if (errors.Count > 0)
                        {
                            foreach (string str11 in errors)
                            {
                                RuntimeException exception2 = new RuntimeException(str11);
                                base.WriteError(new ErrorRecord(exception2, "TypesDynamicRemoveException", ErrorCategory.InvalidOperation, null));
                            }
                        }
                        else if (base.Context.RunspaceConfiguration != null)
                        {
                            base.Context.RunspaceConfiguration.Types.Append(new TypeConfigurationEntry(type, true));
                        }
                        else if (base.Context.InitialSessionState != null)
                        {
                            base.Context.InitialSessionState.Types.Add(new SessionStateTypeEntry(type, true));
                        }
                    }
                    catch (RuntimeException exception3)
                    {
                        base.WriteError(new ErrorRecord(exception3, "TypesDynamicRemoveException", ErrorCategory.InvalidOperation, null));
                    }
                }
            }
        }

        [Parameter(Mandatory=true, ParameterSetName="RemoveFileSet"), ValidateNotNullOrEmpty]
        public string[] Path
        {
            get
            {
                return this.typeFiles;
            }
            set
            {
                this.typeFiles = value;
            }
        }

        [Parameter(Mandatory=true, ValueFromPipeline=true, ParameterSetName="RemoveTypeDataSet")]
        public System.Management.Automation.Runspaces.TypeData TypeData
        {
            get
            {
                return this.typeData;
            }
            set
            {
                this.typeData = value;
            }
        }

        [ValidateNotNullOrEmpty, Parameter(Mandatory=true, Position=0, ValueFromPipeline=true, ValueFromPipelineByPropertyName=true, ParameterSetName="RemoveTypeSet"), ArgumentToTypeNameTransformation]
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
    }
}

