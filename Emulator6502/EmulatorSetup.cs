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

        public IList<string> CpuDump { get; private set; } = new List<string>();
        public IList<ushort> MemoryDump { get; private set; } = new List<ushort>();
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

            return true;
        }

        public void SetMemoryByte(ushort address, byte data, bool addToMemoryDump = true)
        {
            if (addToMemoryDump)
                this.MemoryDump.Add(address);

            _memory.SetByte(address, data);
        }

        public byte GetMemoryByte(ushort address) => _memory.GetByte(address);



        public void RunDebugCycle(int cycle)
        {
            _cpu.PreRunCycle();

            if (_cpu.RW)
                _memory.Read();

            this.CpuDump.Add($"{cycle}\t${_addressBus.Address:X4}\t{_dataBus.Data:X2}\t{_cpu.PC:X4}\t{_cpu.IR:X2}\t{_cpu.Instruction.Operation}\t{_cpu.TState}\t{_cpu.A}\t{_cpu.X}\t{_cpu.Y}\t{_cpu.P.ToString()}");

            _cpu.RunCycle();

            if (!_cpu.RW)
                _memory.Write();

        }

    }
}