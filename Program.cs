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
        Pins = new[] { 15, 16, 18, 19, 21, 22, 23, 24 },
        Commands = new()
                {
                    {"0",new[]{ 19, 18} },
                    {"1",new[]{ 22, 23} },
                    {"2",new[]{ 19, 23} },
                    {"3",new[]{ 16, 23} },
                    {"4",new[]{ 24, 21} },
                    {"5",new[]{ 22, 21} },
                    {"6",new[]{ 19, 21} },
                    {"7",new[]{ 16, 21} },
                    {"8",new[]{ 24, 18} },
                    {"9",new[]{ 22, 18} },
                    {"*",new[]{ 23, 18} },
                    {"#",new[]{ 24, 15} },
                   {"F1",new[]{ 16, 15} },
                   {"F2",new[]{ 19, 15} },
                   {"F3",new[]{ 22, 15} },
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