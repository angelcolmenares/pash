namespace System.Management.Automation
{
    using System;

    [AttributeUsage(AttributeTargets.Field, AllowMultiple=false)]
    internal class TraceSourceAttribute : Attribute
    {
        private string category;
        private string description;

        internal TraceSourceAttribute(string category, string description)
        {
            this.category = category;
            this.description = description;
        }

        internal string Category
        {
            get
            {
                return this.category;
            }
        }

        internal string Description
        {
            get
            {
                return this.description;
            }
            set
            {
                this.description = value;
            }
        }
    }
}

