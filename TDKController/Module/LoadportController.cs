using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TDKController.Config;
using TDKController.Interface;

namespace TDKController.Module
{
    internal class LoadportController : ILoadportController
    {
        private LoadportControllerConfig _config;

        public LoadportController(LoadportControllerConfig config)
        {
            _config = config;
        }

        // Implement ILoadportController interface members here
    }
}
