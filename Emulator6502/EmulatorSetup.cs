using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

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
            var rgx = new Regex("[^A-Z0-9]");
            byteData = rgx.Replace(byteData.ToUpper(), "");
            if (byteData.Length % 2 != 0)
                throw new ArgumentException("Err. Program is not in correct format.");

            var programBytes = new List<byte>();

            for (int i = 0; i < byteData.Length; i += 2)
            {
                string byteString = byteData.Substring(i, 2);
                programBytes.Add(Byte.Parse(byteString, NumberStyles.HexNumber));
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



        public string RunDebugCycle(int cycle)
        {
            _cpu.PreRunCycle();

            if (_cpu.RW)
                _memory.Read();

            this.CpuDump.Add($"{cycle}\t${_addressBus.Address:X4}\t{_dataBus.Data:X2}\t{_cpu.Fetch?.Operation}\t{_cpu.PC:X4}\t{_cpu.IR:X2}\t{_cpu.Execute.Operation}\t{_cpu.TState}\t{_cpu.Accu:X2}\t{_cpu.X:X2}\t{_cpu.Y:X2}\t{_cpu.StatusReg.ToString()}");

            string debugInfo = _cpu.RunCycle();

            if (!_cpu.RW)
                _memory.Write();

            return debugInfo;

        }

    }
}