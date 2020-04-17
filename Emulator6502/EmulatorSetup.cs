using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Emulator6502
{
    public class EmulatorSetup
    {
        private AddressBus _addressBus;
        private DataBus _dataBus;
        private Memory _memory;
        private Cpu6502 _cpu;

        public ushort ProgramStartAddress { get; private set; }

        public EmulatorSetup()
        {
            _addressBus = new AddressBus();
            _dataBus = new DataBus();
            _memory = new Memory(_addressBus, _dataBus);
            _cpu = new Cpu6502(_addressBus, _dataBus);
        }


        public bool SetProgramStartAddress(string address)
        {
            var startAddress = address.ToMemoryAddress();
            if (!startAddress.HasValue)
                return false;

            this.ProgramStartAddress = (ushort)startAddress;
            return true;
        }

        public bool LoadProgramToMemory(string byteData)
        {
            var programBytes = new List<byte>();

            try
            {
                programBytes = byteData.Split(',').Select(s => Byte.Parse(s, NumberStyles.HexNumber)).ToList();
            }
            catch
            {
                return false;
            }

            ushort pointer = this.ProgramStartAddress;
            foreach (var prgByte in programBytes)
            {
                _memory.SetByte(pointer, prgByte);
                pointer++;
            }
            //18,A9,23,6D,43,20,8D,43,20,00

            // Write a program to memory
            // _memory.SetByte(0x0500, 0x18);   // CLC
            // _memory.SetByte(0x0501, 0xA9);   // LDA #
            // _memory.SetByte(0x0502, 0x23);
            // _memory.SetByte(0x0503, 0x6D);   // ADC Abs
            // _memory.SetByte(0x0504, 0x43);
            // _memory.SetByte(0x0505, 0x20);
            // _memory.SetByte(0x0506, 0x8D);   // STA Abs
            // _memory.SetByte(0x0507, 0x43);
            // _memory.SetByte(0x0508, 0x20);
            // _memory.SetByte(0x0509, 0x00);   // BRK

            _memory.SetByte(0x2043, 0x04);

            return true;
        }


        public void RunDebugCycle(int cycle)
        {

            _cpu.PreRunCycle();

            if (_cpu.RW) _memory.Read();

            Console.WriteLine("Cycle\tAddress\tData\tPC\tIR\tTstate");
            Console.WriteLine("-------------------------------------------------");
            Console.WriteLine($"{cycle}\t${_addressBus.Address:X4}\t{_dataBus.Data:X2}\t{_cpu.PC:X4}\t{_cpu.IR:X2}\t{_cpu.TState}");

            _cpu.RunCycle();

            if (!_cpu.RW) _memory.Write();



            //-------------- Clock cycle completed

            Console.WriteLine("Data in $2043 " + _memory.GetByte(0x2043));

        }

    }
}