using System;

namespace Emulator6502
{
    class Program
    {
        static void Main(string[] args)
        {
            var emulatorSetup = new EmulatorSetup();

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
                        if (!emulatorSetup.SetProgramStartAddress(cliArgument))
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
                        if (!emulatorSetup.SetProgramStartAddress(cliArgument))
                        {
                            Console.WriteLine("Error! Address in wrong format.");
                            break;
                        }

                        Console.Write("Program byte data: ");
                        cliArgument = Console.ReadLine();
                        if (!emulatorSetup.LoadProgramToMemory(cliArgument))
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

                    default:
                        if (debug)
                        {
                            Console.WriteLine("Press Enter for next cycle.");
                            emulatorSetup.RunDebugCycle(cycle);
                            cycle++;
                        }
                        break;
                }
            }

        }





    }
}