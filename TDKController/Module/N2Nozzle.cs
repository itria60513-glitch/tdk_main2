using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TDKController.Config;
using TDKController.Interface;

namespace TDKController.Module
{
    internal class N2Nozzle : IN2Nozzle
    {
        private N2NozzleConfig _config;

        public N2Nozzle(N2NozzleConfig config)
        {
            _config = config;
        }

        // Implement IN2Nozzle interface members here
    }
}
