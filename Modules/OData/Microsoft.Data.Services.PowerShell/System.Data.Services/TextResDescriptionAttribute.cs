namespace System.Data.Services
{
    using System;
    using System.ComponentModel;

    [AttributeUsage(AttributeTargets.All)]
    internal sealed class TextResDescriptionAttribute : DescriptionAttribute
    {
        private bool replaced;

        public TextResDescriptionAttribute(string description) : base(description)
        {
        }

        public override string Description
        {
            get
            {
                if (!this.replaced)
                {
                    this.replaced = true;
                    base.DescriptionValue = TextRes.GetString(base.Description);
                }
                return base.Description;
            }
        }
    }
}

