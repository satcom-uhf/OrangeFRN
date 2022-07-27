using OrangeFRN;
using System.Device.Gpio;

namespace Tests
{
    public class SpyTests
    {
        [Fact]
        public void DefaultStateTest()
        {
            using var target = new GpioController(PinNumberingScheme.Logical, new FakeGpio());
            var spy = new LogSpy(target, new Config
            {
                DefaultState = new()
                {
                    {3, PinValue.High },
                    {5, PinValue.Low },
                    {7, PinValue.High }
                }
            });
            spy.Run();
            Assert.Equal(PinValue.High, target.Read(3));
            Assert.Equal(PinValue.Low, target.Read(5));
            Assert.Equal(PinValue.High, target.Read(7));
        }

        [Fact]
        public async Task ExecuteCommandTest()
        {
            using var target = new GpioController(PinNumberingScheme.Logical, new FakeGpio());
            var spy = new LogSpy(target, new Config
            {
                DefaultState = new()
                {
                    { 3, PinValue.High },
                    { 5, PinValue.Low },
                    { 7, PinValue.High }
                },
                Commands = new()
                {
                    {"bang!",
                        (new()
                        {
                            {5, PinValue.High },
                            {7, PinValue.Low }
                        }, TimeSpan.FromSeconds(5))
                    },
                    {"booms",
                        (new()
                        {
                            {4,PinValue.High }
                        }, TimeSpan.FromSeconds(3))
                    }
                }
            });
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