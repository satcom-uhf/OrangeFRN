// See https://aka.ms/new-console-template for more information
using System.Device.Gpio;
const string log = "frnclient.log";
try
{
    //var lines = await File.ReadAllLinesAsync("frnclient.log");
    //foreach (var line in lines)
    //{
    //    Console.WriteLine(line);
    //}
    ////if (args.Length == 0)
    ////{
    ////    Console.WriteLine("OrangeFRN <pin>");
    ////    return 0;
    ////}
    ////Console.WriteLine("Blinking LED. Press Ctrl+C to end.");
    ////var pin = int.Parse(args[0]);
    ////using var controller = new GpioController();
    ////controller.OpenPin(pin, PinMode.Output);
    ////bool ledOn = true;
    ////while (true)
    ////{
    ////    controller.Write(pin, ((ledOn) ? PinValue.High : PinValue.Low));
    ////    await Task.Delay(2000);
    ////    ledOn = !ledOn;
    ////}
    FileSystemWatcher _fileWatcher = new FileSystemWatcher(new FileInfo(log).FullName.Replace(log, ""));
    string[] lines = File.ReadAllLines(log);
    _fileWatcher.NotifyFilter = NotifyFilters.LastWrite;
    _fileWatcher.Filter = log;
    _fileWatcher.Changed += (s, e) =>
    {
        try
        {
            var newlines = File.ReadAllLines(log);
            var delta = newlines.Length - lines.Length;
            if (delta > 0)
            {
                lines = newlines;
                foreach (var line in lines.TakeLast(delta))
                {
                    Console.WriteLine(line);
                }
            }            
        }
        catch
        {

        }
    };
    _fileWatcher.EnableRaisingEvents = true;
    Console.WriteLine("Ok, I'm watching for new records in the log file 0_0 ");
    Console.ReadLine();
    return 0;
}
catch (Exception ex)
{
    Console.WriteLine(ex.ToString());
    return -1;
}
