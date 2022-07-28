namespace OrangeFRN
{
    public class Config
    {
        public string CommandPrefix { get; set; } = "MOTO";
        public string CommandSuffix { get; set; } = "!";
        public byte DefaultLevel { get; set; }
        public int ClickTimeMs { get; set; } = 300;
        public int[] Pins { get; set; } = new int [] {}; 
        public Dictionary<string, byte[]> Commands { get; set; } = new();
    }
}
