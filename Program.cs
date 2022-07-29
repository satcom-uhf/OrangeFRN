// See https://aka.ms/new-console-template for more information
using OrangeFRN;
using System.Device.Gpio;
using System.Text.Json;
using Serilog;
using System.Diagnostics;
using System.Text;

const string config = "config.json";
try
{
    Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: "{Timestamp:HH:mm:ss.fff} [{Level}] {Message}{NewLine}{Exception}")
    .CreateLogger();
    var cts = new CancellationTokenSource();
    Console.CancelKeyPress += (s, e) =>
    {
        Log.Information("Canceling...");
        cts.Cancel();
        e.Cancel = true;
    };
    Log.Information("OrangeFRN is running. Press CTRL+C to exit.");

    Config cfg = InitConfig();
    using var controller = new GpioController();
    var spy = new LogSpy(controller, cfg);
    if (args.Length > 0)
    {
        Log.Information("Opening {port}", args[0]);
        var port = new System.IO.Ports.SerialPort(args[0]);
        string buffer = "";

        port.DataReceived += (s, e) =>
        {
            var readCount = port.BytesToRead;
            var bytes = new byte[readCount];
            port.Read(bytes, 0, readCount);
            var str = Encoding.ASCII.GetString(bytes);
            var data = new string(str.Where(x =>
            (char.IsLetterOrDigit(x) && x != 'P')
            || char.IsSeparator(x)
            || x == '.').ToArray());
            Log.Information("Serial data: {data}", data);
            buffer += data;
            var msg = buffer.Length > 14 ? buffer.Substring(buffer.Length - 14) : buffer;
            if (spy.AllowToSendFeedback && !string.IsNullOrEmpty(msg))
            {
                try
                {
                    Process.Start(new ProcessStartInfo("/opt/alterfrn/FRNClientConsole.Linux-armv7.r7312", $"public \"{msg}\" frnconsole.cfg.unix"));
                    buffer = "";
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Cannot send feedback via commandline");
                }
            }
        };
        port.Open();
    }

    await spy.Run(cts.Token);
    return 0;
}
catch (Exception ex)
{
    Console.WriteLine(ex.ToString());
    return -1;
}

Config InitConfig()
{
    var cfg = new Config
    {
        Pins = new[] { 3, 19, 18, 15, 16, 2, 14, 13 },
        Commands = new()
                {
                    {"0",new[]{  15, 18} },
                    {"1",new[]{  2, 14 } },
                    {"2",new[]{  15, 14} },
                    {"3",new[]{  19, 14} },
                    {"4",new[]{  13, 16} },
                    {"5",new[]{   2, 16} },
                    {"6",new[]{  15, 16} },
                    {"7",new[]{  19, 16} },
                    {"8",new[]{  13, 18} },
                    {"9",new[]{   2, 18} },
                    {"*",new[]{  14, 18} },
                    {"#",new[]{  13, 3 } },
                   {"F1",new[]{  19, 3 } },
                   {"F2",new[]{  15, 3 } },
                   {"F3",new[]{   2, 3 } },
                }
    };
    if (!File.Exists(config))
    {
        File.WriteAllText(config, JsonSerializer.Serialize(cfg));
    }
    else
    {
        cfg = JsonSerializer.Deserialize<Config>(File.ReadAllText(config)) ?? throw new Exception("Corrupted config");
    }
    return cfg;
}