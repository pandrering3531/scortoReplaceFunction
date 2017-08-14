using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Data.Services;
using System.Data.Services.Common;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.ServiceModel.Web;
using System.Web;
using STPC.DynamicForms.Web.Models;
using System.ServiceModel;

namespace STPC.DynamicForms.Services
{
	[ServiceBehavior(IncludeExceptionDetailInFaults = true)]
	public class FormsPersistenceService : DataService<ObjectContext>
	{
		// This method is called only once to initialize service-wide policies.
		public static void InitializeService(DataServiceConfiguration config)
		{
			config.SetEntitySetAccessRule("AdCampaign", EntitySetRights.All);
			config.SetEntitySetAccessRule("AuthenticationAudit", EntitySetRights.All);
			config.SetEntitySetAccessRule("Categories", EntitySetRights.All);
			config.SetEntitySetAccessRule("DefaultForms", EntitySetRights.All);
			config.SetEntitySetAccessRule("ForbiddenPasswords", EntitySetRights.All);
			config.SetEntitySetAccessRule("Forms", EntitySetRights.All);
			config.SetEntitySetAccessRule("FormPages", EntitySetRights.All);
			config.SetEntitySetAccessRule("FormPageActions", EntitySetRights.All);
			config.SetEntitySetAccessRule("FormPageActionsByStates", EntitySetRights.All);
			config.SetEntitySetAccessRule("FormPageActionsRoles", EntitySetRights.All);
			config.SetEntitySetAccessRule("FormPageByStates", EntitySetRights.All);
			config.SetEntitySetAccessRule("FormPageResponses", EntitySetRights.All);
			config.SetEntitySetAccessRule("FormResponses", EntitySetRights.All);
			config.SetEntitySetAccessRule("FormStates", EntitySetRights.All);
			config.SetEntitySetAccessRule("Hierarchies", EntitySetRights.All);
			config.SetEntitySetAccessRule("HierarchyNodeTypes", EntitySetRights.All);
			config.SetServiceOperationAccessRule("GetHierarchiesLevels", ServiceOperationRights.AllRead);
			config.SetServiceOperationAccessRule("GetHierarchiesLevelsByAplicatioName", ServiceOperationRights.AllRead);
			config.SetEntitySetAccessRule("MenuItem", EntitySetRights.All);
			config.SetEntitySetAccessRule("MenuItemRole", EntitySetRights.All);
			config.SetEntitySetAccessRule("NodeTypeDetail", EntitySetRights.All);
			config.SetEntitySetAccessRule("Options", EntitySetRights.All);
			config.SetEntitySetAccessRule("ObjectPermissions", EntitySetRights.All);
			config.SetEntitySetAccessRule("PageEvent", EntitySetRights.All);
			config.SetEntitySetAccessRule("PageFields", EntitySetRights.All);
			config.SetEntitySetAccessRule("PageFieldTypes", EntitySetRights.All);
			config.SetEntitySetAccessRule("PageMathOperation", EntitySetRights.All);
			config.SetEntitySetAccessRule("PageResponseItems", EntitySetRights.All);
			config.SetEntitySetAccessRule("PageStrategy", EntitySetRights.All);
			config.SetEntitySetAccessRule("PageTemplates", EntitySetRights.All);
			config.SetEntitySetAccessRule("Panels", EntitySetRights.All);
			config.SetEntitySetAccessRule("PanelTemplates", EntitySetRights.All);
			config.SetEntitySetAccessRule("PasswordHistory", EntitySetRights.All);
			config.SetEntitySetAccessRule("PerformanceIndicators", EntitySetRights.All);
			config.SetEntitySetAccessRule("Request", EntitySetRights.All);
			config.SetEntitySetAccessRule("ResetQuestions", EntitySetRights.All);
			config.SetEntitySetAccessRule("Reports", EntitySetRights.All);
			config.SetEntitySetAccessRule("Roles", EntitySetRights.All);
			config.SetEntitySetAccessRule("StrategyParameter", EntitySetRights.All);
			config.SetEntitySetAccessRule("UserResetQuestions", EntitySetRights.All);
			config.SetEntitySetAccessRule("StrategySettings", EntitySetRights.All);
            config.SetEntitySetAccessRule("AplicationName", EntitySetRights.All);
            config.SetEntitySetAccessRule("TBL_CapturaInformacionBasica_GridSimuladorCuotasBE", EntitySetRights.All);
            config.SetEntitySetAccessRule("TBL_CapturaInformacionBasica_GridSimuladorCuotasFNC", EntitySetRights.All);
            config.SetEntitySetAccessRule("TBL_Desembolso_FINCOMERCIOGrid", EntitySetRights.All);
            config.SetEntitySetAccessRule("TBL_CapturaInformacionBasicaAnalista_GridGrafologia", EntitySetRights.All);

            config.DataServiceBehavior.MaxProtocolVersion = DataServiceProtocolVersion.V2;
			config.UseVerboseErrors = true;
		}

		protected override ObjectContext CreateDataSource()
		{
			var ctx = new STPC_FormsFormEntities();
			return ((IObjectContextAdapter)ctx).ObjectContext;
		}

		[WebGet]
		public IEnumerable<string> GetHierarchiesLevels()
		{
			STPC_FormsFormEntities db = new STPC_FormsFormEntities();
			return db.Hierarchies.Where(hi => hi.IsActive == true).Select(r => r.Level).Distinct();
		}

		[WebGet]
		public IEnumerable<string> GetHierarchiesLevelsByAplicatioName(int aplicationNameId)
		{
			STPC_FormsFormEntities db = new STPC_FormsFormEntities();
			return db.Hierarchies.Where(hi => hi.IsActive == true && hi.AplicationNameId == aplicationNameId).Select(r => r.Level).Distinct();
		}
	}
}