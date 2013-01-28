using System;
using System.Linq;
using System.Collections.Generic;
using System.ServiceModel;

namespace Microsoft.WSMan.Fault
{        
   public class FaultFactory
   {
      private readonly string _reason;
      private readonly string _code;
      private readonly string _namespace;
      private readonly string _action;

      public FaultFactory(string reason, string code, string @namespace, string action)
      {
         _reason = reason;
         _action = action;
         _namespace = @namespace;
         _code = code;
      }

      public virtual FaultException Create()
      {
         return new FaultException(_reason, FaultCode.CreateReceiverFaultCode(_code, _namespace), _action);
      }

      public virtual bool Check(FaultException exception)
      {
         return Check(exception.Code);
      }

      public virtual bool Check(FaultCode code)
      {
         return code.IsReceiverFault &&
                code.SubCode != null &&
                CheckCodeName(code) &&
                CheckCodeNamespace(code);
      }

      protected virtual bool CheckCodeNamespace(FaultCode code)
      {
         return code.SubCode.Namespace == _namespace;
      }

      protected virtual bool CheckCodeName(FaultCode code)
      {
         return code.SubCode.Name == _code;
      }
   }
}