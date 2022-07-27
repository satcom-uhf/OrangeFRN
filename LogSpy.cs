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
            ApplyState(_config.DefaultState);
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
                            await ExecuteCommand(line);
                        }
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex);
                }
            };
            fileWatcher.EnableRaisingEvents = true;
        }

        private async Task ExecuteCommand(string line)
        {
            foreach (var item in _config.Commands)
            {
                if (line.Contains(item.Key, StringComparison.InvariantCultureIgnoreCase))
                {
                    ApplyState(item.Value.pins);
                    await Task.Delay(item.Value.time);
                    ApplyState(_config.DefaultState);
                    return;
                }
            }
        }
        private void ApplyState(Dictionary<int, PinValue> pins)
        {
            foreach (var item in pins)
            {
                if (!_controller.IsPinOpen(item.Key))
                {
                    _controller.OpenPin(item.Key, PinMode.Output);
                }
                _controller.Write(item.Key, item.Value);
            }
        }

    }
}
