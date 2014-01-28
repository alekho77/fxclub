using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace fxanalysis
{
    class Test : ICommand
    {
        public bool Execute(IList<string> cmd_params)
        {
            if (cmd_params.Count == 0)
            {
                return true;
            }
            return false;
        }
    }
}
