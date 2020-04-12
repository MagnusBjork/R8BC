using System;

namespace Emulator6502
{
    class Program
    {
        static void Main(string[] args)
        {

            var addressBus = new AddressBus();
            var dataBus = new DataBus();

            var memory = new Memory(addressBus, dataBus);


            // Write a program to memory
            memory.SetByte(0x0500, 0x18);
            memory.SetByte(0x0501, 0xA9);
            memory.SetByte(0x0502, 0x23);
            memory.SetByte(0x0503, 0x6D);
            memory.SetByte(0x0504, 0x43);
            memory.SetByte(0x0505, 0x20);
            memory.SetByte(0x0506, 0x8D);
            memory.SetByte(0x0507, 0x43);
            memory.SetByte(0x0508, 0x20);

            memory.SetByte(0x2043, 0x72);

            // http://visual6502.org/JSSim/expert.html?graphics=f&steps=4&loglevel=4&r=500&a=500&d=18A9236D43208D4320


            var cpu = new Cpu6502(addressBus, dataBus);


            Console.WriteLine("Cycle\tAddress\tData\tPC\tIR\tTstate");
            Console.WriteLine("-------------------------------------------------");


            bool quit = false;
            int cycle = 0;
            while (!quit)
            {
                cpu.PreRunCycle();

                memory.Enable(cpu.RW);

                Console.WriteLine($"{cycle}\t${addressBus.Address:X4}\t{dataBus.Data:X2}\t{cpu.PC:X4}\t{cpu.IR:X2}\t{cpu.TState}");

                cpu.RunCycle();

                cycle++;

                //-------------- Clock cycle completed



                var key = Console.ReadLine();
                if (key == "q") quit = true;

            }

            Console.WriteLine("End");
            Console.ReadLine();
        }
    }
}