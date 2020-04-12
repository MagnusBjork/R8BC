
using System;
using System.Collections.Generic;
using System.Linq;

namespace Emulator6502
{

    public enum EnumTstate
    {
        T0, T1, T2, T0T2, T3, T4, T5, T6
    }

    public enum EnumAddressingMode
    {
        Implied, Immediate, Absolute, Zero_page, Zero_page_indexing, Relative, Indexed_indirect, Index
    }


    public class Instruction
    {
        public byte Opcode;
        public string Name { get; set; }
        public EnumAddressingMode AddressingMode { get; set; }
        public int Bytes { get; set; }
        public int Cycles { get; set; }
        public bool NextFetchOnLastExecuteCycle { get; set; }
    }




    public class Cpu6502
    {
        public ushort PC { get; private set; }
        public byte IR { get; private set; }
        public bool RW { get; private set; }

        public EnumTstate TState => _tState;

        public byte X { get; private set; }
        public byte Y { get; private set; }
        public byte A { get; private set; }


        private readonly IAddressBus _addressBus;
        private readonly IDataBus _dataBus;

        private EnumTstate _tState;
        private Instruction _instruction;

        private byte _adl;
        private byte _adh;

        private byte _pd;
        private byte _alu;



        public Cpu6502(IAddressBus addressBus, IDataBus dataBus)
        {
            _addressBus = addressBus;
            _dataBus = dataBus;

            // Set a temp init state for CPU to start executing.
            PC = 0x0500;
            LoadAddress(PC);
            RW = true;  // Read
            _tState = EnumTstate.T1;
        }

        public void PreRunCycle()
        {
            _addressBus.Address = (ushort)((_adh << 8) + _adl);

        }


        public void RunCycle()
        {

            if (RW)
            {
                //-------- Read

                if (_tState.Equals(EnumTstate.T1))
                {
                    // T1 Fetch

                    _instruction = LoadToDecoder(_dataBus.Data);
                    IR = _instruction.Opcode;
                    PC++;
                    LoadAddress(PC);
                };

                if (_tState.Equals(EnumTstate.T2) || _tState.Equals(EnumTstate.T0T2))
                {
                    // T2 Execute

                    if (IR.Equals(0x18))
                    {
                        Console.WriteLine($"IR: {IR:X2} - T2 Execute - Flags: C");
                    }

                    if (IR.Equals(0xA9))
                    {
                        A = _dataBus.Data;
                        Console.WriteLine($"IR: {IR:X2} - T2 Execute - Accu: {A:X2} - Flags: N, Z");
                        PC++;
                        LoadAddress(PC);
                    }

                    if (IR.Equals(0x6D))
                    {
                        _pd = _dataBus.Data;
                        Console.WriteLine($"IR: {IR:X2} - T2 Execute - Low byte: {_pd:X2}");
                        PC++;
                        LoadAddress(PC);
                    }
                }

                if (_tState.Equals(EnumTstate.T3))
                {
                    // T3 Execute

                    if (IR.Equals(0x6D))
                    {
                        _alu = _pd;
                        _pd = _dataBus.Data;
                        Console.WriteLine($"IR: {IR:X2} - T3 Execute - High byte: {_pd:X2}");
                        PC++;

                        _adh = _pd;
                        _adl = _alu;
                    }
                }


                if (_tState.Equals(EnumTstate.T0))
                {
                    // Execute T0 

                    if (IR.Equals(0x6D))
                    {
                        //_addressBus.Address = (ushort)((_adh << 8) + _adl);
                        A = _dataBus.Data;
                        Console.WriteLine($"IR: {IR:X2} - T0 Execute - ADH: {_adh:X2} ADL: {_adl:X2} - Accu: {A:X2}");
                    }
                }

                else
                {
                    //-------- Write
                }
            }


            _tState = GetNextTstate(_tState, _instruction);
        }

        private void LoadAddress(ushort address)
        {
            _adl = (byte)(address);
            _adh = (byte)(address >> 8);
        }




        private Instruction LoadToDecoder(byte opcode)
        {
            var instructions = new List<Instruction>() {
                new Instruction { Opcode = 0x18, Name = "CLC", AddressingMode = EnumAddressingMode.Immediate, Bytes = 1, Cycles = 2, NextFetchOnLastExecuteCycle = true },
                new Instruction { Opcode = 0xA9, Name = "LDA", AddressingMode = EnumAddressingMode.Immediate, Bytes = 2, Cycles = 2, NextFetchOnLastExecuteCycle = true },
                new Instruction { Opcode = 0x6D, Name = "ADC", AddressingMode = EnumAddressingMode.Absolute , Bytes = 3, Cycles = 4, NextFetchOnLastExecuteCycle = false},
                new Instruction { Opcode = 0x8D, Name = "STA", AddressingMode = EnumAddressingMode.Absolute , Bytes = 3, Cycles = 4, NextFetchOnLastExecuteCycle = false}
             };

            return instructions.Where(i => i.Opcode.Equals(opcode)).SingleOrDefault() ?? new Instruction();
        }

        public static EnumTstate GetNextTstate(EnumTstate currentTstate, Instruction instruction)
        {
            if (instruction is null)
                return EnumTstate.T1;


            switch (currentTstate)
            {
                case EnumTstate.T1:
                    return instruction.NextFetchOnLastExecuteCycle == true ? EnumTstate.T0T2 : EnumTstate.T2;

                case EnumTstate.T0:
                case EnumTstate.T0T2:
                    return EnumTstate.T1;

                case EnumTstate.T2:
                    if (instruction.Cycles == 2)
                        return EnumTstate.T1;
                    else if (instruction.Cycles == 3)
                        return EnumTstate.T0;
                    else
                        return EnumTstate.T3;

                case EnumTstate.T3:
                    if (instruction.Cycles == 3)
                        return EnumTstate.T1;
                    else if (instruction.Cycles == 4)
                        return EnumTstate.T0;
                    else
                        return EnumTstate.T4;


                default:
                    throw new NotImplementedException();
            }

        }

    }

}