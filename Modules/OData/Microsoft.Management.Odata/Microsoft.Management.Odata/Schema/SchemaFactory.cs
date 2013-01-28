using Microsoft.Management.Odata.Common;
using Microsoft.Management.Odata.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Management.Odata.Schema
{
	internal class SchemaFactory : IItemFactory<Schema, UserContext>
	{
		private readonly Schema logicalSchema;

		private readonly List<ISchemaBuilder> schemaBuilders;

		public SchemaFactory(string mofFileName, string dispatchFileName, List<ISchemaBuilder> schemaBuilders, DSConfiguration settings)
		{
			bool enabled;
			SchemaLoader schemaLoader = new SchemaLoader();
			using (OperationTracer operationTracer = new OperationTracer(new Action<string>(TraceHelper.Current.SchemaLoadingStart), new Action<string>(TraceHelper.Current.SchemaLoadingEnd), mofFileName))
			{
				if (settings == null)
				{
					enabled = true;
				}
				else
				{
					enabled = settings.Invocation.Enabled;
				}
				bool flag = enabled;
				this.logicalSchema = schemaLoader.LoadSchemaFiles(mofFileName, dispatchFileName, flag);
				if (settings != null)
				{
					this.logicalSchema.ValidateResourceLimits(settings);
				}
			}
			this.schemaBuilders = schemaBuilders;
		}

		public Schema Create(UserContext id, string membershipId)
		{
			Schema schema;
			try
			{
				using (OperationTracer operationTracer = new OperationTracer(new Action<string>(TraceHelper.Current.UserSchemaCreationStart), new Action<string>(TraceHelper.Current.UserSchemaCreationEnd), id.Name))
				{
					Schema schema1 = new Schema(this.logicalSchema.ContainerName, this.logicalSchema.ContainerNamespace);
					foreach (ISchemaBuilder schemaBuilder in this.schemaBuilders)
					{
						schemaBuilder.Build(this.logicalSchema, schema1, id, membershipId);
					}
					TraceHelper.Current.UserSchemaCreationSucceeded(id.Name);
					schema1.Trace(string.Concat("New user schema for user ", id.Name));
					schema = schema1;
				}
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				TraceHelper.Current.UserSchemaCreationFailed(id.Name, exception.Message);
				TraceHelper.Current.DebugMessage(exception.ToTraceMessage("User Schema creation failed", new StringBuilder()).ToString());
				throw;
			}
			return schema;
		}
	}
}