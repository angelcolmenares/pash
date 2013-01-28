namespace System.Management.Automation
{
    using System;
    using System.Collections.ObjectModel;
    using System.Management.Automation.Runspaces;

    internal class PSVariableAttributeCollection : Collection<Attribute>
    {
        private PSVariable variable;

        internal PSVariableAttributeCollection(PSVariable variable)
        {
            if (variable == null)
            {
                throw PSTraceSource.NewArgumentNullException("variable");
            }
            this.variable = variable;
        }

        internal void AddAttributeNoCheck(Attribute item)
        {
            base.InsertItem(base.Count, item);
        }

        protected override void InsertItem(int index, Attribute item)
        {
            object newValue = this.VerifyNewAttribute(item);
            base.InsertItem(index, item);
            this.variable.SetValueRaw(newValue, true);
        }

        protected override void SetItem(int index, Attribute item)
        {
            object newValue = this.VerifyNewAttribute(item);
            base.SetItem(index, item);
            this.variable.SetValueRaw(newValue, true);
        }

        private object VerifyNewAttribute(Attribute item)
        {
            object inputData = this.variable.Value;
            ArgumentTransformationAttribute attribute = item as ArgumentTransformationAttribute;
            if (attribute != null)
            {
                ExecutionContext executionContextFromTLS = LocalPipeline.GetExecutionContextFromTLS();
                EngineIntrinsics engineIntrinsics = null;
                if (executionContextFromTLS != null)
                {
                    engineIntrinsics = executionContextFromTLS.EngineIntrinsics;
                }
                inputData = attribute.Transform(engineIntrinsics, inputData);
            }
            if (!PSVariable.IsValidValue(inputData, item))
            {
                ValidationMetadataException exception = new ValidationMetadataException("ValidateSetFailure", null, Metadata.InvalidMetadataForCurrentValue, new object[] { this.variable.Name, (this.variable.Value != null) ? this.variable.Value.ToString() : "" });
                throw exception;
            }
            return inputData;
        }
    }
}

