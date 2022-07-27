using System.Device.Gpio;

namespace OrangeFRN
{
    using PinsState = Dictionary<int, PinValue>;
    public class Config
    {
        public PinsState DefaultState { get; set; } = new PinsState();
        public Dictionary<string, (PinsState pins, TimeSpan time)> Commands { get; set; } = new Dictionary<string, (PinsState pins, TimeSpan time)>();    
    }
}
