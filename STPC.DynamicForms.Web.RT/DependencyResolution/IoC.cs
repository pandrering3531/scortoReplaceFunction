using STPC.DynamicForms.DecisionEngine;
using StructureMap;

namespace STPC.DynamicForms.Web.RT {
    public static class IoC {
        public static IContainer Initialize() {
            ObjectFactory.Initialize(x =>
                        {
                            x.Scan(scan =>
                                    {
                                        scan.TheCallingAssembly();
                                        scan.WithDefaultConventions();
                                    });
                            x.For<IDecisionEngine>().Use<STPC.DynamicForms.DecisionEngine.DecisionEngine>();
                        });
            return ObjectFactory.Container;
        }
    }
}