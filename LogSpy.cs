﻿using Serilog;
using System.Device.Gpio;
using System.Threading.Channels;

namespace OrangeFRN
{
    public class LogSpy
    {
        public const string LogFile = "frnclient.log";

        private readonly GpioController _controller;
        private readonly Config _config;

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
            var dir = new FileInfo(LogFile).FullName.Replace(LogFile, "");
            var fileWatcher = new FileSystemWatcher(dir);
            string[] lines = File.ReadAllLines(LogFile);
            fileWatcher.NotifyFilter = NotifyFilters.LastWrite;
            fileWatcher.Filter = LogFile;
            fileWatcher.Changed += (s, e) =>
            {
                try
                {
                    var newlines = File.ReadAllLines(LogFile);
                    var delta = newlines.Length - lines.Length;
                    if (delta > 0)
                    {
                        lines = newlines;
                        foreach (var line in lines.TakeLast(delta))
                        {
                            channel.Writer.TryWrite(line);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            };
            fileWatcher.EnableRaisingEvents = true;
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var cmd = await channel.Reader.ReadAsync(cancellationToken);
                    Log.Information(cmd);
                    await ExecuteCommand(cmd);
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

        private async Task ExecuteCommand(string line)
        {
            int from = line.IndexOf(_config.CommandPrefix) + _config.CommandPrefix.Length;
            int to = line.LastIndexOf(_config.CommandSuffix);
            var length = to - from;
            if (length < 0)
            {
                Log.Information("No commands found");
                return;
            }
            var commands = line.Substring(from, length).Trim().Split(' ');
            PinValue defaultLevel = _config.DefaultLevel;
            PinValue invertedLevel = !defaultLevel;

            foreach (var command in commands)
            {
                if (_config.Commands.TryGetValue(command, out var pins))
                {
                    Log.Information("Command {command}", command);
                    ApplyState(pins, invertedLevel);
                    await Task.Delay(_config.ClickTimeMs);
                    ApplyState(_config.Pins, _config.DefaultLevel);
                    await Task.Delay(_config.ClickTimeMs);
                    Log.Information("Command done");
                }
            }
        }
        private void ApplyState(int[] pins, PinValue level)
        {
            foreach (var pin in pins)
            {
                _controller.Write(pin, level);
                Log.Information("{pin}:{level}", pin, level);
            }
        }

    }
}
