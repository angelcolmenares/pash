using System;
using System.Linq;
using System.Collections.Generic;
using System.ServiceModel;

namespace Microsoft.WSMan.Transfer
{
	public delegate object ExtractBodyDelegate(Type expectedType);

	public interface ITransferRequestHandler
	{
		   object HandleGet();
		   object HandlePut(ExtractBodyDelegate extractBodyCallback);
		   EndpointAddress HandleCreate(ExtractBodyDelegate extractBodyCallback);
		   void HandlerDelete();
	}
}