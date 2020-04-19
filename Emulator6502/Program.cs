using System;

namespace Emulator6502
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("6502 Emulator");
            Console.WriteLine("-------------");

            var emulator = new EmulatorSetup();


            //http://visual6502.org/JSSim/expert.html?graphics=f&steps=40&loglevel=4&r=500&a=500&d=18A5106D43208D432018A924

            // ---- Preset my test program
            emulator.SetProgramStartAddress("0x500");
            string testPrg = "18,A5,10,6D,43,20,8D,43,20,18,A9,24";
            emulator.LoadProgramToMemory(testPrg);
            emulator.SetMemoryByte(0x2043, 0x04);
            emulator.SetMemoryByte(0x0010, 0xe8);


            bool quit = false;
            int cycle = 0;
            bool debug = false;
            while (!quit)
            {
                string cliInput = Console.ReadLine();
                cliInput = cliInput.ToLower().Replace(" ", string.Empty);
                string cliArgument = string.Empty;

                switch (cliInput)
                {
                    case "q":
                    case "quit":
                        if (debug)
                            debug = false;
                        else
                            quit = true;
                        break;

                    case "d":
                    case "debug":
                        Console.Write("Start address (hex): $");
                        cliArgument = Console.ReadLine();
                        if (!emulator.SetProgramStartAddress(cliArgument))
                        {
                            Console.WriteLine("Error! Address in wrong format.");
                            break;
                        }

                        debug = true;
                        cycle = 0;
                        break;

                    case "l":
                    case "load":
                        Console.Write("Start address (hex): $");
                        cliArgument = Console.ReadLine();
                        if (!emulator.SetProgramStartAddress(cliArgument))
                        {
                            Console.WriteLine("Error! Address in wrong format.");
                            break;
                        }

                        Console.Write("Program byte data: ");
                        cliArgument = Console.ReadLine();
                        if (!emulator.LoadProgramToMemory(cliArgument))
                        {
                            Console.WriteLine("Error! Program data could not be loaded.");
                            break;
                        }

                        break;

                    case "v":
                    case "view":
                        Console.Write("Start address (hex): $");
                        string viewStartAdr = Console.ReadLine();
                        Console.Write("End address (hex): $");
                        string viewEndAdr = Console.ReadLine();
                        break;

                    case "j":
                    case "help":
                        Console.Clear();
                        Console.Write("TODO: HELP");
                        break;

                    default:
                        if (debug)
                        {
                            string debugInfo = emulator.RunDebugCycle(cycle);
                            Console.Clear();
                            UpdateDebugView(emulator);
                            Console.WriteLine(debugInfo);
                            cycle++;
                        }
                        else
                        {
                            Console.Clear();
                            Console.Write("Enter a command or enter 'help'.");
                        }
                        break;
                }
            }
        }

        private static void UpdateDebugView(EmulatorSetup emulator)
        {
            Console.WriteLine("Cycle\tAddress\tData\tPC\tIR\tInstr\tTState\tA\tX\tY\tP(SR)");
            Console.WriteLine("----------------------------------------------------------------------------------------");
            foreach (var cpuCycleDump in emulator.CpuDump)
            {
                Console.WriteLine(cpuCycleDump);
            }

            Console.WriteLine("");
            Console.WriteLine("Memory");
            Console.WriteLine("------");
            foreach (var address in emulator.MemoryDump)
            {
                byte data = emulator.GetMemoryByte(address);
                Console.WriteLine($"${address:X4}: {data:X2}");
            }

        }
    }
}