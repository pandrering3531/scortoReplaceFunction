using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Runtime.Serialization;

namespace STPC.DynamicForms.Web.Models
{
    //TODO: Sacar esto a algo configurable
    public enum IDTypesEnumeration : short
    { 
        CC,CE,NIP
    }

    public class CreateUserViewModel
    {
        public List<SelectListItem> IDTypes { get; set; }
        public List<SelectListItem> HierarchyLevels { get; set; }
        public List<SelectListItem> Hierarchies { get; set; }
        public string[] AppRoles { get; set; }
        public string[] UserRoles { get; set; }
        [Required]
        [Display(Name = "Número de identificación")]
        public long IDNumber { get; set; }
        [Required]
        [Display(Name = "Tipo de identificación")]
        public string IDType { get; set; }
        public User User { get; set; }
        [Required]
        [Display(Name = "Ubicación")]
        public string HierarchyId { get; set; }

        public CreateUserViewModel()
        {
            this.IDTypes = new List<SelectListItem>();
            foreach (var item in Enum.GetNames(typeof(IDTypesEnumeration)))
            {
                this.IDTypes.Add(new SelectListItem { Text = item, Value = item });
            };
        }
    }

    [MetadataType(typeof(UserViewModel))]
	 public partial class User : STPC.DynamicForms.Web.Common.Services.Users.User
    {
        public void CopyTo(ref Object target)
        {
            Type sourceType = this.GetType();
            Type destinationType = target.GetType();

            foreach (System.Reflection.PropertyInfo sourceProperty in sourceType.GetProperties())
            {
                System.Reflection.PropertyInfo destinationProperty = destinationType.GetProperty(sourceProperty.Name);
                if (destinationProperty != null)
                {
                    destinationProperty.SetValue(target, sourceProperty.GetValue(this, null), null);
                }
            }
        }
    }

    public class UserViewModel {
        [Required]
        [Display(Name = "Nombres")]
        public string GivenName { get; set; }
        [Required]
        [Display(Name = "Apellidos")]
        public string LastName { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Correo Electrónico")]
		  [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "El e-mail no es válido")]
        public string Email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; }
        
        [Display(Name = "Pregunta de seguridad")]
        public string PasswordQuestion { get; set; }

        [Display(Name = "Respuesta de seguridad")]
        public string PasswordAnswer { get; set; }
        [Required]
        [Display(Name = "Teléfono")]
        public string Phone_LandLine { get; set; }
        [Required]
        [Display(Name = "Celular")]
        public string Phone_Mobile { get; set; }


    }
    }