namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Text;

    [Cmdlet("Import", "LocalizedData", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113342")]
    public sealed class ImportLocalizedData : PSCmdlet
    {
        private string _baseDirectory;
        private string _bindingVariable;
        private string[] _commandsAllowed = new string[] { "ConvertFrom-StringData" };
        private string _fileName;
        private string _uiculture;

        private string GetFilePath()
        {
            StringBuilder builder;
            string str3;
            if (string.IsNullOrEmpty(this._fileName) && ((base.InvocationExtent == null) || string.IsNullOrEmpty(base.InvocationExtent.File)))
            {
                throw PSTraceSource.NewInvalidOperationException("ImportLocalizedData", "NotCalledFromAScriptFile", new object[0]);
            }
            string directoryName = this._baseDirectory;
            if (string.IsNullOrEmpty(directoryName))
            {
                if (!string.IsNullOrEmpty(base.InvocationExtent.File))
                {
                    directoryName = Path.GetDirectoryName(base.InvocationExtent.File);
                }
                else
                {
                    directoryName = ".";
                }
            }
            directoryName = PathUtils.ResolveFilePath(directoryName, this);
            string fileNameWithoutExtension = this._fileName;
            if (string.IsNullOrEmpty(fileNameWithoutExtension))
            {
                fileNameWithoutExtension = base.InvocationExtent.File;
            }
            else if (!string.IsNullOrEmpty(Path.GetDirectoryName(fileNameWithoutExtension)))
            {
                throw PSTraceSource.NewInvalidOperationException("ImportLocalizedData", "FileNameParameterCannotHavePath", new object[0]);
            }
            fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileNameWithoutExtension);
            CultureInfo currentUICulture = null;
            if (this._uiculture == null)
            {
                currentUICulture = CultureInfo.CurrentUICulture;
            }
            else
            {
                try
                {
                    currentUICulture = CultureInfo.GetCultureInfo(this._uiculture);
                }
                catch (ArgumentException)
                {
                    throw PSTraceSource.NewArgumentException("Culture");
                }
            }
            for (CultureInfo info2 = currentUICulture; (info2 != null) && !string.IsNullOrEmpty(info2.Name); info2 = info2.Parent)
            {
                builder = new StringBuilder(directoryName);
                builder.Append(@"\");
                builder.Append(info2.Name);
                builder.Append(@"\");
                builder.Append(fileNameWithoutExtension);
                builder.Append(".psd1");
                str3 = builder.ToString();
                if (File.Exists(str3))
                {
                    return str3;
                }
            }
            builder = new StringBuilder(directoryName);
            builder.Append(@"\");
            builder.Append(fileNameWithoutExtension);
            builder.Append(".psd1");
            str3 = builder.ToString();
            if (File.Exists(str3))
            {
                return str3;
            }
            InvalidOperationException exception = PSTraceSource.NewInvalidOperationException("ImportLocalizedData", "CannotFindPsd1File", new object[] { fileNameWithoutExtension + ".psd1", directoryName + @"\" + currentUICulture.Name + @"\" });
            base.WriteError(new ErrorRecord(exception, "ImportLocalizedData", ErrorCategory.ObjectNotFound, directoryName + @"\" + currentUICulture.Name + @"\" + fileNameWithoutExtension + ".psd1"));
            return null;
        }

        private string GetScript(string filePath)
        {
            InvalidOperationException exception = null;
            try
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (ArgumentException exception2)
            {
                exception = PSTraceSource.NewInvalidOperationException("ImportLocalizedData", "ErrorOpeningFile", new object[] { filePath, exception2.Message });
            }
            catch (IOException exception3)
            {
                exception = PSTraceSource.NewInvalidOperationException("ImportLocalizedData", "ErrorOpeningFile", new object[] { filePath, exception3.Message });
            }
            catch (NotSupportedException exception4)
            {
                exception = PSTraceSource.NewInvalidOperationException("ImportLocalizedData", "ErrorOpeningFile", new object[] { filePath, exception4.Message });
            }
            catch (UnauthorizedAccessException exception5)
            {
                exception = PSTraceSource.NewInvalidOperationException("ImportLocalizedData", "ErrorOpeningFile", new object[] { filePath, exception5.Message });
            }
            base.WriteError(new ErrorRecord(exception, "ImportLocalizedData", ErrorCategory.OpenError, filePath));
            return null;
        }

        protected override void ProcessRecord()
        {
            string filePath = this.GetFilePath();
            if (filePath != null)
            {
                if (!File.Exists(filePath))
                {
                    InvalidOperationException exception = PSTraceSource.NewInvalidOperationException("ImportLocalizedData", "FileNotExist", new object[] { filePath });
                    base.WriteError(new ErrorRecord(exception, "ImportLocalizedData", ErrorCategory.ObjectNotFound, filePath));
                }
                else
                {
                    string script = this.GetScript(filePath);
                    if (script != null)
                    {
                        try
                        {
                            object obj2;
                            ScriptBlock block = base.Context.Engine.ParseScriptBlock(script, false);
                            block.CheckRestrictedLanguage(this.SupportedCommand, null, false);
                            PSLanguageMode languageMode = base.Context.LanguageMode;
                            base.Context.LanguageMode = PSLanguageMode.RestrictedLanguage;
                            try
                            {
                                obj2 = block.InvokeReturnAsIs(new object[0]);
                                if (obj2 == AutomationNull.Value)
                                {
                                    obj2 = null;
                                }
                            }
                            finally
                            {
                                base.Context.LanguageMode = languageMode;
                            }
                            if (this._bindingVariable != null)
                            {
                                VariablePath variablePath = new VariablePath(this._bindingVariable);
                                if (variablePath.IsUnscopedVariable)
                                {
                                    variablePath = variablePath.CloneAndSetLocal();
                                }
                                if (string.IsNullOrEmpty(variablePath.UnqualifiedPath))
                                {
                                    InvalidOperationException exception2 = PSTraceSource.NewInvalidOperationException("ImportLocalizedData", "IncorrectVariableName", new object[] { this._bindingVariable });
                                    base.WriteError(new ErrorRecord(exception2, "ImportLocalizedData", ErrorCategory.InvalidArgument, this._bindingVariable));
                                }
                                else
                                {
                                    SessionStateScope scope = null;
                                    PSVariable variableItem = base.SessionState.Internal.GetVariableItem(variablePath, out scope);
                                    if (variableItem == null)
                                    {
                                        variableItem = new PSVariable(variablePath.UnqualifiedPath, obj2, ScopedItemOptions.None);
                                        base.Context.EngineSessionState.SetVariable(variablePath, variableItem, false, CommandOrigin.Internal);
                                    }
                                    else
                                    {
                                        variableItem.Value = obj2;
                                    }
                                }
                            }
                            else
                            {
                                base.WriteObject(obj2);
                            }
                        }
                        catch (RuntimeException exception3)
                        {
                            throw PSTraceSource.NewInvalidOperationException(exception3, "ImportLocalizedData", "ErrorLoadingDataFile", new object[] { filePath, exception3.Message });
                        }
                    }
                }
            }
        }

        [Parameter]
        public string BaseDirectory
        {
            get
            {
                return this._baseDirectory;
            }
            set
            {
                this._baseDirectory = value;
            }
        }

        [Alias(new string[] { "Variable" }), Parameter(Position=0), ValidateNotNullOrEmpty]
        public string BindingVariable
        {
            get
            {
                return this._bindingVariable;
            }
            set
            {
                this._bindingVariable = value;
            }
        }

        [Parameter]
        public string FileName
        {
            get
            {
                return this._fileName;
            }
            set
            {
                this._fileName = value;
            }
        }

        [Parameter]
        public string[] SupportedCommand
        {
            get
            {
                return this._commandsAllowed;
            }
            set
            {
                this._commandsAllowed = value;
            }
        }

        [Parameter(Position=1)]
        public string UICulture
        {
            get
            {
                return this._uiculture;
            }
            set
            {
                this._uiculture = value;
            }
        }
    }
}

