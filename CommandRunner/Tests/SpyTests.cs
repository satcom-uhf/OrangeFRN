using OrangeFRN;
using Serilog;
using System.Device.Gpio;
using System.Diagnostics;
using System.Text.Json;
using Xunit.Abstractions;

namespace Tests
{
    [Collection("Sequential")]
    public class SpyTests
    {
        private readonly GpioController target = new GpioController(PinNumberingScheme.Logical, new FakeGpio());
        public SpyTests(ITestOutputHelper output)
        {
            Log.Logger = new LoggerConfiguration()
            // add the xunit test output sink to the serilog logger
            // https://github.com/trbenning/serilog-sinks-xunit#serilog-sinks-xunit
            .WriteTo.TestOutput(output, outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level}] {Message}{NewLine}{Exception}")
            .CreateLogger();
        }

        [Fact]
        public async Task DefaultStateTest()
        {
            var cts = new CancellationTokenSource(1000);
            var spy = new LogSpy(target, new Config
            {
                DefaultLevel = 1,
                Pins = new[] { 3, 5, 7 }
            });
            await spy.Run(cts.Token);
            Assert.Equal(1, target.Read(3));
            Assert.Equal(1, target.Read(5));
            Assert.Equal(1, target.Read(7));
        }

        [Fact]
        public async Task ShouldExecuteCommandsIfLogFileModified()
        {
            var cts = new CancellationTokenSource(7000);

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
            var task = spy.Run(cts.Token);
            async Task CheckChatMessage()
            {
                File.AppendAllLines(LogSpy.LogFile, new[] { "New message: MOTO 7 3!" });

                await Task.Delay(800);
                Assert.Equal(PinValue.High, target.Read(3));
                Assert.Equal(PinValue.Low, target.Read(5));
                Assert.Equal(PinValue.High, target.Read(7));

                await Task.Delay(1000);
                Assert.Equal(PinValue.Low, target.Read(3));
                Assert.Equal(PinValue.Low, target.Read(5));
                Assert.Equal(PinValue.Low, target.Read(7));

                await Task.Delay(1000);
                Assert.Equal(PinValue.Low, target.Read(3));
                Assert.Equal(PinValue.High, target.Read(5));
                Assert.Equal(PinValue.Low, target.Read(7));

                await Task.Delay(1000);
                Assert.Equal(PinValue.Low, target.Read(3));
                Assert.Equal(PinValue.Low, target.Read(5));
                Assert.Equal(PinValue.Low, target.Read(7));

            }
            await CheckChatMessage();
            await CheckChatMessage();
            await task;
        }
    }
}