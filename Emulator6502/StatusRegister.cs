namespace Emulator6502
{
    public class StatusRegister
    {
        public bool N { get; private set; }
        public bool V { get; private set; }
        public bool B { get; private set; }
        public bool D { get; private set; }
        public bool I { get; private set; }
        public bool Z { get; private set; }
        public bool C { get; private set; }

        public string ToString()
        {
            string status = "nv-BdIZc";
            if (N)
                status.Replace("n", "N");
            if (V)
                status.Replace("v", "V");
            if (!B)
                status.Replace("B", "b");
            if (D)
                status.Replace("d", "D");
            if (!I)
                status.Replace("I", "i");
            if (!Z)
                status.Replace("Z", "z");
            if (C)
                status.Replace("c", "C");

            return status;
        }
    }
}