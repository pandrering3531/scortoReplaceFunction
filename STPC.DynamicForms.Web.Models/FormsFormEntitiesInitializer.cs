using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using System.Data.Entity.Migrations;

namespace STPC.DynamicForms.Web.Models
{
  public class FormsFormEntitiesInitializer : CreateDatabaseIfNotExists<STPC_FormsFormEntities> //  DropCreateDatabaseAlways<STPC_FormsFormEntities>//
  {
    protected override void Seed(STPC_FormsFormEntities context)
    {
      #region PageFieldType

      var formTypes = new List<PageFieldType>
            {
                new PageFieldType { Uid=Guid.NewGuid(),  FieldType="TextBox", FieldTypeName ="Texto", ControlType="Generic", SortOrder=1},
                new PageFieldType { Uid=Guid.NewGuid(),  FieldType="TextBox", FieldTypeName ="Número", ControlType="Generic", SortOrder=2, RegExDefault=@"^[0-9,-\.]+$", ErrorMsgRegEx="El valor del campo %FormFieldName% debe ser numérico"},
                new PageFieldType { Uid=Guid.NewGuid(),  FieldType="Calendar", FieldTypeName ="Fecha", ControlType="Generic", SortOrder=3, RegExDefault=@"^(0[1-9]|[12][0-9]|3[01])[- /.](0[1-9]|1[012])[- /.](19|20)\d\d", ErrorMsgRegEx="El valor del campo %FormFieldName% debe ser una fecha"},
                new PageFieldType { Uid=Guid.NewGuid(),  FieldType="TextBox", FieldTypeName ="Correo Electrónico", ControlType="Generic", SortOrder=4,  RegExDefault="^([\\w-\\.]+)@((\\[[0-9]{1,3}\\.[0-9]{1,3}\\.[0-9]{1,3}\\.)|(([\\w-]+\\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\\]?)$",  ErrorMsgRegEx="El valor del campo %FormFieldName% debe ser una dirección de correo electrónico válida"},
                new PageFieldType { Uid=Guid.NewGuid(),  FieldType="RadioList", FieldTypeName ="Lista de opciones únicas", ControlType="ChoiceList", SortOrder=5},
                new PageFieldType { Uid=Guid.NewGuid(),  FieldType="CheckBoxList", FieldTypeName ="Lista de opciones múltiples", ControlType="ChoiceList", SortOrder=6},
                new PageFieldType { Uid=Guid.NewGuid(),  FieldType="TextArea", FieldTypeName ="Area de texto", ControlType="TextArea", SortOrder=7},
                new PageFieldType { Uid=Guid.NewGuid(),  FieldType="SelectList", FieldTypeName ="Lista de selección", ControlType="SelectList", SortOrder=8},
                new PageFieldType { Uid=Guid.NewGuid(),  FieldType="CheckBox", FieldTypeName ="Caja de chequeo", ControlType="CheckBox", SortOrder=9},
                new PageFieldType { Uid=Guid.NewGuid(),  FieldType="FileUpload", FieldTypeName ="Subir Archivo", ControlType="FileUpload", SortOrder=10},
                new PageFieldType { Uid=Guid.NewGuid(),  FieldType="Literal", FieldTypeName ="Texto literal", ControlType="Literal", SortOrder=11},
                new PageFieldType { Uid=Guid.NewGuid(),  FieldType="LHyperLink", FieldTypeName ="Hipervinculo", ControlType="LHyperLink", SortOrder=12},
                new PageFieldType { Uid=Guid.NewGuid(),  FieldType="TextBox", FieldTypeName ="Moneda", ControlType="Generic", SortOrder=13, RegExDefault=@"^[0-9,-\.]+$", ErrorMsgRegEx="El valor del campo %FormFieldName% debe ser numérico"},
                new PageFieldType { Uid=Guid.NewGuid(),  FieldType="TextBox", FieldTypeName ="Solo Texto", ControlType="Generic", SortOrder=14, ErrorMsgRegEx="El valor del campo %FormFieldName% debe ser solo texto"},
                new PageFieldType { Uid=Guid.NewGuid(),  FieldType="Blank", FieldTypeName ="Espacio en Blanco", ControlType="Blank", SortOrder=15},
            };
      formTypes.ForEach(ft => ft.ErrorMsgRequired = "El campo %FormFieldName% es obligatorio");
      formTypes.ForEach(ft => ft.Timestamp = DateTime.Now);
      formTypes.ForEach(s => context.PageFieldTypes.Add(s));
      context.SaveChanges();

      #endregion PageFieldType

      #region Aplication
      var aplication = new AplicationName
      {
        Name = "LiSimAbc"
      };
      context.AplicationName.Add(aplication);
      context.SaveChanges();
      #endregion

      var aplicationNameId = context.AplicationName.FirstOrDefault();

      #region Hierarchy

      new List<Hierarchy> {
                new Hierarchy { 
                    Name="Nodo inicial", Level="", NodeType=0,IsActive=true,AplicationNameId= aplicationNameId.Id
                       }
      }.ForEach(h => context.Hierarchies.Add(h));
      context.SaveChanges();

      #endregion Hierarchy

      #region Role


      new List<Role> {new Role { Rolename="Editor", AplicationNameId=aplicationNameId.Id},
					 new Role { Rolename="Auxiliar", AplicationNameId=aplicationNameId.Id},
					 new Role { Rolename="Gerente", AplicationNameId=aplicationNameId.Id},
					 new Role { Rolename="Director", AplicationNameId=aplicationNameId.Id},
					 new Role { Rolename="Co-Administrador", AplicationNameId=aplicationNameId.Id},
					 new Role { Rolename="Administrador", AplicationNameId=aplicationNameId.Id}}.ForEach(r => context.Roles.Add(r));
      context.SaveChanges();

      #endregion Role

      #region Menu Items

      new List<MenuItem> {
        new MenuItem { Uid = Guid.NewGuid(), Controller="Home", Action="Index", Message="Inicio", DisplayOrder=1,AplicationNameId=aplicationNameId.Id},
        new MenuItem { Uid = Guid.NewGuid(), Controller=null, Action=null, Message="Administración Dt", DisplayOrder=2,AplicationNameId=aplicationNameId.Id},
        new MenuItem { Uid = Guid.NewGuid(), Controller=null, Action=null, Message="Nuevas Solicitudes", DisplayOrder=3,AplicationNameId=aplicationNameId.Id},
        new MenuItem { Uid = Guid.NewGuid(), Controller="Request", Action="RequestsByParamProcedure", Message="Mis Solicitudes", DisplayOrder=4,AplicationNameId=aplicationNameId.Id,Parameters="spGetAvailableInterviewsUser"},
        new MenuItem { Uid = Guid.NewGuid(), Controller="Request", Action="List", Message="Busqueda de Solicitudes", DisplayOrder=5,AplicationNameId=aplicationNameId.Id},
        
      }.ForEach(item => context.MenuItem.Add(item));
      context.SaveChanges();

      //Crea las opciones de me´nú del DT
      var uidAdministracionDt = context.MenuItem.Where(e => e.Message == "Administración Dt").FirstOrDefault();
      new List<MenuItem> {
        new MenuItem { Uid = Guid.NewGuid(), Controller="Account", Action="List", Message="Usuarios", DisplayOrder=6,ParentMenuItemUid=uidAdministracionDt.Uid,AplicationNameId=aplicationNameId.Id},
        new MenuItem { Uid = Guid.NewGuid(), Controller="ObjectPermissions", Action="Index", Message="Configurar permisos", DisplayOrder=8,ParentMenuItemUid=uidAdministracionDt.Uid,AplicationNameId=aplicationNameId.Id},
        new MenuItem { Uid = Guid.NewGuid(), Controller="StrategySettings", Action="Index", Message="Configurar estrategias", DisplayOrder=7,ParentMenuItemUid=uidAdministracionDt.Uid,AplicationNameId=aplicationNameId.Id},
        new MenuItem { Uid = Guid.NewGuid(), Controller="PerformanceIndicator", Action="List", Message="Indicadores", DisplayOrder=4,ParentMenuItemUid=uidAdministracionDt.Uid,AplicationNameId=aplicationNameId.Id},
        new MenuItem { Uid = Guid.NewGuid(), Controller="Hierarchies", Action="List", Message="Jerarquias", DisplayOrder=5,ParentMenuItemUid=uidAdministracionDt.Uid,AplicationNameId=aplicationNameId.Id},
        new MenuItem { Uid = Guid.NewGuid(), Controller="Categories", Action="List", Message="Categorias", DisplayOrder=2,ParentMenuItemUid=uidAdministracionDt.Uid,AplicationNameId=aplicationNameId.Id},
        new MenuItem { Uid = Guid.NewGuid(), Controller="Form", Action="List", Message="Formularios", DisplayOrder=3,ParentMenuItemUid=uidAdministracionDt.Uid,AplicationNameId=aplicationNameId.Id},
        new MenuItem { Uid = Guid.NewGuid(), Controller="RedisCache", Action="Index", Message="Administracion REDIS caché", DisplayOrder=9,ParentMenuItemUid=uidAdministracionDt.Uid,AplicationNameId=aplicationNameId.Id},
        new MenuItem { Uid = Guid.NewGuid(), Controller="Campaign", Action="List", Message="Campañas", DisplayOrder=1,ParentMenuItemUid=uidAdministracionDt.Uid,AplicationNameId=aplicationNameId.Id},
        
      }.ForEach(item => context.MenuItem.Add(item));
      context.SaveChanges();


      for (int i = 0; i < context.MenuItem.Local.Count; i++)
      {
        if (context.MenuItem.Local[i].Message.Equals("Nuevas Solicitudes"))
        {
          context.MenuItem.Add
            (
          new MenuItem { Uid = Guid.NewGuid(), Controller = null, Action = null, Message = "Tipo 1", DisplayOrder = 1, ParentMenuItemUid = context.MenuItem.Local[i].Uid, AplicationNameId = aplicationNameId.Id }
            );

          break;
        }
      }

      context.SaveChanges();

      #endregion Menu Items

      #region Menu Item Role

      List<MenuItemRole> _lstNewMenuItemRole = new List<MenuItemRole>();

      for (int i = 0; i < context.MenuItem.Local.Count; i++)
      {
        _lstNewMenuItemRole.Add
          (
        new MenuItemRole { Uid = Guid.NewGuid(), MenuItemUid = context.MenuItem.Local[i].Uid, RoleName = "Administrador" }
          );
      }

      _lstNewMenuItemRole.ForEach(item => context.MenuItemRole.Add(item));
      context.SaveChanges();

      #endregion Menu Item Role

      #region default user

      var user =
      new User
      {
        Id = Guid.NewGuid(),
        //ApplicationName="LiSim",
        Username = "CC1",
        GivenName = "Administrador",
        LastName = "Base",
        Email = "no@email.com",
        Password = "EcxVTGU9VcDxBizxs63uZlHQDaw=",
        PasswordQuestion = "1+1",
        PasswordAnswer = "8NVig50gzQZZnxvCYIvATptNATk=",
        IsApproved = true,
        IsLockedOut = false,
        IsOnLine = false,
        CreationDate = DateTime.Now,
        LastActivityDate = DateTime.Now,
        LastLockedOutDate = DateTime.Now,
        LastLoginDate = DateTime.Now,
        LastPasswordChangedDate = DateTime.Now,
        Phone_Mobile = "5555555",
        Phone_LandLine = "5555555",
        FailedPasswordAnswerAttemptCount = 0,
        FailedPasswordAnswerAttemptWindowStart = DateTime.Now,
        FailedPasswordAttemptCount = 0,
        FailedPasswordAttemptWindowStart = DateTime.Now,
        Roles = new List<Role>(),
        Hierarchy = context.Hierarchies.FirstOrDefault(),
        AplicationNameId = aplicationNameId.Id
      };

      context.Users.Add(user);
      context.SaveChanges();
      user.Roles = context.Roles.ToList();
      context.Entry(user).State = System.Data.EntityState.Modified;
      context.SaveChanges();

      #endregion default user

      context.Database.ExecuteSqlCommand(Properties.Settings.Default.SPMisSolicitudes);
      context.Database.ExecuteSqlCommand(Properties.Settings.Default.SPInsertRequest);
      context.Database.ExecuteSqlCommand(Properties.Settings.Default.SPSearchRequest);
      context.Database.ExecuteSqlCommand(Properties.Settings.Default.SPSelect_RequestById);
      context.Database.ExecuteSqlCommand(Properties.Settings.Default.SPUpdate_Request);
      context.Database.ExecuteSqlCommand(Properties.Settings.Default.SPGetUsersByStates);

    }
  }
}