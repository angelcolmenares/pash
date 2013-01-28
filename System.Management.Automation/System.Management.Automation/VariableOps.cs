namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Language;
    using System.Runtime.CompilerServices;

    internal static class VariableOps
    {
        private static Collection<Attribute> GetAttributeCollection(AttributeBaseAst[] attributeAsts)
        {
            Collection<Attribute> collection = new Collection<Attribute>();
            foreach (AttributeBaseAst ast in attributeAsts)
            {
                collection.Add(ast.GetAttribute());
            }
            return collection;
        }

        private static object GetAutomaticVariableValue(int tupleIndex, ExecutionContext executionContext, VariableExpressionAst varAst)
        {
            if (executionContext._debuggingMode > 0)
            {
                executionContext.Debugger.CheckVariableRead(SpecialVariables.AutomaticVariables[tupleIndex]);
            }
            object automaticVariableValue = executionContext.EngineSessionState.GetAutomaticVariableValue((AutomaticVariable) tupleIndex);
            if (automaticVariableValue != AutomationNull.Value)
            {
                return automaticVariableValue;
            }
            if (ThrowStrictModeUndefinedVariable(executionContext, varAst))
            {
                throw InterpreterError.NewInterpreterException(SpecialVariables.AutomaticVariables[tupleIndex], typeof(RuntimeException), varAst.Extent, "VariableIsUndefined", ParserStrings.VariableIsUndefined, new object[] { SpecialVariables.AutomaticVariables[tupleIndex] });
            }
            return null;
        }

        private static object GetUsingValue(MutableTuple tuple, int index, ExecutionContext context)
        {
            UsingResult usingValueFromTuple = GetUsingValueFromTuple(tuple, index);
            if (usingValueFromTuple != null)
            {
                return usingValueFromTuple.Value;
            }
            for (SessionStateScope scope = context.EngineSessionState.CurrentScope; scope != null; scope = scope.Parent)
            {
                usingValueFromTuple = GetUsingValueFromTuple(scope.LocalsTuple, index);
                if (usingValueFromTuple != null)
                {
                    return usingValueFromTuple.Value;
                }
                foreach (MutableTuple tuple2 in scope.DottedScopes)
                {
                    usingValueFromTuple = GetUsingValueFromTuple(tuple2, index);
                    if (usingValueFromTuple != null)
                    {
                        return usingValueFromTuple.Value;
                    }
                }
            }
            throw InterpreterError.NewInterpreterException(null, typeof(RuntimeException), null, "UsingWithoutInvokeCommand", ParserStrings.UsingWithoutInvokeCommand, new object[0]);
        }

        private static UsingResult GetUsingValueFromTuple(MutableTuple tuple, int index)
        {
            PSBoundParametersDictionary automaticVariable = tuple.GetAutomaticVariable(AutomaticVariable.PSBoundParameters) as PSBoundParametersDictionary;
            if (automaticVariable != null)
            {
                IList implicitUsingParameters = automaticVariable.ImplicitUsingParameters;
                if ((implicitUsingParameters != null) && (index <= (implicitUsingParameters.Count - 1)))
                {
                    return new UsingResult { Value = implicitUsingParameters[index] };
                }
            }
            return null;
        }

        private static PSReference GetVariableAsRef(VariablePath variablePath, ExecutionContext executionContext, Type staticType)
        {
            SessionStateScope scope;
            SessionStateInternal engineSessionState = executionContext.EngineSessionState;
            CommandOrigin scopeOrigin = engineSessionState.CurrentScope.ScopeOrigin;
            PSVariable variable = engineSessionState.GetVariableItem(variablePath, out scope, scopeOrigin);
            if (variable == null)
            {
                throw InterpreterError.NewInterpreterException(variablePath, typeof(RuntimeException), null, "NonExistingVariableReference", ParserStrings.NonExistingVariableReference, new object[0]);
            }
            object obj2 = variable.Value;
            if ((staticType == null) && (obj2 != null))
            {
                obj2 = PSObject.Base(obj2);
                if (obj2 != null)
                {
                    staticType = obj2.GetType();
                }
            }
            if (staticType == null)
            {
                ArgumentTypeConverterAttribute attribute = variable.Attributes.OfType<ArgumentTypeConverterAttribute>().FirstOrDefault<ArgumentTypeConverterAttribute>();
                staticType = (attribute != null) ? attribute.TargetType : typeof(LanguagePrimitives.Null);
            }
            return PSReference.CreateInstance(variable, staticType);
        }

        internal static object GetVariableValue(VariablePath variablePath, ExecutionContext executionContext, VariableExpressionAst varAst)
        {
            SessionStateScope scope2;
            if (!variablePath.IsVariable)
            {
                CmdletProviderContext context;
                SessionStateScope scope;
                SessionStateInternal internal2 = executionContext.EngineSessionState;
                return internal2.GetVariableValueFromProvider(variablePath, out context, out scope, internal2.CurrentScope.ScopeOrigin);
            }
            SessionStateInternal engineSessionState = executionContext.EngineSessionState;
            CommandOrigin scopeOrigin = engineSessionState.CurrentScope.ScopeOrigin;
            PSVariable variable = engineSessionState.GetVariableItem(variablePath, out scope2, scopeOrigin);
            if (variable != null)
            {
                return variable.Value;
            }
            if (engineSessionState.ExecutionContext._debuggingMode > 0)
            {
                engineSessionState.ExecutionContext.Debugger.CheckVariableRead(variablePath.UnqualifiedPath);
            }
            if (ThrowStrictModeUndefinedVariable(executionContext, varAst))
            {
                throw InterpreterError.NewInterpreterException(variablePath.UserPath, typeof(RuntimeException), varAst.Extent, "VariableIsUndefined", ParserStrings.VariableIsUndefined, new object[] { variablePath.UserPath });
            }
            return null;
        }

        private static object SetVariableValue(VariablePath variablePath, object value, ExecutionContext executionContext, AttributeBaseAst[] attributeAsts)
        {
            SessionStateScope scope;
            SessionStateInternal engineSessionState = executionContext.EngineSessionState;
            CommandOrigin scopeOrigin = engineSessionState.CurrentScope.ScopeOrigin;
            if (!variablePath.IsVariable)
            {
                engineSessionState.SetVariable(variablePath, value, true, scopeOrigin);
                return value;
            }
            if (executionContext.PSDebugTraceLevel > 0)
            {
                executionContext.Debugger.TraceVariableSet(variablePath.UnqualifiedPath, value);
            }
            if (variablePath.IsUnscopedVariable)
            {
                variablePath = variablePath.CloneAndSetLocal();
            }
            PSVariable newValue = engineSessionState.GetVariableItem(variablePath, out scope, scopeOrigin);
            if (newValue == null)
            {
                Collection<Attribute> attributes = (attributeAsts == null) ? new Collection<Attribute>() : GetAttributeCollection(attributeAsts);
                newValue = new PSVariable(variablePath.UnqualifiedPath, value, ScopedItemOptions.None, attributes);
                engineSessionState.SetVariable(variablePath, newValue, false, scopeOrigin);
                if (executionContext._debuggingMode > 0)
                {
                    executionContext.Debugger.CheckVariableWrite(variablePath.UnqualifiedPath);
                }
                return value;
            }
            if (attributeAsts != null)
            {
                newValue.Attributes.Clear();
                Collection<Attribute> attributeCollection = GetAttributeCollection(attributeAsts);
                value = PSVariable.TransformValue(attributeCollection, value);
                if (!PSVariable.IsValidValue(attributeCollection, value))
                {
                    ValidationMetadataException exception = new ValidationMetadataException("ValidateSetFailure", null, Metadata.InvalidValueFailure, new object[] { newValue.Name, (value != null) ? value.ToString() : "" });
                    throw exception;
                }
                newValue.SetValueRaw(value, true);
                newValue.AddParameterAttributesNoChecks(attributeCollection);
                if (executionContext._debuggingMode > 0)
                {
                    executionContext.Debugger.CheckVariableWrite(variablePath.UnqualifiedPath);
                }
                return value;
            }
            newValue.Value = value;
            return value;
        }

        private static bool ThrowStrictModeUndefinedVariable(ExecutionContext executionContext, VariableExpressionAst varAst)
        {
            if (varAst == null)
            {
                return false;
            }
            if (!executionContext.IsStrictVersion(2))
            {
                if (!executionContext.IsStrictVersion(1))
                {
                    return false;
                }
                for (Ast ast = varAst.Parent; ast != null; ast = ast.Parent)
                {
                    if (ast is ExpandableStringExpressionAst)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private class UsingResult
        {
            public object Value { get; set; }
        }
    }
}

