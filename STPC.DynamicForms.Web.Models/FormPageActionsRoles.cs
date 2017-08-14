using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace STPC.DynamicForms.Web.Models
{
  public class FormPageActionsRoles
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Uid { get; set; }

    [MaxLength(255)]
    public string Rolename { get; set; }

    
    public Guid FormPageActionsUid { get; set; }

    

  }
}
