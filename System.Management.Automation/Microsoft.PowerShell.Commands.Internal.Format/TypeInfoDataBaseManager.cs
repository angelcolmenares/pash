namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Management.Automation;
    using System.Management.Automation.Host;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Runspaces;
    using System.Runtime.InteropServices;

    internal sealed class TypeInfoDataBaseManager
    {
        private TypeInfoDataBase dataBase;
        internal object databaseLock;
        private bool disableFormatTableUpdates;
        private List<string> formatFileList;
        internal bool isShared;
        internal object updateDatabaseLock;

        internal TypeInfoDataBaseManager()
        {
            this.databaseLock = new object();
            this.updateDatabaseLock = new object();
            this.isShared = false;
            this.formatFileList = new List<string>();
        }

        internal TypeInfoDataBaseManager(IEnumerable<string> formatFiles, bool isShared, AuthorizationManager authorizationManager, PSHost host)
        {
            this.databaseLock = new object();
            this.updateDatabaseLock = new object();
            this.formatFileList = new List<string>();
            Collection<PSSnapInTypeAndFormatErrors> files = new Collection<PSSnapInTypeAndFormatErrors>();
            Collection<string> loadErrors = new Collection<string>();
            foreach (string str in formatFiles)
            {
                if (string.IsNullOrEmpty(str) || !Path.IsPathRooted(str))
                {
                    throw PSTraceSource.NewArgumentException("formatFiles", "FormatAndOutXmlLoadingStrings", "FormatFileNotRooted", new object[] { str });
                }
                PSSnapInTypeAndFormatErrors item = new PSSnapInTypeAndFormatErrors(string.Empty, str) {
                    Errors = loadErrors
                };
                files.Add(item);
                this.formatFileList.Add(str);
            }
            MshExpressionFactory expressionFactory = new MshExpressionFactory();
            List<XmlLoaderLoggerEntry> logEntries = null;
            this.LoadFromFile(files, expressionFactory, true, authorizationManager, host, false, out logEntries);
            this.isShared = isShared;
            if (loadErrors.Count > 0)
            {
                throw new FormatTableLoadException(loadErrors);
            }
        }

        internal void Add(string formatFile, bool shouldPrepend)
        {
            if (string.IsNullOrEmpty(formatFile) || !Path.IsPathRooted(formatFile))
            {
                throw PSTraceSource.NewArgumentException("formatFile", "FormatAndOutXmlLoadingStrings", "FormatFileNotRooted", new object[] { formatFile });
            }
            lock (this.formatFileList)
            {
                if (shouldPrepend)
                {
                    this.formatFileList.Insert(0, formatFile);
                }
                else
                {
                    this.formatFileList.Add(formatFile);
                }
            }
        }

        internal void AddFormatData(IEnumerable<ExtendedTypeDefinition> formatData, bool shouldPrepend)
        {
            Collection<PSSnapInTypeAndFormatErrors> files = new Collection<PSSnapInTypeAndFormatErrors>();
            Collection<string> loadErrors = new Collection<string>();
            if (shouldPrepend)
            {
                foreach (ExtendedTypeDefinition definition in formatData)
                {
                    PSSnapInTypeAndFormatErrors item = new PSSnapInTypeAndFormatErrors(string.Empty, definition) {
                        Errors = loadErrors
                    };
                    files.Add(item);
                }
                if (files.Count == 0)
                {
                    return;
                }
            }
            lock (this.formatFileList)
            {
                foreach (string str in this.formatFileList)
                {
                    PSSnapInTypeAndFormatErrors errors2 = new PSSnapInTypeAndFormatErrors(string.Empty, str) {
                        Errors = loadErrors
                    };
                    files.Add(errors2);
                }
            }
            if (!shouldPrepend)
            {
                foreach (ExtendedTypeDefinition definition2 in formatData)
                {
                    PSSnapInTypeAndFormatErrors errors3 = new PSSnapInTypeAndFormatErrors(string.Empty, definition2) {
                        Errors = loadErrors
                    };
                    files.Add(errors3);
                }
                if (files.Count == this.formatFileList.Count)
                {
                    return;
                }
            }
            MshExpressionFactory expressionFactory = new MshExpressionFactory();
            List<XmlLoaderLoggerEntry> logEntries = null;
            this.LoadFromFile(files, expressionFactory, false, null, null, false, out logEntries);
            if (loadErrors.Count > 0)
            {
                throw new FormatTableLoadException(loadErrors);
            }
        }

        private static void AddPostLoadInstrinsics(TypeInfoDataBase db)
        {
            FormatShapeSelectionOnType item = new FormatShapeSelectionOnType {
                appliesTo = new AppliesTo()
            };
            item.appliesTo.AddAppliesToType("Microsoft.PowerShell.Commands.FormatDataLoadingInfo");
            item.formatShape = FormatShape.List;
            db.defaultSettingsSection.shapeSelectionDirectives.formatShapeSelectionOnTypeList.Add(item);
        }

        private static void AddPreLoadInstrinsics(TypeInfoDataBase db)
        {
        }

        internal TypeInfoDataBase GetTypeInfoDataBase()
        {
            return this.dataBase;
        }

        internal bool LoadFromFile(Collection<PSSnapInTypeAndFormatErrors> files, MshExpressionFactory expressionFactory, bool acceptLoadingErrors, AuthorizationManager authorizationManager, PSHost host, bool preValidated, out List<XmlLoaderLoggerEntry> logEntries)
        {
            bool flag;
            try
            {
                TypeInfoDataBase base2 = null;
                lock (this.updateDatabaseLock)
                {
                    base2 = LoadFromFileHelper(files, expressionFactory, authorizationManager, host, preValidated, out logEntries, out flag);
                }
                lock (this.databaseLock)
                {
                    if (acceptLoadingErrors || flag)
                    {
                        this.dataBase = base2;
                    }
                    return flag;
                }
            }
            finally
            {
                lock (this.databaseLock)
                {
                    if (this.dataBase == null)
                    {
                        TypeInfoDataBase db = new TypeInfoDataBase();
                        AddPreLoadInstrinsics(db);
                        AddPostLoadInstrinsics(db);
                        this.dataBase = db;
                    }
                }
            }
            return flag;
        }

        private static TypeInfoDataBase LoadFromFileHelper(Collection<PSSnapInTypeAndFormatErrors> files, MshExpressionFactory expressionFactory, AuthorizationManager authorizationManager, PSHost host, bool preValidated, out List<XmlLoaderLoggerEntry> logEntries, out bool success)
        {
            success = true;
            logEntries = new List<XmlLoaderLoggerEntry>();
            TypeInfoDataBase db = new TypeInfoDataBase();
            AddPreLoadInstrinsics(db);
            foreach (PSSnapInTypeAndFormatErrors errors in files)
            {
                if (errors.FormatData != null)
                {
                    using (TypeInfoDataBaseLoader loader = new TypeInfoDataBaseLoader())
                    {
                        if (!loader.LoadFormattingData(errors.FormatData, db, expressionFactory))
                        {
                            success = false;
                        }
                        foreach (XmlLoaderLoggerEntry entry in loader.LogEntries)
                        {
                            if (entry.entryType == XmlLoaderLoggerEntry.EntryType.Error)
                            {
                                string item = StringUtil.Format(FormatAndOutXmlLoadingStrings.MshSnapinQualifiedError, errors.PSSnapinName, entry.message);
                                errors.Errors.Add(item);
                            }
                        }
                        logEntries.AddRange(loader.LogEntries);
                        continue;
                    }
                }
                XmlFileLoadInfo info = new XmlFileLoadInfo(Path.GetPathRoot(errors.FullPath), errors.FullPath, errors.Errors, errors.PSSnapinName);
                using (TypeInfoDataBaseLoader loader2 = new TypeInfoDataBaseLoader())
                {
                    if (!loader2.LoadXmlFile(info, db, expressionFactory, authorizationManager, host, preValidated))
                    {
                        success = false;
                    }
                    foreach (XmlLoaderLoggerEntry entry2 in loader2.LogEntries)
                    {
                        if (entry2.entryType == XmlLoaderLoggerEntry.EntryType.Error)
                        {
                            string str2 = StringUtil.Format(FormatAndOutXmlLoadingStrings.MshSnapinQualifiedError, info.psSnapinName, entry2.message);
                            info.errors.Add(str2);
                            if (entry2.failToLoadFile)
                            {
                                errors.FailToLoadFile = true;
                            }
                        }
                    }
                    logEntries.AddRange(loader2.LogEntries);
                }
            }
            AddPostLoadInstrinsics(db);
            return db;
        }

        internal void Remove(string formatFile)
        {
            lock (this.formatFileList)
            {
                this.formatFileList.Remove(formatFile);
            }
        }

        internal void Update(AuthorizationManager authorizationManager, PSHost host)
        {
            if (!this.DisableFormatTableUpdates)
            {
                if (this.isShared)
                {
                    throw PSTraceSource.NewInvalidOperationException("FormatAndOutXmlLoadingStrings", "SharedFormatTableCannotBeUpdated", new object[0]);
                }
                Collection<PSSnapInTypeAndFormatErrors> mshsnapins = new Collection<PSSnapInTypeAndFormatErrors>();
                lock (this.formatFileList)
                {
                    foreach (string str in this.formatFileList)
                    {
                        PSSnapInTypeAndFormatErrors item = new PSSnapInTypeAndFormatErrors(string.Empty, str);
                        mshsnapins.Add(item);
                    }
                }
                this.UpdateDataBase(mshsnapins, authorizationManager, host, false);
            }
        }

        internal void UpdateDataBase(Collection<PSSnapInTypeAndFormatErrors> mshsnapins, AuthorizationManager authorizationManager, PSHost host, bool preValidated)
        {
            if (!this.DisableFormatTableUpdates)
            {
                if (this.isShared)
                {
                    throw PSTraceSource.NewInvalidOperationException("FormatAndOutXmlLoadingStrings", "SharedFormatTableCannotBeUpdated", new object[0]);
                }
                MshExpressionFactory expressionFactory = new MshExpressionFactory();
                List<XmlLoaderLoggerEntry> logEntries = null;
                this.LoadFromFile(mshsnapins, expressionFactory, false, authorizationManager, host, preValidated, out logEntries);
            }
        }

        internal TypeInfoDataBase Database
        {
            get
            {
                return this.dataBase;
            }
        }

        internal bool DisableFormatTableUpdates
        {
            get
            {
                return this.disableFormatTableUpdates;
            }
            set
            {
                this.disableFormatTableUpdates = value;
            }
        }
    }
}

