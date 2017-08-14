using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Xml;
using TestScoringFlowClient.ScoringService;
using STPC.DynamicForms.DecisionEngine;

namespace TestScoringFlowClient
{
    class Program
    {
        static void Main(string[] args)
        {

            STPC.DynamicForms.DecisionEngine.DecisionEngine de = new STPC.DynamicForms.DecisionEngine.DecisionEngine("1", "1");
            var x = de.GetStrategyList();
            foreach(StrategyData v in x){
                Console.WriteLine("{0}, {1}, {2}, {3}", v.Id, v.Name, v.Termination_date, v.Description);
            }

            //STPC.DynamicForms.DecisionEngine.DecisionEngine de = new STPC.DynamicForms.DecisionEngine.DecisionEngine("1","1");
            string message ="";
            if (de.ValidateData(36, "Edad", 1, 10, out message))
                Console.WriteLine("Validation ok : " + message);
            else
                Console.WriteLine("Validation error : " + message);
            
             var y = de.GetList(31, "DS1p_Nombre", "Sexo","//Catalogo");
              foreach (string v in y)
              {
                  Console.WriteLine(v);
              }
              
            var z = de.GetDictionary(34, "SPcategoria", "Sexo", "//row");
            foreach (string v in z.Keys)
            {
                Console.WriteLine("{0}  -  {1} ", v, z[v] );
            }

            
            return;
            ScoringService.ScoringService service = new ScoringService.ScoringService();
            string ticket = service.LoginUser("1", "1");
            service.TicketValue = new Ticket();
            service.TicketValue.TicketId = ticket;
            string strategiesXML = service.GetStrategyList();
            Console.WriteLine(strategiesXML);
            Console.Write("Please enter strategy id: ");
            int stratId = Int32.Parse(Console.ReadLine());
            string val = service.GetStrategyExampleXML(stratId);
            Console.WriteLine(val);
            long appId = service.CreateApplicationByStrategyId(stratId, "<?xml version=\"1.0\" encoding=\"utf-8\"?><application_data><DS1p_Nombre>Sexo</DS1p_Nombre></application_data>");

            while (true)
            {
                string status = service.GetApplicationStatus(appId);
                // Of course, please use correct status Xml parsing, this is just for example
                if (status.Contains("processed") || status.Contains("processed_with_errors")) break;
                Console.WriteLine(status);
                Thread.Sleep(1000);
            }
            string resultXML = service.GetApplicationResult(appId);
            Console.WriteLine(resultXML);
            Console.WriteLine("Work done! Press any key to exit...");
            Console.ReadKey();

        }
    }
}
