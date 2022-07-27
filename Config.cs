namespace OrangeFRN
{
    using PinsState = Dictionary<int, byte>;
    public record CommandDescription(PinsState pins, TimeSpan time) { }
    public class Config
    {
        public PinsState DefaultState { get; set; } = new PinsState();
        public Dictionary<string, CommandDescription> Commands { get; set; } = new ();    
    }
}
