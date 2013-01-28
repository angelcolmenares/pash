namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Collections;
    using System.Management.Automation;
    using System.Management.Automation.Internal;

    internal class ExpressionEntryDefinition : HashtableEntryDefinition
    {
        private bool _noGlobbing;

        internal ExpressionEntryDefinition() : this(false)
        {
        }

        internal ExpressionEntryDefinition(bool noGlobbing) : base("expression", new Type[] { typeof(string), typeof(ScriptBlock) }, true)
        {
            this._noGlobbing = noGlobbing;
        }

        internal override Hashtable CreateHashtableFromSingleType(object val)
        {
            Hashtable hashtable = new Hashtable();
            hashtable.Add("expression", val);
            return hashtable;
        }

        private void ProcessEmptyStringError(bool originalParameterWasHashTable, TerminatingErrorContext invocationContext)
        {
            string str;
            string str2;
            if (originalParameterWasHashTable)
            {
                str = StringUtil.Format(FormatAndOut_MshParameter.MshExEmptyStringHashError, base.KeyName);
                str2 = "ExpressionEmptyString1";
            }
            else
            {
                str = StringUtil.Format(FormatAndOut_MshParameter.MshExEmptyStringError, new object[0]);
                str2 = "ExpressionEmptyString2";
            }
            ParameterProcessor.ThrowParameterBindingException(invocationContext, str2, str);
        }

        private void ProcessGlobbingCharactersError(bool originalParameterWasHashTable, string expression, TerminatingErrorContext invocationContext)
        {
            string str;
            string str2;
            if (originalParameterWasHashTable)
            {
                str = StringUtil.Format(FormatAndOut_MshParameter.MshExGlobbingHashError, base.KeyName, expression);
                str2 = "ExpressionGlobbing1";
            }
            else
            {
                str = StringUtil.Format(FormatAndOut_MshParameter.MshExGlobbingStringError, expression);
                str2 = "ExpressionGlobbing2";
            }
            ParameterProcessor.ThrowParameterBindingException(invocationContext, str2, str);
        }

        internal override object Verify(object val, TerminatingErrorContext invocationContext, bool originalParameterWasHashTable)
        {
            if (val == null)
            {
                throw PSTraceSource.NewArgumentNullException("val");
            }
            ScriptBlock scriptBlock = val as ScriptBlock;
            if (scriptBlock != null)
            {
                return new MshExpression(scriptBlock);
            }
            string str = val as string;
            if (str != null)
            {
                if (string.IsNullOrEmpty(str))
                {
                    this.ProcessEmptyStringError(originalParameterWasHashTable, invocationContext);
                }
                MshExpression expression2 = new MshExpression(str);
                if (this._noGlobbing && expression2.HasWildCardCharacters)
                {
                    this.ProcessGlobbingCharactersError(originalParameterWasHashTable, str, invocationContext);
                }
                return expression2;
            }
            PSTraceSource.NewArgumentException("val");
            return null;
        }
    }
}

