namespace Marada.LapTimer.Simulation
{
    public class LapTimeSimulator
    {
        internal static readonly string LapTimeFormatSpecifier = @"mm\:ss\.ffff";
        private static int currentLap = 1;
        private static int counter = 1;
        private readonly TimeSpan targetLapTime = new(0, 0, 1, 40); // A magic number.
        public static event EventHandler CheckeredFlag;

        public async Task SimulateRealTime(int noOfCarts, int durationInMinutes, double simSpeed, int sessionID = 1)
        {
            // Convert to TimeSpan so we can multiply with the simSpeed, and still use the TimeSpan:
            TimeSpan duration = TimeSpan.FromMinutes(durationInMinutes);
            duration *= simSpeed;

            // A delta is the time around a specific target time. So this is plus or minus 10% the target time. 10% because to get a spread of lap times that are "just right".
            double deltaPercent = 0.10;
            TimeSpan delta = targetLapTime * deltaPercent;

            // Make karts:
            List<Kart> karts = new(noOfCarts);
            for(int i = 0; i < noOfCarts; i++)
            {
                Kart kart = new Kart(
                                    no: (i + 1).ToString(),
                                    targetLapTime: targetLapTime,
                                    delta: delta,
                                    fastness: simSpeed);
                karts.Add(kart);

                // Connect the crossed line event of each kart to the event handler:
                kart.CrossedTheLine += OnKartCrossedTheLineAsync;
            }

            // Set up parallel execution of each kart's  race:
            ParallelOptions parallelOptions = new ParallelOptions() { MaxDegreeOfParallelism = noOfCarts };
            Parallel.Invoke(new ParallelOptions() { MaxDegreeOfParallelism = 1 }, async () => await StartRaceClock(duration));

            // Execute the race by calling the Race method on each kart, in parallel:
            await Parallel.ForEachAsync(
                karts,
                parallelOptions,
                async (driver, ct) => { await driver.Race(); });

            await PrintResult(karts);
        }

        // Race clock tha "counts down" until the checkered flag is flown:
        private async Task StartRaceClock(TimeSpan duration)
        {
            await Console.Out.WriteLineAsync($"{duration.ToString(LapTimeFormatSpecifier)} real time race started at [{DateTime.Now.ToString("HH:mm:ss")}]");
            await Task.Delay(duration);
            CheckeredFlag?.Invoke(this, new());
        }

        // Handle what happens when a kart crosses the line:
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
    }
}