using System.Collections.Concurrent;
using System.Device.Gpio;

namespace Tests
{
    class FakeGpio : GpioDriver
    {
        private readonly ConcurrentDictionary<int, PinValue> _state = new ();
        private readonly ConcurrentDictionary<int, PinMode> _modes = new ();

        protected override int PinCount => int.MaxValue;

        protected override PinValue Read(int pinNumber) => _state[pinNumber];
        protected override void Write(int pinNumber, PinValue value) => _state[pinNumber] = value;

        protected override void AddCallbackForPinValueChangedEvent(int pinNumber, PinEventTypes eventTypes, PinChangeEventHandler callback)
        {

        }

        protected override void ClosePin(int pinNumber)
        {

        }

        protected override int ConvertPinNumberToLogicalNumberingScheme(int pinNumber)
        {
            return pinNumber;
        }

        protected override PinMode GetPinMode(int pinNumber) => _modes.TryGetValue(pinNumber, out var mode) ? mode : PinMode.Input;
        protected override bool IsPinModeSupported(int pinNumber, PinMode mode) => true;

        protected override void OpenPin(int pinNumber) { _state[pinNumber] = PinValue.Low; }
        protected override void RemoveCallbackForPinValueChangedEvent(int pinNumber, PinChangeEventHandler callback)
        {            
        }

        protected override void SetPinMode(int pinNumber, PinMode mode) => _modes[pinNumber] = mode;

        protected override WaitForEventResult WaitForEvent(int pinNumber, PinEventTypes eventTypes, CancellationToken cancellationToken)
            => new WaitForEventResult();
    }
}