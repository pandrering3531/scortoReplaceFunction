using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace STPC.DynamicForms.Core
{
  public class ReportParameters
  {
    public string NamePage { get; set; }
    public string NameField { get; set; }
    public string value { get; set; }
    public string CaptionField { get; set; }
  }
  public class MyViewModel
  {
    public List<ColumnViewModel> Columns { get; set; }
    public List<RowViewModel> Rows { get; set; }
  }

  public class ColumnViewModel
  {
    public string Name { get; set; }
    public string dataType { get; set; }
  }

  public class RowViewModel
  {
    public List<CellValueViewModel> Values { get; set; }
  }

  public class CellValueViewModel
  {
    public string Value { get; set; }
    public int Index { get; set; }
	 public string ColumnName { get; set; }

  }
    public class Values
    {
        public string key { get; set; }
        public string value { get; set; }
    }
}