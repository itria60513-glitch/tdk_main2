using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TDKController.Config;
using TDKController.Interface;

namespace TDKController.Module
{
    internal class E84HandShaking : IE84HandShaking
    {
        private E84HandShakingConfig _config;

        public E84HandShaking(E84HandShakingConfig config)
        {
            _config = config;
        }

        // Implement IE84HandShaking interface members here
    }
}
