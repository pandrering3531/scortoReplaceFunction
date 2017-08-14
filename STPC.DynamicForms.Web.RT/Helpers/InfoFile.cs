using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace STPC.DynamicForms.Web.RT.Helpers
{
	/// <summary>
	/// Clase que retorna la información del archivo cargado en la solicitud
	/// </summary>
	public class InfoFile
	{
		public string FileName { get; set; }
		public Guid UidUpoadFile{ get; set; }

		public InfoFile(string fileName, Guid uidUploadFile)
		{
			this.FileName = fileName;
			this.UidUpoadFile = uidUploadFile;
		}
		public InfoFile()
		{
			
		}
	}


}