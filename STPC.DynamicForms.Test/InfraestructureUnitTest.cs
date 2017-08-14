using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using STPC.DynamicForms.Infraestructure.Logging;

namespace STPC.DynamicForms.Test
{
    [TestClass]
    public class InfraestructureUnitTest
    {
        [TestMethod]
        public void WriteLog()
        {
            var log = LoggingFactory.GetInstance();
            log.WriteLog("mensaje de prueba azure");

        }
    }
}
