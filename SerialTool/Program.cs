// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, Turbina!");
using var port = new System.IO.Ports.SerialPort(args[0], 115200);
port.Open();
var line = "";
do
{
    line = Console.ReadLine();
    port.Write(line);
} while (line.ToLower() != "exit");
port.Close();
