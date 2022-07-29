using System.Device.Gpio;

namespace OrangeFRN
{
    public class LogSpy
    {
        public const string Log = "frnclient.log";

        private readonly GpioController _controller;
        private readonly Config _config;

        public LogSpy(GpioController controller, Config config)
        {
            _controller = controller;
            _config = config;
        }

        public void Run()
        {
            ApplyState(_config.Pins, _config.DefaultLevel);
            var dir = new FileInfo(Log).FullName.Replace(Log, "");
            var fileWatcher = new FileSystemWatcher(dir);
            string[] lines = File.ReadAllLines(Log);
            fileWatcher.NotifyFilter = NotifyFilters.LastWrite;
            fileWatcher.Filter = Log;
            fileWatcher.Changed += async (s, e) =>
            {
                try
                {
                    var newlines = File.ReadAllLines(Log);
                    var delta = newlines.Length - lines.Length;
                    if (delta > 0)
                    {
                        lines = newlines;
                        foreach (var line in lines.TakeLast(delta))
                        {
                            Console.WriteLine(line);
                            await ExecuteCommand(line);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            };
            fileWatcher.EnableRaisingEvents = true;
        }

        private async Task ExecuteCommand(string line)
        {
            int from = line.IndexOf(_config.CommandPrefix) + _config.CommandPrefix.Length;
            int to = line.LastIndexOf(_config.CommandSuffix);
            var length = to - from;
            if (length < 0)
            {
                Console.WriteLine("[No commands found]");
                return;
            }
            var commands = line.Substring(from, length).Trim().Split(' ');
            PinValue defaultLevel = _config.DefaultLevel;
            PinValue invertedLevel = !defaultLevel;

            foreach (var command in commands)
            {
                if (_config.Commands.TryGetValue(command, out var pins))
                {
                    ApplyState(pins, invertedLevel);
                    await Task.Delay(_config.ClickTimeMs);
                    ApplyState(_config.Pins, _config.DefaultLevel);
                }
            }
        }
        private void ApplyState(int[] pins, PinValue level)
        {
            foreach (var pin in pins)
            {
                if (!_controller.IsPinOpen(pin))
                {
                    _controller.OpenPin(pin, PinMode.Output);
                }
                _controller.Write(pin, level);
                Console.WriteLine($"Pin {pin} : {level}");
            }
        }

    }
}
