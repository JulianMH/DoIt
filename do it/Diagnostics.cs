using System;
using System.Windows.Navigation;
using System.Diagnostics;
using System.Windows;

namespace DoIt
{
    /// <summary>
    /// Sammelt informationen über Abstürze der App
    /// </summary>
    internal static class Diagnostics
    {
        internal static string NavigationHistory { get; set; }

        internal static void RootFrame_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            //Debugdaten, müssen vom User nicht verstanden werden.
            switch (e.NavigationMode)
            {
                case NavigationMode.Back:
                    NavigationHistory += "Navigiere zurück:\t" + e.Uri + "\n";
                    break;
                case NavigationMode.New:
                    NavigationHistory += "Navigiere weiter:\t" + e.Uri + "\n";
                    break;
                default:
                    NavigationHistory += "Navigiere:\t" + e.Uri + "\n";
                    break;
            }
        }

        #region Profiling
        internal static string times;
        private static long totalTime;
        private static Stopwatch watch;
        internal static void ProfilingStart()
        {
            times = "";
            totalTime = 0;
            watch = new Stopwatch();
            watch.Start();
        }
        internal static void ProfilingTakeTime(string info)
        {
            watch.Stop();
            times += watch.ElapsedMilliseconds + "\t" + info + "\n";
            totalTime += watch.ElapsedMilliseconds;
            watch.Reset();
            watch.Start();
        }
        internal static void ProfilingShowTimes()
        {
            MessageBox.Show(times + "\n" + totalTime);
        }
        #endregion
    }
}
