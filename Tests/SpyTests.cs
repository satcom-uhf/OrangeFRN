using OrangeFRN;
using System.Device.Gpio;
using System.Text.Json;

namespace Tests
{
    [Collection("Sequential")]
    public class SpyTests
    {
        private readonly GpioController target = new GpioController(PinNumberingScheme.Logical, new FakeGpio());
            
        [Fact]
        public void DefaultStateTest()
        {
            var spy = new LogSpy(target, new Config
            {
                DefaultState = new()
                {
                    {3, (byte)PinValue.High },
                    {5, (byte)PinValue.Low },
                    {7, (byte)PinValue.High }
                }
            });
            spy.Run();
            Assert.Equal(PinValue.High, target.Read(3));
            Assert.Equal(PinValue.Low, target.Read(5));
            Assert.Equal(PinValue.High, target.Read(7));
        }

        [Fact]
        public async Task ShouldExecuteCommandsIfLogFileModified()
        {
            await ExecuteCommandTest(new Config
            {
                DefaultState = new()
                {
                    { 3, (byte)PinValue.High },
                    { 5, (byte)PinValue.Low },
                    { 7, (byte)PinValue.High }
                },
                Commands = new()
                {
                    {"bang!",
                        new (new()
                        {
                            {5, (byte)PinValue.High },
                            {7, (byte)PinValue.Low }
                        }, TimeSpan.FromSeconds(5))
                    },
                    {"booms",
                        new (new()
                        {
                            {4,(byte)PinValue.High }
                        }, TimeSpan.FromSeconds(3))
                    }
                }
            });
        }

        [Fact]
        public async Task ShouldExecuteCommandsFromJsonIfLogFileModified()
        {
            await ExecuteCommandTest(JsonSerializer.Deserialize<Config>(File.ReadAllText("config.json")));
        }
        private async Task ExecuteCommandTest(Config cfg)
        {
            var spy = new LogSpy(target, cfg);
            spy.Run();
            File.AppendAllLines(LogSpy.Log, new[] { "New message:Bang!" });
            await Task.Delay(300);
            Assert.Equal(PinValue.High, target.Read(5));
            Assert.Equal(PinValue.Low, target.Read(7));
            File.AppendAllLines(LogSpy.Log, new[] { "New message:bOOms!" });
            await Task.Delay(300);
            Assert.Equal(PinValue.High, target.Read(4));
            await Task.Delay(TimeSpan.FromSeconds(5));
            Assert.Equal(PinValue.High, target.Read(3));
            Assert.Equal(PinValue.Low, target.Read(5));
            Assert.Equal(PinValue.High, target.Read(7));
        }
    }
}