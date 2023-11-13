using Marada.TimerTech.LaptimeSimulation;

namespace Turbokart.Apps.LapTimer.ManualTests
{
    internal class LapTimerTestProgram
    {
        static TimeSpan oneSecond = TimeSpan.FromSeconds(1);
        static string dot = "\u2B24";
        static string tod = "\u25EF";

        static async Task Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.Unicode;
            TimeSpan t = new(0, 10, 0);
            Console.ForegroundColor = ConsoleColor.Red;
            await Task.Delay(oneSecond);
            await StartRace();
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(tod + tod + tod + tod + tod);
            Console.ForegroundColor = ConsoleColor.Green;

            LapTimeSimulator simulator = new LapTimeSimulator();
            await simulator.SimulateRealTime(5, 10, 0.1);
        }

        private static async Task StartRace()
        {
            TimeSpan t = new(0, 0, 5);
            while(t > TimeSpan.Zero)
            {
                await Task.Delay(1000);
                t = t.Subtract(oneSecond);
                await Console.Out.WriteAsync(dot);
            }
            await Task.Delay(2000);
        }
    }
}