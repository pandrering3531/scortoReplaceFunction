using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace STPC.DynamicForms.DecisionEngine
{
    public class DecisionEngineLogonException:ApplicationException
        
    {
        public DecisionEngineLogonException() : base("Authentication Error") { }

        public DecisionEngineLogonException(Exception e) : base("Authentication Error",e) { }
    }
}
