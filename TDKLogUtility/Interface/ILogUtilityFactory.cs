using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TDKLogUtility.Module
{
    public interface ILogUtilityFactory : IFactory
    {
    }
    public interface IFactory
    {
        ILogUtility CreateInstance();
    }
}
