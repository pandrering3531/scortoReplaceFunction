using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace STPC.DynamicForms.Web.RT.Helpers
{
	public class ListStrategiesCache
	{
		private int strategieID;
		private List<string> listValues;

		public List<string> ListValues
		{
			get { return listValues; }
			set { listValues = value; }
		}


		public int StrategieID
		{
			get { return strategieID; }
			set { strategieID = value; }
		}

		public ListStrategiesCache()
		{ 
		
		}
	}
}