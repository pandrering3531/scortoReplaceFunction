using STPC.DynamicForms.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace STPC.DynamicForms.Web.RT.Models
{
    public class RequestRepository
    {
        public int requestId{get;set;}
        public string formFieldName{get;set;}
        public Guid formPageId { get; set; }
        public Guid PageFlowId { get; set; }
        public string formName { get; set; }
        public List<Values> dataPageEvent;
    }
}