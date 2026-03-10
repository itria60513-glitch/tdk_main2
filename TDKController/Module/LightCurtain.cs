using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TDKController.Config;
using TDKController.Interface;

namespace TDKController.Module
{
    internal class LightCurtain : ILightCurtain
    {
        private LightCurtainConfig _config;

        public LightCurtain(LightCurtainConfig config)
        {
            _config = config;
        }

        // Implement ILightCurtain interface members here
    }
}
