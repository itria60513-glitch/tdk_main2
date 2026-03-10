using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TDKLogUtility.Module;

namespace EFEMGUI
{
    internal class TestDiLog
    {
        private ILogUtility _log;
        public TestDiLog(ILogUtility log)
        {
            _log = log;
        }

        public void WriteLog(string message)
        {
            _log.WriteLog("TDK", "test di log: "+message);
        }
    }
}
