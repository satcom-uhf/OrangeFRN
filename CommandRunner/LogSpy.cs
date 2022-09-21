using Serilog;
using System.Device.Gpio;
using System.Threading.Channels;

namespace OrangeFRN
{
    public class LogSpy
    {
        public const string LogFile = "frnclient.log";

        private readonly GpioController _controller;
        private readonly Config _config;
        public bool AllowToSendFeedback { get; private set; }

        public Action<string,string> SetAntennaPosition { get; set; }

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
                    if (!ExecuteCommand(cmd))
                    {
                        SetAntenna(cmd);
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

        private bool ExecuteCommand(string line)
        {
            int from = line.IndexOf(_config.CommandPrefix) + _config.CommandPrefix.Length;
            int to = line.LastIndexOf(_config.CommandSuffix);
            var length = to - from;
            if (length < 0)
            {
                Log.Information("No key clicks detected");
                return false;
            }
            var keys = line.Substring(from, length).Trim().Split(' ');
            PinValue defaultLevel = _config.DefaultLevel;
            PinValue invertedLevel = !defaultLevel;
            AllowToSendFeedback = false;
            foreach (var key in keys)
            {
                if (_config.Commands.TryGetValue(key, out var pins))
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
            return true;
        }
        private void SetAntenna(string line)
        {
            if (!line.Contains("ANT "))
            {
                Log.Information("No antenna commands detected");
                return;
            }
            var keys = line.Replace("ANT","").Trim().Split(' ');
            SetAntennaPosition(keys[0], keys[1]);
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
