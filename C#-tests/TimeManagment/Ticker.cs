using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TimeManagment
{
    public class Ticker
    {
        /* CLASS DESCRIPTION
         * 
         * All this class does: ticks every Xms and if there is something in "_executionQueue" -- executes all "Actions" in one(!) "List".
         * Then removes it (executed List) from queue and waits until next tick.
         * If the queue is empty -- still runs every Xms, but does nothing.
         * "_executionQueue" can be filled from outside by public "AddToExecutionQueue" method.
         * If there were a lag and actual waiting time was more than Xms -- then instead of waiting 0ms next time, code will "sleep" a "minimal sleep time", to prevent disastrous consequences.
         *
         */



        #region PUBLIC FIELDS

        public bool isRunning;

        #endregion PUBLIC FIELDS



        #region PRIVATE FIELDS

        private Queue<List<Action>> _executionQueue;

        #endregion PRIVATE FIELDS



        #region CONSTRUCTOR and DESTRUCTOR
        public Ticker()
        {
            isRunning = false;
            _executionQueue = new();
        }
        #endregion CONSTRUCTOR and DESTRUCTOR



        #region PUBLIC METHODS
        public void StartRunning() { }              //

        public void StopRunning() { }               //

        public void AddToExecutionQueue() { }       //

        #endregion PUBLIC METHODS



        #region PRIVATE METHODS
        private void ExecuteEveryTick() { }         //

        private void ExecuteNextAction() { }        //

        private void BusyWaitUntilNextTick() { }    //

        #endregion PRIVATE METHODS
    }
}
