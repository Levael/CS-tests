using System.Runtime;
using System.Runtime.InteropServices;


namespace APIs
{
    public static class TimeFunctions
    {
        [DllImport("winmm.dll", EntryPoint = "timeBeginPeriod", SetLastError = true)]
        public static extern uint TimeBeginPeriod(uint uMilliseconds);                                      // set "system time resolution" to minimum -- 1ms

        [DllImport("winmm.dll", EntryPoint = "timeEndPeriod", SetLastError = true)]
        public static extern uint TimeEndPeriod(uint uMilliseconds);                                        // resets it to previous value
    }

    public static class  Optimization
    {
        public static void PauseGarbageCollector()
        {
            GC.Collect();                                       // Принудительный сбор мусора перед приостановкой
            GC.WaitForPendingFinalizers();                      // Ожидание завершения финализаторов
            GCSettings.LatencyMode = GCLatencyMode.LowLatency;  // Отключение автоматического сборщика мусора
        }

        public static void ResumeGarbageCollector()
        {
            GCSettings.LatencyMode = GCLatencyMode.Interactive;
        }
    }
}
