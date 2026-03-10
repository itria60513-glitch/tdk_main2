using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TDKController.Config;
using TDKController.Interface;

namespace TDKController.Module
{
    internal class CarrierIDReader : ICarrierIDReader
    {
        private CarrierIDReaderConfig _config;

        public CarrierIDReader(CarrierIDReaderConfig config)
        {
            _config = config;
        }

        // Implement ICarrierIDReader interface members here
    }
}
