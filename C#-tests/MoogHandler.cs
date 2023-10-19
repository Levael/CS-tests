using GlobalTimeManagment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeManagment;

namespace MoogManagment
{
    public class MoogHandler
    {
        /* CLASS DESCRIPTION
        * 
        * 
        *
        */



        #region PUBLIC FIELDS

        public bool isConnecting;
        public bool isConnected;

        public bool isEngaging;
        public bool isEngaged;

        public bool isParking;
        public bool isParked;

        public bool isMoving;

        public bool isReadyForMoving;

        #endregion PUBLIC FIELDS



        #region PRIVATE FIELDS

        private Ticker _ticker;

        #endregion PRIVATE FIELDS



        #region CONSTRUCTOR and DESTRUCTOR
        public MoogHandler()
        {
            //
        }

        #endregion CONSTRUCTOR and DESTRUCTOR



        #region PUBLIC METHODS
        public bool Connect() {
            //
        }

        public bool Engage()
        {
            //
        }

        public bool Park()
        {
            //
        }

        public bool MoveByTrajectory(Trajectory trajectory, GlobalTimeManager globalTimeManager)
        {
            // At the end send all collected data to GTM.timeLine as logs (kinda)
        }

        #endregion PUBLIC METHODS



        #region PRIVATE METHODS

        private void SendKeepAlivePacket()
        {
            //
        }

        #endregion PRIVATE METHODS
    }
}
