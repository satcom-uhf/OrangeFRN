namespace OrangeFRN
{
    public class Config
    {
        public string AntennaPort { get; set; }
        public string MotorolaPort { get; set; }
        public string PathToFRN { get; set; }
        public string ChatCommandTemplate { get; set; }
        public byte DefaultLevel { get; set; }
        public int ClickTimeMs { get; set; } = 300;
        public int[] Pins { get; set; } = new int [] {}; 
        public Dictionary<string, int[]> Commands { get; set; } = new();
    }
}
