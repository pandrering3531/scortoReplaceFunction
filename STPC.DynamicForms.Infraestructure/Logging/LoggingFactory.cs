using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Reflection;

namespace STPC.DynamicForms.Infraestructure.Logging
{
    public class LoggingFactory
    {

        public static ILogging GetInstance()
        {
            string factory = ConfigurationManager.AppSettings["LoggingAssembly"].ToString();
            string provider = ConfigurationManager.AppSettings["LoggingProvider"].ToString();
            Assembly assembly = Assembly.Load(factory);
            object instance = assembly.CreateInstance(factory + "." + provider);
            return (ILogging)instance;         
        }
    }
}