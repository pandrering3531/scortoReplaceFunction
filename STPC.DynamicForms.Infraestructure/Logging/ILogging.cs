using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Diagnostics;

namespace STPC.DynamicForms.Infraestructure.Logging
{
  public interface ILogging
  {
    /// <summary>
    /// Writes the log.
    /// </summary>
    /// <param name="iMessage">The i message.</param>
    void WriteLog(string iMessage);
  }
}
