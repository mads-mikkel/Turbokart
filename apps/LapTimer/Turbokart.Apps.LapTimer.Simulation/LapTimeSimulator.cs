namespace Turbokart.Apps.LapTimer.Simulation
{
    public class LapTimeSimulator
    {
        internal static readonly string LapTimeFormatSpecifier = @"mm\:ss\.ffff";
        private static int currentLap = 1;
        private static int counter = 1;
        private readonly TimeSpan targetLapTime = new(0, 0, 1, 40);
        public static event EventHandler CheckeredFlag;

        public async Task SimulateRealTime(int noOfCarts, int durationInMinutes, double simSpeed, int sessionID = 1)
        {
            TimeSpan duration = TimeSpan.FromMinutes(durationInMinutes);
            duration *= simSpeed;

            double deltaPercent = 0.10;
            long deltaTicks = (long)((double)targetLapTime.Ticks * deltaPercent);
            TimeSpan delta = TimeSpan.FromTicks(deltaTicks);

            List<Kart> karts = new(noOfCarts);
            for(int i = 0; i < noOfCarts; i++)
            {
                Kart kart = new Kart(
                                    no: (i + 1).ToString(),
                                    targetLapTime: targetLapTime,
                                    delta: delta,
                                    fastness: simSpeed);
                karts.Add(kart);
                kart.CrossedTheLine += OnKartCrossedTheLineAsync;
            }

            ParallelOptions parallelOptions = new ParallelOptions() { MaxDegreeOfParallelism = noOfCarts };
            Parallel.Invoke(new ParallelOptions() { MaxDegreeOfParallelism = 1 }, async () => await StartRaceClock(duration));
            await Parallel.ForEachAsync(
                karts,
                parallelOptions,
                async (driver, ct) => { await driver.Race(); });

            await PrintResult(karts);
        }

        private async Task StartRaceClock(TimeSpan duration)
        {
            await Console.Out.WriteLineAsync($"{duration.ToString(LapTimeFormatSpecifier)} real time race started at [{DateTime.Now.ToString("HH:mm:ss")}]");
            await Task.Delay(duration);
            CheckeredFlag?.Invoke(this, new());
        }

        private async void OnKartCrossedTheLineAsync(object sender, CrossedLineEventArgs e)
        {
            if(currentLap != e.Lap)
            {
                currentLap = e.Lap;
                Console.WriteLine();
            }
            await Console.Out.WriteLineAsync($"[{counter++}: {DateTime.Now.ToString("HH:mm:ss")}]\t" + e.ToString());
        }

        private async Task PrintResult(List<Kart> karts)
        {
            var sorted = karts.OrderByDescending(d => d.CurrentLap).ThenBy(d => d.TotalTime).ToList();
            string s = $"{Environment.NewLine}--- POSITIONS ---{Environment.NewLine}";
            for(int i = 0; i < sorted.Count; i++)
            {
                s += $"{i + 1}.\tDriver No. {sorted[i].KartNo}\t{sorted[i].TotalTime.ToString(LapTimeFormatSpecifier)}\t{sorted[i].CurrentLap} Laps{Environment.NewLine}";
            }
            await Console.Out.WriteLineAsync(s);
        }
        
        private async Task<TimeSpan> CountDownOneSecond(TimeSpan currentTime)
        {
            await Task.Delay(1000);
            return currentTime.Subtract(new TimeSpan(0, 0, 1));
        }
    }
}