using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace STPC.DynamicForms.Web.Models
{
	public class StrategyParameter
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public Guid Uid { get; set; }

		public Guid PageStrategyUid { get; set; }
		public Guid? PageFieldId { get; set; }
		[MaxLength(64)]
		public string FieldType { get; set; }
		[MaxLength(64)]
		public string ParameterType { get; set; }
		[MaxLength(128)]
		public string ParameterName { get; set; }

		public StrategyParameter()
		{
		}

	}
}