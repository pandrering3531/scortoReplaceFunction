namespace STPC.DynamicForms.Web.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AdCampaign",
                c => new
                    {
                        Uid = c.Guid(nullable: false, identity: true),
                        Text = c.String(),
                        Image = c.String(),
                        Hierarchy_id = c.Int(nullable: false),
                        BeginDate = c.DateTime(nullable: false),
                        EndDate = c.DateTime(nullable: false),
                        Url = c.String(),
                        ApplyToChilds = c.Boolean(nullable: false),
                        AplicationNameId = c.Int(),
                    })
                .PrimaryKey(t => t.Uid)
                .ForeignKey("dbo.AplicationName", t => t.AplicationNameId)
                .ForeignKey("dbo.Hierarchy", t => t.Hierarchy_id, cascadeDelete: true)
                .Index(t => t.AplicationNameId)
                .Index(t => t.Hierarchy_id);
            
            CreateTable(
                "dbo.Hierarchy",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 256),
                        Level = c.String(maxLength: 128),
                        NodeType = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        AplicationNameId = c.Int(),
                        Parent_Id = c.Int(),
                        AdCampaign_Uid = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Hierarchy", t => t.Parent_Id)
                .ForeignKey("dbo.AplicationName", t => t.AplicationNameId)
                .ForeignKey("dbo.AdCampaign", t => t.AdCampaign_Uid)
                .Index(t => t.Parent_Id)
                .Index(t => t.AplicationNameId)
                .Index(t => t.AdCampaign_Uid);
            
            CreateTable(
                "dbo.AplicationName",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 64),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Categories",
                c => new
                    {
                        Uid = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 128),
                        Dependency = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        AplicationNameId = c.Int(),
                    })
                .PrimaryKey(t => t.Uid)
                .ForeignKey("dbo.AplicationName", t => t.AplicationNameId)
                .Index(t => t.AplicationNameId);
            
            CreateTable(
                "dbo.Options",
                c => new
                    {
                        Uid = c.Int(nullable: false, identity: true),
                        Key = c.String(maxLength: 256),
                        Value = c.String(maxLength: 256),
                        Category_Uid = c.Int(nullable: false),
                        Option_Uid_Parent = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        AplicationNameId = c.Int(),
                    })
                .PrimaryKey(t => t.Uid)
                .ForeignKey("dbo.Categories", t => t.Category_Uid, cascadeDelete: true)
                .ForeignKey("dbo.AplicationName", t => t.AplicationNameId)
                .Index(t => t.Category_Uid)
                .Index(t => t.AplicationNameId);
            
            CreateTable(
                "dbo.PerformanceIndicator",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IndicatorType = c.Int(nullable: false),
                        Source = c.String(maxLength: 128),
                        Enabled = c.Boolean(nullable: false),
                        LastModifiedBy = c.Guid(nullable: false),
                        Modified = c.DateTime(),
                        WarningMinValue = c.Int(nullable: false),
                        WarningMaxValue = c.Int(nullable: false),
                        ViolationMinvalue = c.Int(nullable: false),
                        ViolationMaxvalue = c.Int(nullable: false),
                        AplicationNameId = c.Int(),
                        Role_Rolename = c.String(maxLength: 255),
                        Hierarchy_Id = c.Int(),
                        User_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Role", t => t.Role_Rolename)
                .ForeignKey("dbo.Hierarchy", t => t.Hierarchy_Id)
                .ForeignKey("dbo.User", t => t.User_Id)
                .ForeignKey("dbo.AplicationName", t => t.AplicationNameId)
                .Index(t => t.Role_Rolename)
                .Index(t => t.Hierarchy_Id)
                .Index(t => t.User_Id)
                .Index(t => t.AplicationNameId);
            
            CreateTable(
                "dbo.Role",
                c => new
                    {
                        Rolename = c.String(nullable: false, maxLength: 255),
                        AplicationNameId = c.Int(),
                        PageField_Uid = c.Guid(),
                        PageField_Uid1 = c.Guid(),
                    })
                .PrimaryKey(t => t.Rolename)
                .ForeignKey("dbo.AplicationName", t => t.AplicationNameId)
                .ForeignKey("dbo.PageField", t => t.PageField_Uid)
                .ForeignKey("dbo.PageField", t => t.PageField_Uid1)
                .Index(t => t.AplicationNameId)
                .Index(t => t.PageField_Uid)
                .Index(t => t.PageField_Uid1);
            
            CreateTable(
                "dbo.User",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        AplicationNameId = c.Int(),
                        Username = c.String(nullable: false, maxLength: 255),
                        GivenName = c.String(maxLength: 64),
                        LastName = c.String(maxLength: 64),
                        Email = c.String(nullable: false, maxLength: 128),
                        Password = c.String(nullable: false, maxLength: 128),
                        PasswordQuestion = c.String(maxLength: 255),
                        PasswordAnswer = c.String(maxLength: 255),
                        IsApproved = c.Boolean(nullable: false),
                        LastActivityDate = c.DateTime(nullable: false),
                        LastLoginDate = c.DateTime(nullable: false),
                        LastPasswordChangedDate = c.DateTime(nullable: false),
                        CreationDate = c.DateTime(nullable: false),
                        IsOnLine = c.Boolean(nullable: false),
                        IsLockedOut = c.Boolean(nullable: false),
                        LastLockedOutDate = c.DateTime(nullable: false),
                        FailedPasswordAttemptCount = c.Int(nullable: false),
                        FailedPasswordAttemptWindowStart = c.DateTime(nullable: false),
                        FailedPasswordAnswerAttemptCount = c.Int(nullable: false),
                        FailedPasswordAnswerAttemptWindowStart = c.DateTime(nullable: false),
                        Phone_LandLine = c.String(maxLength: 64),
                        Phone_Mobile = c.String(maxLength: 64),
                        Vacations_Start = c.DateTime(),
                        Vacations_End = c.DateTime(),
                        Token = c.String(),
                        Address = c.String(),
                        IsResetPassword = c.Boolean(nullable: false),
                        WorkStation = c.String(maxLength: 256),
                        Hierarchy_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AplicationName", t => t.AplicationNameId)
                .ForeignKey("dbo.Hierarchy", t => t.Hierarchy_Id, cascadeDelete: true)
                .Index(t => t.AplicationNameId)
                .Index(t => t.Hierarchy_Id);
            
            CreateTable(
                "dbo.PageField",
                c => new
                    {
                        Uid = c.Guid(nullable: false, identity: true),
                        FormFieldType_Uid = c.Guid(nullable: false),
                        FormFieldName = c.String(maxLength: 30),
                        FormFieldPrompt = c.String(maxLength: 1500),
                        LiteralText = c.String(maxLength: 1500),
                        IsRequired = c.Boolean(nullable: false),
                        IsHidden = c.Boolean(nullable: false),
                        IsMultipleSelect = c.Boolean(nullable: false),
                        IsEmptyOption = c.Boolean(nullable: false),
                        EmptyOption = c.String(maxLength: 255),
                        ValidExtensions = c.String(maxLength: 255),
                        ErrorExtensions = c.String(maxLength: 255),
                        Orientation = c.String(maxLength: 30),
                        OptionsMode = c.String(maxLength: 5),
                        Options = c.String(),
                        OptionsCategoryName = c.String(),
                        OptionsWebServiceUrl = c.String(maxLength: 256),
                        ListSize = c.Int(),
                        Rows = c.Int(),
                        Cols = c.Int(),
                        MaxSize = c.String(),
                        MaxSizeBD = c.String(),
                        MinSize = c.String(),
                        SortOrder = c.Int(nullable: false),
                        PanelColumn = c.Int(nullable: false),
                        PanelColumnSortOrder = c.Int(nullable: false),
                        PanelUid = c.Guid(nullable: false),
                        ShowDelete = c.Boolean(nullable: false),
                        ValidationStrategyID = c.Int(),
                        Timestamp = c.DateTime(nullable: false),
                        Style = c.String(),
                        Queryable = c.Boolean(nullable: false),
                        ToolTip = c.String(),
                        ViewRoles = c.String(),
                        EditRoles = c.String(),
                        Role_Rolename = c.String(maxLength: 255),
                        Role_Rolename1 = c.String(maxLength: 255),
                    })
                .PrimaryKey(t => t.Uid)
                .ForeignKey("dbo.Panel", t => t.PanelUid, cascadeDelete: true)
                .ForeignKey("dbo.PageFieldType", t => t.FormFieldType_Uid, cascadeDelete: true)
                .ForeignKey("dbo.Role", t => t.Role_Rolename)
                .ForeignKey("dbo.Role", t => t.Role_Rolename1)
                .Index(t => t.PanelUid)
                .Index(t => t.FormFieldType_Uid)
                .Index(t => t.Role_Rolename)
                .Index(t => t.Role_Rolename1);
            
            CreateTable(
                "dbo.Panel",
                c => new
                    {
                        Uid = c.Guid(nullable: false, identity: true),
                        SortOrder = c.Int(nullable: false),
                        Name = c.String(maxLength: 64),
                        Description = c.String(maxLength: 512),
                        DivCssStyle = c.String(maxLength: 128),
                        Columns = c.Int(nullable: false),
                        Timestamp = c.DateTime(nullable: false),
                        ViewRoles = c.String(),
                        EditRoles = c.String(),
                        Type = c.String(maxLength: 10),
                        TableDetailName = c.String(),
                        Page_Uid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Uid)
                .ForeignKey("dbo.FormPage", t => t.Page_Uid, cascadeDelete: true)
                .Index(t => t.Page_Uid);
            
            CreateTable(
                "dbo.FormPage",
                c => new
                    {
                        Uid = c.Guid(nullable: false, identity: true),
                        SortOrder = c.Int(nullable: false),
                        DisplayOrder = c.Int(nullable: false),
                        Name = c.String(maxLength: 64),
                        Description = c.String(maxLength: 512),
                        ReadOnlyState = c.Guid(),
                        ShortPath = c.String(maxLength: 50),
                        Timestamp = c.DateTime(nullable: false),
                        StrategyID = c.Int(nullable: false),
                        Form_Uid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Uid)
                .ForeignKey("dbo.Form", t => t.Form_Uid, cascadeDelete: true)
                .Index(t => t.Form_Uid);
            
            CreateTable(
                "dbo.Form",
                c => new
                    {
                        Uid = c.Guid(nullable: false, identity: true),
                        UserId = c.Guid(nullable: false),
                        Timestamp = c.DateTime(nullable: false),
                        Name = c.String(maxLength: 64),
                    })
                .PrimaryKey(t => t.Uid);
            
            CreateTable(
                "dbo.Report",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 50),
                        ReportPath = c.String(maxLength: 255),
                        Parameters = c.String(maxLength: 255),
                        IsDefaultView = c.Boolean(nullable: false),
                        AplicationNameId = c.Int(),
                        Form_Uid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Form", t => t.Form_Uid, cascadeDelete: true)
                .ForeignKey("dbo.AplicationName", t => t.AplicationNameId)
                .Index(t => t.Form_Uid)
                .Index(t => t.AplicationNameId);
            
            CreateTable(
                "dbo.PageEvent",
                c => new
                    {
                        Uid = c.Guid(nullable: false, identity: true),
                        FieldValue = c.String(maxLength: 256),
                        ListenerField = c.String(maxLength: 30),
                        FormPageUid = c.Guid(nullable: false),
                        ListenerFieldId = c.Guid(nullable: false),
                        PageFieldUid = c.Guid(nullable: false),
                        SourceField = c.String(maxLength: 30),
                        EventType = c.String(maxLength: 30),
                    })
                .PrimaryKey(t => t.Uid)
                .ForeignKey("dbo.FormPage", t => t.FormPageUid, cascadeDelete: true)
                .Index(t => t.FormPageUid);
            
            CreateTable(
                "dbo.FormPageActions",
                c => new
                    {
                        Uid = c.Guid(nullable: false, identity: true),
                        Name = c.String(maxLength: 64),
                        Description = c.String(maxLength: 512),
                        Caption = c.String(maxLength: 64),
                        IsAssociated = c.Boolean(nullable: false),
                        IsExecuteStrategy = c.Boolean(nullable: false),
                        IsPrivateResource = c.Boolean(nullable: false),
                        PageId = c.Guid(nullable: false),
                        DisplayOrder = c.Int(nullable: false),
                        Save = c.Boolean(nullable: false),
                        GoToPageId = c.Guid(),
                        FormStatesUid = c.Guid(),
                        ShowSuccessMessage = c.Boolean(nullable: false),
                        ShowFailureMessage = c.Boolean(nullable: false),
                        SuccessMessage = c.String(maxLength: 128),
                        FailureMessage = c.String(maxLength: 128),
                        StrategyID = c.Int(nullable: false),
                        Resource = c.String(maxLength: 1024),
                        SendUserParam = c.Boolean(nullable: false),
                        SendRequestIdParam = c.Boolean(nullable: false),
                        SendHierarchyIdParam = c.Boolean(nullable: false),
                        FormPage_Uid = c.Guid(),
                    })
                .PrimaryKey(t => t.Uid)
                .ForeignKey("dbo.FormStates", t => t.FormStatesUid)
                .ForeignKey("dbo.FormPage", t => t.FormPage_Uid)
                .Index(t => t.FormStatesUid)
                .Index(t => t.FormPage_Uid);
            
            CreateTable(
                "dbo.FormPageActionsRoles",
                c => new
                    {
                        Uid = c.Guid(nullable: false, identity: true),
                        Rolename = c.String(maxLength: 255),
                        FormPageActionsUid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Uid)
                .ForeignKey("dbo.FormPageActions", t => t.FormPageActionsUid, cascadeDelete: true)
                .Index(t => t.FormPageActionsUid);
            
            CreateTable(
                "dbo.FormPageActionsByStates",
                c => new
                    {
                        Uid = c.Guid(nullable: false, identity: true),
                        FormPageActionsUid = c.Guid(nullable: false),
                        FormStatesUid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Uid)
                .ForeignKey("dbo.FormPageActions", t => t.FormPageActionsUid, cascadeDelete: true)
                .ForeignKey("dbo.FormStates", t => t.FormStatesUid, cascadeDelete: true)
                .Index(t => t.FormPageActionsUid)
                .Index(t => t.FormStatesUid);
            
            CreateTable(
                "dbo.FormStates",
                c => new
                    {
                        Uid = c.Guid(nullable: false, identity: true),
                        StateName = c.String(maxLength: 128),
                        StateSymbol = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Uid);
            
            CreateTable(
                "dbo.FormPageByStates",
                c => new
                    {
                        Uid = c.Guid(nullable: false, identity: true),
                        FormStatesUid = c.Guid(nullable: false),
                        FormPageUid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Uid)
                .ForeignKey("dbo.FormStates", t => t.FormStatesUid, cascadeDelete: true)
                .ForeignKey("dbo.FormPage", t => t.FormPageUid, cascadeDelete: true)
                .Index(t => t.FormStatesUid)
                .Index(t => t.FormPageUid);
            
            CreateTable(
                "dbo.PageFieldType",
                c => new
                    {
                        Uid = c.Guid(nullable: false),
                        FieldTypeName = c.String(nullable: false, maxLength: 30),
                        SortOrder = c.Int(nullable: false),
                        FieldType = c.String(nullable: false, maxLength: 30),
                        ControlType = c.String(nullable: false, maxLength: 30),
                        ErrorMsgRequired = c.String(maxLength: 300),
                        RegExDefault = c.String(maxLength: 300),
                        ErrorMsgRegEx = c.String(maxLength: 300),
                        ValidExtensions = c.String(maxLength: 300),
                        ErrorExtensions = c.String(maxLength: 300),
                        Timestamp = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Uid);
            
            CreateTable(
                "dbo.ObjectPermissions",
                c => new
                    {
                        Uid = c.Guid(nullable: false, identity: true),
                        TableName = c.String(maxLength: 256),
                        ObjectName = c.String(maxLength: 256),
                        Permission = c.String(maxLength: 10),
                        Role_Rolename = c.String(maxLength: 255),
                    })
                .PrimaryKey(t => t.Uid)
                .ForeignKey("dbo.Role", t => t.Role_Rolename)
                .Index(t => t.Role_Rolename);
            
            CreateTable(
                "dbo.Request",
                c => new
                    {
                        RequestId = c.Int(nullable: false, identity: true),
                        Created = c.DateTime(nullable: false),
                        Updated = c.DateTime(nullable: false),
                        FormId = c.Guid(nullable: false),
                        WorkFlowState = c.Guid(),
                        PageFlowState = c.String(maxLength: 255),
                        PageFlowId = c.Guid(nullable: false),
                        CreatedBy = c.String(maxLength: 255),
                        UpdatedBy = c.String(maxLength: 255),
                        AssignedTo = c.String(maxLength: 255),
                        AplicationNameId = c.Int(),
                    })
                .PrimaryKey(t => t.RequestId)
                .ForeignKey("dbo.AplicationName", t => t.AplicationNameId)
                .Index(t => t.AplicationNameId);
            
            CreateTable(
                "dbo.MenuItem",
                c => new
                    {
                        Uid = c.Guid(nullable: false, identity: true),
                        Controller = c.String(maxLength: 64),
                        Action = c.String(maxLength: 64),
                        Message = c.String(maxLength: 128),
                        DisplayOrder = c.Int(nullable: false),
                        ParentMenuItemUid = c.Guid(),
                        FormUid = c.Guid(),
                        FormState = c.Guid(),
                        Parameters = c.String(maxLength: 1024),
                        AplicationNameId = c.Int(),
                        MenuItem_Uid = c.Guid(),
                    })
                .PrimaryKey(t => t.Uid)
                .ForeignKey("dbo.MenuItem", t => t.MenuItem_Uid)
                .ForeignKey("dbo.AplicationName", t => t.AplicationNameId)
                .Index(t => t.MenuItem_Uid)
                .Index(t => t.AplicationNameId);
            
            CreateTable(
                "dbo.AuthenticationAudit",
                c => new
                    {
                        Uid = c.Guid(nullable: false, identity: true),
                        UserName = c.String(nullable: false, maxLength: 255),
                        EventTime = c.DateTime(nullable: false),
                        WorkStation = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Uid);
            
            CreateTable(
                "dbo.DefaultForm",
                c => new
                    {
                        Uid = c.Int(nullable: false, identity: true),
                        LastModifiedBy = c.Guid(nullable: false),
                        Modified = c.DateTime(),
                        Role_Rolename = c.String(maxLength: 255),
                        Hierarchy_Id = c.Int(),
                        User_Id = c.Guid(),
                        Form_Uid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Uid)
                .ForeignKey("dbo.Role", t => t.Role_Rolename)
                .ForeignKey("dbo.Hierarchy", t => t.Hierarchy_Id)
                .ForeignKey("dbo.User", t => t.User_Id)
                .ForeignKey("dbo.Form", t => t.Form_Uid, cascadeDelete: true)
                .Index(t => t.Role_Rolename)
                .Index(t => t.Hierarchy_Id)
                .Index(t => t.User_Id)
                .Index(t => t.Form_Uid);
            
            CreateTable(
                "dbo.ForbiddenPassword",
                c => new
                    {
                        Uid = c.Int(nullable: false, identity: true),
                        LastModifiedBy = c.String(maxLength: 255),
                        LastModified = c.DateTime(nullable: false),
                        ForbiddenText = c.String(maxLength: 255),
                    })
                .PrimaryKey(t => t.Uid);
            
            CreateTable(
                "dbo.FormPageResponse",
                c => new
                    {
                        Uid = c.Guid(nullable: false),
                        Timestamp = c.DateTime(nullable: false),
                        FormPage_Uid = c.Guid(),
                        FormResponse_Uid = c.Guid(),
                    })
                .PrimaryKey(t => t.Uid)
                .ForeignKey("dbo.FormPage", t => t.FormPage_Uid)
                .ForeignKey("dbo.FormResponse", t => t.FormResponse_Uid)
                .Index(t => t.FormPage_Uid)
                .Index(t => t.FormResponse_Uid);
            
            CreateTable(
                "dbo.FormResponse",
                c => new
                    {
                        Uid = c.Guid(nullable: false),
                        Timestamp = c.DateTime(nullable: false),
                        FormSpec_Uid = c.Guid(),
                    })
                .PrimaryKey(t => t.Uid)
                .ForeignKey("dbo.Form", t => t.FormSpec_Uid)
                .Index(t => t.FormSpec_Uid);
            
            CreateTable(
                "dbo.HierarchyNodeType",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        NodeType = c.String(maxLength: 128),
                        TableName = c.String(maxLength: 256),
                        AplicationNameId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AplicationName", t => t.AplicationNameId)
                .Index(t => t.AplicationNameId);
            
            CreateTable(
                "dbo.MenuItemRole",
                c => new
                    {
                        Uid = c.Guid(nullable: false, identity: true),
                        RoleName = c.String(maxLength: 255),
                        MenuItemUid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Uid);
            
            CreateTable(
                "dbo.NodeTypeDetail",
                c => new
                    {
                        NodeTypeDetailId = c.Guid(nullable: false),
                        PageFieldTypeId = c.Guid(nullable: false),
                        FieldName = c.String(maxLength: 30),
                        FieldPrompt = c.String(maxLength: 1500),
                        IsRequired = c.Boolean(nullable: false),
                        OptionsCategoryName = c.String(maxLength: 128),
                        MaxSize = c.String(maxLength: 16),
                        MinSize = c.String(maxLength: 16),
                        SortOrder = c.Int(nullable: false),
                        Style = c.String(maxLength: 64),
                        HierarchyNodeType_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.NodeTypeDetailId)
                .ForeignKey("dbo.HierarchyNodeType", t => t.HierarchyNodeType_Id, cascadeDelete: true)
                .ForeignKey("dbo.PageFieldType", t => t.PageFieldTypeId, cascadeDelete: true)
                .Index(t => t.HierarchyNodeType_Id)
                .Index(t => t.PageFieldTypeId);
            
            CreateTable(
                "dbo.PageResponseItem",
                c => new
                    {
                        Uid = c.Guid(nullable: false),
                        ResponseStr = c.String(maxLength: 300),
                        Timestamp = c.DateTime(nullable: false),
                        PageResponse_Uid = c.Guid(),
                        PageItem_Uid = c.Guid(),
                    })
                .PrimaryKey(t => t.Uid)
                .ForeignKey("dbo.FormPageResponse", t => t.PageResponse_Uid)
                .ForeignKey("dbo.PageField", t => t.PageItem_Uid)
                .Index(t => t.PageResponse_Uid)
                .Index(t => t.PageItem_Uid);
            
            CreateTable(
                "dbo.PageStrategy",
                c => new
                    {
                        Uid = c.Guid(nullable: false, identity: true),
                        PageId = c.Guid(nullable: false),
                        StrategyId = c.Int(nullable: false),
                        HasTrigger = c.Boolean(nullable: false),
                        TriggerFieldUid = c.Guid(),
                        HasResponse = c.Boolean(nullable: false),
                        ResponseFieldUid = c.Guid(),
                    })
                .PrimaryKey(t => t.Uid);
            
            CreateTable(
                "dbo.StrategyParameter",
                c => new
                    {
                        Uid = c.Guid(nullable: false, identity: true),
                        PageStrategyUid = c.Guid(nullable: false),
                        PageFieldId = c.Guid(),
                        FieldType = c.String(maxLength: 64),
                        ParameterType = c.String(maxLength: 64),
                        ParameterName = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Uid)
                .ForeignKey("dbo.PageStrategy", t => t.PageStrategyUid, cascadeDelete: true)
                .Index(t => t.PageStrategyUid);
            
            CreateTable(
                "dbo.PageTemplate",
                c => new
                    {
                        Uid = c.Guid(nullable: false),
                        Name = c.String(maxLength: 256),
                        Description = c.String(maxLength: 512),
                        Timestamp = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Uid);
            
            CreateTable(
                "dbo.PanelTemplate",
                c => new
                    {
                        Uid = c.Guid(nullable: false),
                        Columns = c.Int(nullable: false),
                        SortOrder = c.Int(nullable: false),
                        Timestamp = c.DateTime(nullable: false),
                        PageTemplate_Uid = c.Guid(),
                    })
                .PrimaryKey(t => t.Uid)
                .ForeignKey("dbo.PageTemplate", t => t.PageTemplate_Uid)
                .Index(t => t.PageTemplate_Uid);
            
            CreateTable(
                "dbo.PasswordHistory",
                c => new
                    {
                        Uid = c.Guid(nullable: false, identity: true),
                        LastModifiedBy = c.String(nullable: false, maxLength: 128),
                        LastModified = c.DateTime(nullable: false),
                        Password = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Uid);
            
            CreateTable(
                "dbo.ResetQuestion",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Question = c.String(maxLength: 512),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.UserResetQuestion",
                c => new
                    {
                        Uid = c.Guid(nullable: false, identity: true),
                        Answer = c.String(maxLength: 512),
                        User_Id = c.Guid(),
                        Question_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Uid)
                .ForeignKey("dbo.User", t => t.User_Id)
                .ForeignKey("dbo.ResetQuestion", t => t.Question_Id)
                .Index(t => t.User_Id)
                .Index(t => t.Question_Id);
            
            CreateTable(
                "dbo.PageMathOperation",
                c => new
                    {
                        Uid = c.Guid(nullable: false, identity: true),
                        Expression = c.String(maxLength: 1050),
                        Trigger = c.String(maxLength: 256),
                        ResultField = c.Guid(nullable: false),
                        PanelUid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Uid)
                .ForeignKey("dbo.Panel", t => t.PanelUid, cascadeDelete: true)
                .Index(t => t.PanelUid);
            
            CreateTable(
                "dbo.StrategySettings",
                c => new
                    {
                        Uid = c.Guid(nullable: false, identity: true),
                        StrategyID = c.Int(nullable: false),
                        AttributeName = c.String(),
                        Value = c.String(),
                        ValueType = c.String(),
                    })
                .PrimaryKey(t => t.Uid);
            
            CreateTable(
                "dbo.TBL_CapturaInformacionBasica_GridSimuladorCuotasBE",
                c => new
                    {
                        F_Id = c.Int(nullable: false, identity: true),
                        F_RequestId = c.Int(nullable: false),
                        F_monValorObligacionBe = c.Long(nullable: false),
                        F_monValorCuotaBe = c.Long(nullable: false),
                        F_varEntidadBE = c.String(nullable: false, maxLength: 50),
                        F_varNumeroObligacionBe = c.String(nullable: false, maxLength: 10),
                        F_bitEliminado = c.Boolean(nullable: false),
                        F_bitSeleccionado = c.Boolean(nullable: false),
                        F_monSaldo = c.Long(nullable: false),
                    })
                .PrimaryKey(t => t.F_Id);
            
            CreateTable(
                "dbo.TBL_CapturaInformacionBasica_GridSimuladorCuotasFNC",
                c => new
                    {
                        F_Id = c.Int(nullable: false, identity: true),
                        F_RequestId = c.Int(nullable: false),
                        F_monValorObligacionFnc = c.Long(nullable: false),
                        F_monValorCuotaFnc = c.Long(nullable: false),
                        F_varEntidadFnc = c.String(nullable: false, maxLength: 50),
                        F_varNumeroObligacionFnc = c.String(nullable: false, maxLength: 10),
                        F_bitEliminado = c.Boolean(nullable: false),
                        F_bitSeleccionado = c.Boolean(nullable: false),
                        F_monSaldo = c.Long(nullable: false),
                        F_varPlazo = c.String(nullable: false),
                        F_varFormapago = c.String(nullable: false),
                        F_varProximoPago = c.String(nullable: false),
                        F_varTipo = c.String(nullable: false),
                        F_varProducto = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.F_Id);
            
            CreateTable(
                "dbo.TBL_Desembolso_FINCOMERCIOGrid",
                c => new
                    {
                        F_Id = c.Int(nullable: false, identity: true),
                        F_RequestId = c.Int(nullable: false),
                        F_varTipoDesembolso = c.String(maxLength: 50),
                        F_varBanco = c.String(nullable: false, maxLength: 10),
                        F_varNumeroCuenta = c.String(nullable: false, maxLength: 10),
                        F_varTipoCuenta = c.String(nullable: false, maxLength: 10),
                        F_bitEliminado = c.Boolean(nullable: false),
                        F_bitSeleccionado = c.Boolean(nullable: false),
                        F_varIdBanco = c.String(maxLength: 10),
                    })
                .PrimaryKey(t => t.F_Id);
            
            CreateTable(
                "dbo.TBL_CapturaInformacionBasicaAnalista_GridGrafologia",
                c => new
                    {
                        F_Id = c.Int(nullable: false, identity: true),
                        F_RequestId = c.Int(nullable: false),
                        F_NroRegistro = c.Int(nullable: false),
                        F_varNombreSoporte = c.String(nullable: false, maxLength: 50),
                        F_varSoporte = c.String(nullable: false),
                        F_varResultado = c.String(nullable: false, maxLength: 50),
                        F_varObservacion = c.String(nullable: false),
                        F_bitEliminado = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.F_Id);
            
            CreateTable(
                "dbo.UserRole",
                c => new
                    {
                        User_Id = c.Guid(nullable: false),
                        Role_Rolename = c.String(nullable: false, maxLength: 255),
                    })
                .PrimaryKey(t => new { t.User_Id, t.Role_Rolename })
                .ForeignKey("dbo.User", t => t.User_Id, cascadeDelete: true)
                .ForeignKey("dbo.Role", t => t.Role_Rolename, cascadeDelete: true)
                .Index(t => t.User_Id)
                .Index(t => t.Role_Rolename);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.UserRole", new[] { "Role_Rolename" });
            DropIndex("dbo.UserRole", new[] { "User_Id" });
            DropIndex("dbo.PageMathOperation", new[] { "PanelUid" });
            DropIndex("dbo.UserResetQuestion", new[] { "Question_Id" });
            DropIndex("dbo.UserResetQuestion", new[] { "User_Id" });
            DropIndex("dbo.PanelTemplate", new[] { "PageTemplate_Uid" });
            DropIndex("dbo.StrategyParameter", new[] { "PageStrategyUid" });
            DropIndex("dbo.PageResponseItem", new[] { "PageItem_Uid" });
            DropIndex("dbo.PageResponseItem", new[] { "PageResponse_Uid" });
            DropIndex("dbo.NodeTypeDetail", new[] { "PageFieldTypeId" });
            DropIndex("dbo.NodeTypeDetail", new[] { "HierarchyNodeType_Id" });
            DropIndex("dbo.HierarchyNodeType", new[] { "AplicationNameId" });
            DropIndex("dbo.FormResponse", new[] { "FormSpec_Uid" });
            DropIndex("dbo.FormPageResponse", new[] { "FormResponse_Uid" });
            DropIndex("dbo.FormPageResponse", new[] { "FormPage_Uid" });
            DropIndex("dbo.DefaultForm", new[] { "Form_Uid" });
            DropIndex("dbo.DefaultForm", new[] { "User_Id" });
            DropIndex("dbo.DefaultForm", new[] { "Hierarchy_Id" });
            DropIndex("dbo.DefaultForm", new[] { "Role_Rolename" });
            DropIndex("dbo.MenuItem", new[] { "AplicationNameId" });
            DropIndex("dbo.MenuItem", new[] { "MenuItem_Uid" });
            DropIndex("dbo.Request", new[] { "AplicationNameId" });
            DropIndex("dbo.ObjectPermissions", new[] { "Role_Rolename" });
            DropIndex("dbo.FormPageByStates", new[] { "FormPageUid" });
            DropIndex("dbo.FormPageByStates", new[] { "FormStatesUid" });
            DropIndex("dbo.FormPageActionsByStates", new[] { "FormStatesUid" });
            DropIndex("dbo.FormPageActionsByStates", new[] { "FormPageActionsUid" });
            DropIndex("dbo.FormPageActionsRoles", new[] { "FormPageActionsUid" });
            DropIndex("dbo.FormPageActions", new[] { "FormPage_Uid" });
            DropIndex("dbo.FormPageActions", new[] { "FormStatesUid" });
            DropIndex("dbo.PageEvent", new[] { "FormPageUid" });
            DropIndex("dbo.Report", new[] { "AplicationNameId" });
            DropIndex("dbo.Report", new[] { "Form_Uid" });
            DropIndex("dbo.FormPage", new[] { "Form_Uid" });
            DropIndex("dbo.Panel", new[] { "Page_Uid" });
            DropIndex("dbo.PageField", new[] { "Role_Rolename1" });
            DropIndex("dbo.PageField", new[] { "Role_Rolename" });
            DropIndex("dbo.PageField", new[] { "FormFieldType_Uid" });
            DropIndex("dbo.PageField", new[] { "PanelUid" });
            DropIndex("dbo.User", new[] { "Hierarchy_Id" });
            DropIndex("dbo.User", new[] { "AplicationNameId" });
            DropIndex("dbo.Role", new[] { "PageField_Uid1" });
            DropIndex("dbo.Role", new[] { "PageField_Uid" });
            DropIndex("dbo.Role", new[] { "AplicationNameId" });
            DropIndex("dbo.PerformanceIndicator", new[] { "AplicationNameId" });
            DropIndex("dbo.PerformanceIndicator", new[] { "User_Id" });
            DropIndex("dbo.PerformanceIndicator", new[] { "Hierarchy_Id" });
            DropIndex("dbo.PerformanceIndicator", new[] { "Role_Rolename" });
            DropIndex("dbo.Options", new[] { "AplicationNameId" });
            DropIndex("dbo.Options", new[] { "Category_Uid" });
            DropIndex("dbo.Categories", new[] { "AplicationNameId" });
            DropIndex("dbo.Hierarchy", new[] { "AdCampaign_Uid" });
            DropIndex("dbo.Hierarchy", new[] { "AplicationNameId" });
            DropIndex("dbo.Hierarchy", new[] { "Parent_Id" });
            DropIndex("dbo.AdCampaign", new[] { "Hierarchy_id" });
            DropIndex("dbo.AdCampaign", new[] { "AplicationNameId" });
            DropForeignKey("dbo.UserRole", "Role_Rolename", "dbo.Role");
            DropForeignKey("dbo.UserRole", "User_Id", "dbo.User");
            DropForeignKey("dbo.PageMathOperation", "PanelUid", "dbo.Panel");
            DropForeignKey("dbo.UserResetQuestion", "Question_Id", "dbo.ResetQuestion");
            DropForeignKey("dbo.UserResetQuestion", "User_Id", "dbo.User");
            DropForeignKey("dbo.PanelTemplate", "PageTemplate_Uid", "dbo.PageTemplate");
            DropForeignKey("dbo.StrategyParameter", "PageStrategyUid", "dbo.PageStrategy");
            DropForeignKey("dbo.PageResponseItem", "PageItem_Uid", "dbo.PageField");
            DropForeignKey("dbo.PageResponseItem", "PageResponse_Uid", "dbo.FormPageResponse");
            DropForeignKey("dbo.NodeTypeDetail", "PageFieldTypeId", "dbo.PageFieldType");
            DropForeignKey("dbo.NodeTypeDetail", "HierarchyNodeType_Id", "dbo.HierarchyNodeType");
            DropForeignKey("dbo.HierarchyNodeType", "AplicationNameId", "dbo.AplicationName");
            DropForeignKey("dbo.FormResponse", "FormSpec_Uid", "dbo.Form");
            DropForeignKey("dbo.FormPageResponse", "FormResponse_Uid", "dbo.FormResponse");
            DropForeignKey("dbo.FormPageResponse", "FormPage_Uid", "dbo.FormPage");
            DropForeignKey("dbo.DefaultForm", "Form_Uid", "dbo.Form");
            DropForeignKey("dbo.DefaultForm", "User_Id", "dbo.User");
            DropForeignKey("dbo.DefaultForm", "Hierarchy_Id", "dbo.Hierarchy");
            DropForeignKey("dbo.DefaultForm", "Role_Rolename", "dbo.Role");
            DropForeignKey("dbo.MenuItem", "AplicationNameId", "dbo.AplicationName");
            DropForeignKey("dbo.MenuItem", "MenuItem_Uid", "dbo.MenuItem");
            DropForeignKey("dbo.Request", "AplicationNameId", "dbo.AplicationName");
            DropForeignKey("dbo.ObjectPermissions", "Role_Rolename", "dbo.Role");
            DropForeignKey("dbo.FormPageByStates", "FormPageUid", "dbo.FormPage");
            DropForeignKey("dbo.FormPageByStates", "FormStatesUid", "dbo.FormStates");
            DropForeignKey("dbo.FormPageActionsByStates", "FormStatesUid", "dbo.FormStates");
            DropForeignKey("dbo.FormPageActionsByStates", "FormPageActionsUid", "dbo.FormPageActions");
            DropForeignKey("dbo.FormPageActionsRoles", "FormPageActionsUid", "dbo.FormPageActions");
            DropForeignKey("dbo.FormPageActions", "FormPage_Uid", "dbo.FormPage");
            DropForeignKey("dbo.FormPageActions", "FormStatesUid", "dbo.FormStates");
            DropForeignKey("dbo.PageEvent", "FormPageUid", "dbo.FormPage");
            DropForeignKey("dbo.Report", "AplicationNameId", "dbo.AplicationName");
            DropForeignKey("dbo.Report", "Form_Uid", "dbo.Form");
            DropForeignKey("dbo.FormPage", "Form_Uid", "dbo.Form");
            DropForeignKey("dbo.Panel", "Page_Uid", "dbo.FormPage");
            DropForeignKey("dbo.PageField", "Role_Rolename1", "dbo.Role");
            DropForeignKey("dbo.PageField", "Role_Rolename", "dbo.Role");
            DropForeignKey("dbo.PageField", "FormFieldType_Uid", "dbo.PageFieldType");
            DropForeignKey("dbo.PageField", "PanelUid", "dbo.Panel");
            DropForeignKey("dbo.User", "Hierarchy_Id", "dbo.Hierarchy");
            DropForeignKey("dbo.User", "AplicationNameId", "dbo.AplicationName");
            DropForeignKey("dbo.Role", "PageField_Uid1", "dbo.PageField");
            DropForeignKey("dbo.Role", "PageField_Uid", "dbo.PageField");
            DropForeignKey("dbo.Role", "AplicationNameId", "dbo.AplicationName");
            DropForeignKey("dbo.PerformanceIndicator", "AplicationNameId", "dbo.AplicationName");
            DropForeignKey("dbo.PerformanceIndicator", "User_Id", "dbo.User");
            DropForeignKey("dbo.PerformanceIndicator", "Hierarchy_Id", "dbo.Hierarchy");
            DropForeignKey("dbo.PerformanceIndicator", "Role_Rolename", "dbo.Role");
            DropForeignKey("dbo.Options", "AplicationNameId", "dbo.AplicationName");
            DropForeignKey("dbo.Options", "Category_Uid", "dbo.Categories");
            DropForeignKey("dbo.Categories", "AplicationNameId", "dbo.AplicationName");
            DropForeignKey("dbo.Hierarchy", "AdCampaign_Uid", "dbo.AdCampaign");
            DropForeignKey("dbo.Hierarchy", "AplicationNameId", "dbo.AplicationName");
            DropForeignKey("dbo.Hierarchy", "Parent_Id", "dbo.Hierarchy");
            DropForeignKey("dbo.AdCampaign", "Hierarchy_id", "dbo.Hierarchy");
            DropForeignKey("dbo.AdCampaign", "AplicationNameId", "dbo.AplicationName");
            DropTable("dbo.UserRole");
            DropTable("dbo.TBL_CapturaInformacionBasicaAnalista_GridGrafologia");
            DropTable("dbo.TBL_Desembolso_FINCOMERCIOGrid");
            DropTable("dbo.TBL_CapturaInformacionBasica_GridSimuladorCuotasFNC");
            DropTable("dbo.TBL_CapturaInformacionBasica_GridSimuladorCuotasBE");
            DropTable("dbo.StrategySettings");
            DropTable("dbo.PageMathOperation");
            DropTable("dbo.UserResetQuestion");
            DropTable("dbo.ResetQuestion");
            DropTable("dbo.PasswordHistory");
            DropTable("dbo.PanelTemplate");
            DropTable("dbo.PageTemplate");
            DropTable("dbo.StrategyParameter");
            DropTable("dbo.PageStrategy");
            DropTable("dbo.PageResponseItem");
            DropTable("dbo.NodeTypeDetail");
            DropTable("dbo.MenuItemRole");
            DropTable("dbo.HierarchyNodeType");
            DropTable("dbo.FormResponse");
            DropTable("dbo.FormPageResponse");
            DropTable("dbo.ForbiddenPassword");
            DropTable("dbo.DefaultForm");
            DropTable("dbo.AuthenticationAudit");
            DropTable("dbo.MenuItem");
            DropTable("dbo.Request");
            DropTable("dbo.ObjectPermissions");
            DropTable("dbo.PageFieldType");
            DropTable("dbo.FormPageByStates");
            DropTable("dbo.FormStates");
            DropTable("dbo.FormPageActionsByStates");
            DropTable("dbo.FormPageActionsRoles");
            DropTable("dbo.FormPageActions");
            DropTable("dbo.PageEvent");
            DropTable("dbo.Report");
            DropTable("dbo.Form");
            DropTable("dbo.FormPage");
            DropTable("dbo.Panel");
            DropTable("dbo.PageField");
            DropTable("dbo.User");
            DropTable("dbo.Role");
            DropTable("dbo.PerformanceIndicator");
            DropTable("dbo.Options");
            DropTable("dbo.Categories");
            DropTable("dbo.AplicationName");
            DropTable("dbo.Hierarchy");
            DropTable("dbo.AdCampaign");
        }
    }
}
