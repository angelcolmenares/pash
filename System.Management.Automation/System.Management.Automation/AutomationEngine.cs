namespace System.Management.Automation
{
    using System;
    using System.Linq;
    using System.Management.Automation.Host;
    using System.Management.Automation.Language;
    using System.Management.Automation.Runspaces;

    internal class AutomationEngine
    {
        private ExecutionContext _context;
        private System.Management.Automation.CommandDiscovery commandDiscovery;
        internal Parser EngineNewParser;

        internal AutomationEngine(PSHost hostInterface, RunspaceConfiguration runspaceConfiguration, InitialSessionState iss)
        {
            string str = Environment.GetEnvironmentVariable("PathEXT") ?? string.Empty;
            bool flag = false;
            if (str != string.Empty)
            {
                foreach (string str2 in str.Split(new char[] { ';' }))
                {
                    if (str2.Trim().Equals(".CPL", StringComparison.OrdinalIgnoreCase))
                    {
                        flag = true;
                        break;
                    }
                }
            }
            if (!flag)
            {
                str = (str == string.Empty) ? ".CPL" : (str.EndsWith(";", StringComparison.OrdinalIgnoreCase) ? (str + ".CPL") : (str + ";.CPL"));
                Environment.SetEnvironmentVariable("PathEXT", str);
            }
            if (runspaceConfiguration != null)
            {
                this._context = new ExecutionContext(this, hostInterface, runspaceConfiguration);
            }
            else
            {
                this._context = new ExecutionContext(this, hostInterface, iss);
            }
            this.EngineNewParser = new Parser();
            this.commandDiscovery = new System.Management.Automation.CommandDiscovery(this._context);
            if (runspaceConfiguration != null)
            {
                runspaceConfiguration.Bind(this._context);
            }
            else
            {
                iss.Bind(this._context, false);
            }
            InitialSessionState.SetSessionStateDrive(this._context, true);
            InitialSessionState.CreateQuestionVariable(this._context);
        }

        internal string Expand(string s)
        {
            return ((Compiler.GetExpressionValue(Parser.ScanString(s), this.Context, this.Context.EngineSessionState, null) as string) ?? "");
        }

        internal ScriptBlock ParseScriptBlock(string script, bool interactiveCommand)
        {
            return this.ParseScriptBlock(script, null, interactiveCommand);
        }

        internal ScriptBlock ParseScriptBlock(string script, string fileName, bool interactiveCommand)
        {
            ParseError[] errorArray;
            ScriptBlockAst ast = this.EngineNewParser.Parse(fileName, script, null, out errorArray);
            if (interactiveCommand)
            {
                this.EngineNewParser.SetPreviousFirstLastToken(this._context);
            }
            if (!errorArray.Any<ParseError>())
            {
                return new ScriptBlock(ast, false);
            }
            if (errorArray[0].IncompleteInput)
            {
                throw new IncompleteParseException(errorArray[0].Message, errorArray[0].ErrorId);
            }
            throw new ParseException(errorArray);
        }

        internal System.Management.Automation.CommandDiscovery CommandDiscovery
        {
            get
            {
                return this.commandDiscovery;
            }
        }

        internal ExecutionContext Context
        {
            get
            {
                return this._context;
            }
        }
    }
}

