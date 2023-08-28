using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Timings
{
    static public class GlobalTimeManager
    {
        static private List<ExecutableObject> _execution_queue;
        static private int _time_step_ms;


        static GlobalTimeManager ()
        {
            _execution_queue = new();
            _time_step_ms = 1;
        }

        static public void StartExecution ()
        {

        }

    }

    public class ExecutableObject
    {
        //private bool _is_paralel_with_anyone { get; set; }

        void Execute ()
        {

        }
    }
}
