namespace Emulator6502
{
    public class StatusRegister
    {
        public bool N { get; set; }
        public bool V { get; set; }
        public bool B { get; set; }
        public bool D { get; set; }
        public bool I { get; set; }
        public bool Z { get; set; }
        public bool C { get; set; }

        public string ToString()
        {
            string n = N ? "1" : "0";
            string v = V ? "1" : "0";
            string d = D ? "1" : "0";
            string i = I ? "1" : "0";
            string z = Z ? "1" : "0";
            string c = C ? "1" : "0";

            return $"{n}{v}--{d}{i}{z}{c}";
        }
    }
}