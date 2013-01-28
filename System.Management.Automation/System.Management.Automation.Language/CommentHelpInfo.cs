namespace System.Management.Automation.Language
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime.CompilerServices;
    using System.Text;

    public sealed class CommentHelpInfo
    {
        public string GetCommentBlock()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("<#");
            if (!string.IsNullOrEmpty(this.Synopsis))
            {
                builder.AppendLine(".SYNOPSIS");
                builder.AppendLine(this.Synopsis);
            }
            if (!string.IsNullOrEmpty(this.Description))
            {
                builder.AppendLine(".DESCRIPTION");
                builder.AppendLine(this.Description);
            }
            foreach (KeyValuePair<string, string> pair in this.Parameters)
            {
                builder.Append(".PARAMETER ");
                builder.AppendLine(pair.Key);
                builder.AppendLine(pair.Value);
            }
            foreach (string str in this.Inputs)
            {
                builder.AppendLine(".INPUTS");
                builder.AppendLine(str);
            }
            foreach (string str2 in this.Outputs)
            {
                builder.AppendLine(".OUTPUTS");
                builder.AppendLine(str2);
            }
            if (!string.IsNullOrEmpty(this.Notes))
            {
                builder.AppendLine(".NOTES");
                builder.AppendLine(this.Notes);
            }
            foreach (string str3 in this.Examples)
            {
                builder.AppendLine(".EXAMPLE");
                builder.AppendLine(str3);
            }
            foreach (string str4 in this.Links)
            {
                builder.AppendLine(".LINK");
                builder.AppendLine(str4);
            }
            if (!string.IsNullOrEmpty(this.ForwardHelpTargetName))
            {
                builder.Append(".FORWARDHELPTARGETNAME ");
                builder.AppendLine(this.ForwardHelpTargetName);
            }
            if (!string.IsNullOrEmpty(this.ForwardHelpCategory))
            {
                builder.Append(".FORWARDHELPCATEGORY ");
                builder.AppendLine(this.ForwardHelpCategory);
            }
            if (!string.IsNullOrEmpty(this.RemoteHelpRunspace))
            {
                builder.Append(".REMOTEHELPRUNSPACE ");
                builder.AppendLine(this.RemoteHelpRunspace);
            }
            if (!string.IsNullOrEmpty(this.Component))
            {
                builder.AppendLine(".COMPONENT");
                builder.AppendLine(this.Component);
            }
            if (!string.IsNullOrEmpty(this.Role))
            {
                builder.AppendLine(".ROLE");
                builder.AppendLine(this.Role);
            }
            if (!string.IsNullOrEmpty(this.Functionality))
            {
                builder.AppendLine(".FUNCTIONALITY");
                builder.AppendLine(this.Functionality);
            }
            if (!string.IsNullOrEmpty(this.MamlHelpFile))
            {
                builder.Append(".EXTERNALHELP ");
                builder.AppendLine(this.MamlHelpFile);
            }
            builder.AppendLine("#>");
            return builder.ToString();
        }

        public string Component { get; internal set; }

        public string Description { get; internal set; }

        public ReadOnlyCollection<string> Examples { get; internal set; }

        public string ForwardHelpCategory { get; internal set; }

        public string ForwardHelpTargetName { get; internal set; }

        public string Functionality { get; internal set; }

        public ReadOnlyCollection<string> Inputs { get; internal set; }

        public ReadOnlyCollection<string> Links { get; internal set; }

        public string MamlHelpFile { get; internal set; }

        public string Notes { get; internal set; }

        public ReadOnlyCollection<string> Outputs { get; internal set; }

        public IDictionary<string, string> Parameters { get; internal set; }

        public string RemoteHelpRunspace { get; internal set; }

        public string Role { get; internal set; }

        public string Synopsis { get; internal set; }
    }
}

