using Microsoft.Management.Infrastructure;
using System;
using System.Globalization;
using System.Management.Automation;

namespace Microsoft.Management.Infrastructure.CimCmdlets
{
	internal class CimMethodResultObserver : CimResultObserver<CimMethodResultBase>
	{
		internal string ClassName
		{
			get;
			set;
		}

		internal string MethodName
		{
			get;
			set;
		}

		public CimMethodResultObserver(CimSession session, IObservable<object> observable) : base(session, observable)
		{
		}

		public CimMethodResultObserver(CimSession session, IObservable<object> observable, CimResultContext context) : base(session, observable, context)
		{
		}

		public override void OnNext(CimMethodResultBase value)
		{
			DebugHelper.WriteLogEx();
			string str = null;
			PSObject pSObject = null;
			CimMethodResult cimMethodResult = value as CimMethodResult;
			if (cimMethodResult == null)
			{
				CimMethodStreamedResult cimMethodStreamedResult = value as CimMethodStreamedResult;
				if (cimMethodStreamedResult != null)
				{
					str = "Microsoft.Management.Infrastructure.CimMethodStreamedResult";
					pSObject = new PSObject();
					pSObject.Properties.Add(new PSNoteProperty("ParameterName", cimMethodStreamedResult.ParameterName));
					pSObject.Properties.Add(new PSNoteProperty("ItemType", (object)cimMethodStreamedResult.ItemType));
					pSObject.Properties.Add(new PSNoteProperty("ItemValue", cimMethodStreamedResult.ItemValue));
				}
			}
			else
			{
				str = "Microsoft.Management.Infrastructure.CimMethodResult";
				pSObject = new PSObject();
				foreach (CimMethodParameter outParameter in cimMethodResult.OutParameters)
				{
					pSObject.Properties.Add(new PSNoteProperty(outParameter.Name, outParameter.Value));
				}
			}
			if (pSObject != null)
			{
				pSObject.Properties.Add(new PSNoteProperty("PSComputerName", base.CurrentSession.ComputerName));
				pSObject.TypeNames.Insert(0, str);
				object[] className = new object[3];
				className[0] = str;
				className[1] = this.ClassName;
				className[2] = this.MethodName;
				pSObject.TypeNames.Insert(0, string.Format(CultureInfo.InvariantCulture, "{0}#{1}#{2}", className));
				base.OnNextCore(pSObject);
			}
		}
	}
}