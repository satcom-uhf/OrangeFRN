// See https://aka.ms/new-console-template for more information
using OrangeFRN;
using System.Device.Gpio;
using System.Text.Json;
const string config = "config.json";
try
{
    Config cfg = InitConfig();
    using var controller = new GpioController();
    var spy = new LogSpy(controller, cfg);
    spy.Run();
    Console.WriteLine("OrangeFRN is running. Press enter to exit.");
    Console.ReadLine();
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
        Pins = new[] { 3, 5, 7 },
        Commands = new()
                {
                    {"7",new byte[]{1,0,1} },
                    {"3",new byte[]{0,1,0} }
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