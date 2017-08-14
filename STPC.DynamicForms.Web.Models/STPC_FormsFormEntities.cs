using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace STPC.DynamicForms.Web.Models
{
	public class STPC_FormsFormEntities : DbContext
	{
		public DbSet<AdCampaign> AdCampaign { get; set; }
		public DbSet<AuthenticationAudit> AuthenticationAudit { get; set; }
		public DbSet<Category> Categories { get; set; }
		public DbSet<DefaultForm> DefaultForms { get; set; }
		public DbSet<ForbiddenPassword> ForbiddenPasswords { get; set; }
		public DbSet<Form> Forms { get; set; }
		public DbSet<FormPage> FormPages { get; set; }
		public DbSet<FormPageActions> FormPageActions { get; set; }
		public DbSet<FormPageActionsByStates> FormPageActionsByStates { get; set; }
		public DbSet<FormPageActionsRoles> FormPageActionsRoles { get; set; }
		public DbSet<FormPageByStates> FormPageByStates { get; set; }
		public DbSet<FormPageResponse> FormPageResponses { get; set; }
		public DbSet<FormResponse> FormResponses { get; set; }
		public DbSet<FormStates> FormStates { get; set; }
		public DbSet<Hierarchy> Hierarchies { get; set; }
		public DbSet<HierarchyNodeType> HierarchyNodeTypes { get; set; }
		public DbSet<MenuItem> MenuItem { get; set; }
		public DbSet<MenuItemRole> MenuItemRole { get; set; }
		public DbSet<NodeTypeDetail> NodeTypeDetail { get; set; }
		public DbSet<Option> Options { get; set; }
		public DbSet<ObjectPermissions> ObjectPermissions { get; set; }
		public DbSet<PageEvent> PageEvent { get; set; }
		public DbSet<PageField> PageFields { get; set; }
		public DbSet<PageFieldType> PageFieldTypes { get; set; }
		public DbSet<PageResponseItem> PageResponseItems { get; set; }
		public DbSet<PageStrategy> PageStrategy { get; set; }
		public DbSet<PageTemplate> PageTemplates { get; set; }
		public DbSet<Panel> Panels { get; set; }
		public DbSet<PanelTemplate> PanelTemplates { get; set; }
		public DbSet<PasswordHistory> PasswordHistory { get; set; }
		public DbSet<PerformanceIndicator> PerformanceIndicators { get; set; }
		public DbSet<Request> Request { get; set; }
		public DbSet<Report> Reports { get; set; }
		public DbSet<ResetQuestion> ResetQuestions { get; set; }
		public DbSet<Role> Roles { get; set; }
		public DbSet<StrategyParameter> StrategyParameter { get; set; }
		public DbSet<User> Users { get; set; }
		public DbSet<UserResetQuestion> UserResetQuestions { get; set; }
		public DbSet<PageMathOperation> PageMathOperation { get; set; }
		public DbSet<StrategySettings> StrategySettings { get; set; }
		public DbSet<AplicationName> AplicationName { get; set; }
        public DbSet<TBL_CapturaInformacionBasica_GridSimuladorCuotasBE> TBL_CapturaInformacionBasica_GridSimuladorCuotasBE { get; set; }
        public DbSet<TBL_CapturaInformacionBasica_GridSimuladorCuotasFNC> TBL_CapturaInformacionBasica_GridSimuladorCuotasFNC { get; set; }
        public DbSet<TBL_Desembolso_FINCOMERCIOGrid> TBL_Desembolso_FINCOMERCIOGrid { get; set; }
        public DbSet<TBL_CapturaInformacionBasicaAnalista_GridGrafologia> TBL_CapturaInformacionBasicaAnalista_GridGrafologia { get; set; }
        
		public STPC_FormsFormEntities()
			: base("LisimAbcDb")
		{
			this.Configuration.ProxyCreationEnabled = false;
		}
		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);
			modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

			//modelBuilder.Entity<Role>()
			//.HasMany<PageField>(r => r.ViewFields)
			//.WithMany(pf => pf.ViewR)
			//.Map(m =>
			//{
			//	m.ToTable("ViewRoles");
			//	m.MapLeftKey("Rolename", "AplicationNameId");
			//	m.MapRightKey("PageFieldId");
			//}
			//);

			//modelBuilder.Entity<Role>()
			//.HasMany<PageField>(r => r.EditFields)
			//.WithMany(pf => pf.EditR)
			//.Map(m =>
			//{
			//	m.ToTable("EditRoles");
			//	m.MapLeftKey("Rolename", "AplicationNameId");
			//	m.MapRightKey("PageFieldId");
			//}
			//);
		}
	}
}