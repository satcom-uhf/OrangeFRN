using Serilog;
using System.Device.Gpio;
using System.Text.RegularExpressions;
using System.Threading.Channels;

namespace OrangeFRN
{
    public class LogSpy
    {
        public const string LogFile = "frnclient.log";

        private readonly GpioController _controller;
        private readonly Config _config;
        public bool AllowToSendFeedback { get; private set; }

        public Action<string, string> SetAntennaPosition { get; set; }

        public LogSpy(GpioController controller, Config config)
        {
            _controller = controller;
            _config = config;
        }

        public async Task Run(CancellationToken cancellationToken)
        {
            Channel<string> channel = Channel.CreateUnbounded<string>(new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false
            });
            foreach (var pin in _config.Pins)
            {
                if (!_controller.IsPinOpen(pin))
                {
                    _controller.OpenPin(pin, PinMode.Output);
                }
            }
            ApplyState(_config.Pins, _config.DefaultLevel);
            if (!File.Exists(LogFile))
            {
                Console.WriteLine($"Error: {LogFile} not found");
            }
            string[] lines = File.ReadAllLines(LogFile);
            await Task.Factory.StartNew(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {

                    try
                    {
                        await Task.Delay(200);
                        var newlines = File.ReadAllLines(LogFile);
                        var delta = newlines.Length - lines.Length;
                        if (delta > 0)
                        {
                            lines = newlines;
                            foreach (var line in lines.TakeLast(delta))
                            {
                                await channel.Writer.WriteAsync(line);
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Could not handle last lines from log");
                    }
                }
            });
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var cmd = await channel.Reader.ReadAsync(cancellationToken);
                    Log.Information(cmd);
                    ClickButtons(cmd);
                    var az = GetAzimuth(cmd);
                    var el = GetElevation(cmd);
                    if (az != null || el != null)
                    {
                        SetAntennaPosition(az, el);
                    }
                }
                catch (OperationCanceledException)
                {
                    //expected
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Command failed");
                }
            }
        }
        private string GetElevation(string line)
        {
            var m = Regex.Match(line, "EL (?<Angle>([+-]?[0-9]*[.])?[0-9]+)");
            if (m.Success)
            {
                return m.Groups["Angle"].Value;
            }
            return null;
        }

        private void ClickButtons(string line)
        {
            var m = Regex.Match(line, "CH (?<Keys>([0-9a-cA-C*#]*))");
            if (m.Success)
            {
                var cmd = m.Groups["Keys"].Value;
                Log.Information("Keyboard command {angle} detected", cmd);
                PinValue defaultLevel = _config.DefaultLevel;
                PinValue invertedLevel = !defaultLevel;
                AllowToSendFeedback = false;
                foreach (var key in cmd)
                {
                    if (_config.Commands.TryGetValue(key.ToString(), out var pins))
                    {
                        Thread.Sleep(_config.ClickTimeMs);
                        ApplyState(pins, invertedLevel);
                        Log.Information("Press {key}", key);
                        Thread.Sleep(_config.ClickTimeMs);
                        ApplyState(_config.Pins, _config.DefaultLevel);
                        Log.Information("Free");
                    }
                }
                AllowToSendFeedback = true;
            }
        }
        private string GetAzimuth(string line)
        {
            var m = Regex.Match(line, "AZ (?<Angle>([+-]?[0-9]*[.])?[0-9]+)");
            if (m.Success)
            {
                return m.Groups["Angle"].Value;
            }
            return null;
        }
        private void ApplyState(int[] pins, PinValue level)
        {
            foreach (var pin in pins)
            {
                _controller.Write(pin, level);
                //Log.Information("{pin}:{level}", pin, level);
            }
        }

    }
}
