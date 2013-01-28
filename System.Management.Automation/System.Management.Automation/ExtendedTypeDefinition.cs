namespace System.Management.Automation
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    public sealed class ExtendedTypeDefinition
    {
        private string _typename;
        private List<System.Management.Automation.FormatViewDefinition> _viewdefinitions;

        public ExtendedTypeDefinition(string typeName)
        {
            this._viewdefinitions = new List<System.Management.Automation.FormatViewDefinition>();
            if (string.IsNullOrEmpty(typeName))
            {
                throw PSTraceSource.NewArgumentNullException("typeName");
            }
            this._typename = typeName;
        }

        public ExtendedTypeDefinition(string typeName, IEnumerable<System.Management.Automation.FormatViewDefinition> viewDefinitions)
        {
            this._viewdefinitions = new List<System.Management.Automation.FormatViewDefinition>();
            if (string.IsNullOrEmpty(typeName))
            {
                throw PSTraceSource.NewArgumentNullException("typeName");
            }
            if (viewDefinitions == null)
            {
                throw PSTraceSource.NewArgumentNullException("viewDefinitions");
            }
            this._typename = typeName;
            foreach (System.Management.Automation.FormatViewDefinition definition in viewDefinitions)
            {
                this._viewdefinitions.Add(definition);
            }
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}", new object[] { this._typename });
        }

        public List<System.Management.Automation.FormatViewDefinition> FormatViewDefinition
        {
            get
            {
                return this._viewdefinitions;
            }
        }

        public string TypeName
        {
            get
            {
                return this._typename;
            }
        }
    }
}

