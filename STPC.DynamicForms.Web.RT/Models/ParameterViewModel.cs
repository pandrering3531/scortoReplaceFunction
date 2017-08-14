using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace STPC.DynamicForms.Web.RT.Models
{
  public class ParameterViewModel
  {
    public string Name { get; set; }

    public string Type { get; set; }

    public string InitialValue { get; set; }

    public string Description { get; set; }

    public bool IsInput { get; set; }

    public bool IsOutput { get; set; }

    public string Constraints { get; set; }

    public string SelectedValue { get; set; }

    public string ParameterType { get; set; }

    public ParameterViewModel() { }

  }
}