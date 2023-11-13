namespace Marada.TimerTech.LaptimeSimulation
{
    internal class Kart
    {
        public string KartNo { get; set; }
        private Random generator = new();
        private TimeSpan delta;
        private TimeSpan targetLapTime;
        private TimeSpan totalTime;
        public int CurrentLap { get; set; }
        public event EventHandler<CrossedLineEventArgs> CrossedTheLine;
        private bool isLastLap;


        public Kart(string no, TimeSpan targetLapTime, TimeSpan delta, double fastness)
        {
            KartNo = no;
            this.targetLapTime = TimeSpan.FromTicks((long)(targetLapTime.Ticks * fastness));
            this.delta = TimeSpan.FromTicks((long)(delta.Ticks * fastness));
            LapTimeSimulator.CheckeredFlag += OnCheckeredFlag;
        }

        private async void OnCheckeredFlag(object? sender, EventArgs e)
        {
            await Console.Out.WriteLineAsync($"Checkered flag for {KartNo}.");
            isLastLap = true;
        }

        public async Task Race()
        {
            while(true)
            {
                // Calculate lap:
                long minDeltaTicks = targetLapTime.Ticks - delta.Ticks;
                long maxDeltaTicks = targetLapTime.Ticks + delta.Ticks;
                double x = generator.NextDouble();
                if(x > 0.8)
                {
                    minDeltaTicks -= delta.Ticks;
                }
                else if(x < 0.35)
                {
                    maxDeltaTicks += delta.Ticks;
                }
                long diff = generator.NextInt64(minDeltaTicks, maxDeltaTicks);
                TimeSpan newLapTime = new TimeSpan(diff);
                totalTime += newLapTime;
                ++CurrentLap;

                // Do lap:
                await Task.Delay(newLapTime);

                // Notify
                CrossedLineEventArgs e = new() { KartNo = KartNo, Lap = CurrentLap, LapTime = newLapTime, TotalTime = totalTime };
                CrossedTheLine?.Invoke(this, e);

                // Terminating statement:
                if(isLastLap)
                {
                    return;
                }
            }
        }

        public TimeSpan TotalTime => totalTime;
    }

    internal class CrossedLineEventArgs: EventArgs
    {
        public string KartNo { get; set; }
        public int Lap { get; set; }
        public TimeSpan LapTime { get; set; }
        public TimeSpan TotalTime { get; set; }

        public override string ToString()
        {
            return $"Kart No. {KartNo}\tLap {Lap}\t{LapTime.ToString(LapTimeSimulator.LapTimeFormatSpecifier)}\tTotal Time: {TotalTime.ToString(LapTimeSimulator.LapTimeFormatSpecifier)}";
        }
    }
}