using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace STPC.DynamicForms.Web.Models
{
  public class PageStrategy
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Uid { get; set; }

    public Guid PageId { get; set; }
    public int StrategyId { get; set; }

    public bool HasTrigger { get; set; }
    public Guid? TriggerFieldUid { get; set; }
    public bool HasResponse { get; set; }
    public Guid? ResponseFieldUid { get; set; }

    public List<StrategyParameter> StrategyParametersList { set; get; }


    public PageStrategy()
    {
    }
  }
}
