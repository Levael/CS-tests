using System.Runtime.InteropServices;


namespace WinAPIs
{
    public static class TimeFunctions
    {
        [DllImport("winmm.dll", EntryPoint = "timeBeginPeriod", SetLastError = true)]
        public static extern uint TimeBeginPeriod(uint uMilliseconds);                                      // set "system time resolution" to minimum -- 1ms

        [DllImport("winmm.dll", EntryPoint = "timeEndPeriod", SetLastError = true)]
        public static extern uint TimeEndPeriod(uint uMilliseconds);                                        // resets it to previous value
    }
}
