using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace STPC.DynamicForms.DecisionEngine
{
    public class DecisionEngineTimeoutException:ApplicationException
    {
        public DecisionEngineTimeoutException(int timeout)
            :base("Decision Engine Timeout:" + timeout)
        {
           
        }

    }
}
