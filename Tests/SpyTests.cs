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
                DefaultLevel = 1,
                Pins = new[] { 3, 5, 7 }
            });
            spy.Run();
            Assert.Equal(1, target.Read(3));
            Assert.Equal(1, target.Read(5));
            Assert.Equal(1, target.Read(7));
        }

        [Fact]
        public async Task ShouldExecuteCommandsIfLogFileModified()
        {
            var cfg = new Config
            {
                ClickTimeMs = 1000,
                DefaultLevel = 0,
                Pins = new[] { 3, 5, 7 },
                Commands = new()
                {
                    {"7",new []{3,7 } },
                    {"3",new []{5} }
                }
            };
            var spy = new LogSpy(target, cfg);
            spy.Run();
            File.AppendAllLines(LogSpy.Log, new[] { "New message: MOTO 7 3!" });

            await Task.Delay(300);
            Assert.Equal(PinValue.High, target.Read(3));
            Assert.Equal(PinValue.Low, target.Read(5));
            Assert.Equal(PinValue.High, target.Read(7));

            await Task.Delay(1300);
            Assert.Equal(PinValue.Low, target.Read(3));
            Assert.Equal(PinValue.High, target.Read(5));
            Assert.Equal(PinValue.Low, target.Read(7));

            await Task.Delay(2200);
            Assert.Equal(PinValue.Low, target.Read(3));
            Assert.Equal(PinValue.Low, target.Read(5));
            Assert.Equal(PinValue.Low, target.Read(7));
        }
    }
}