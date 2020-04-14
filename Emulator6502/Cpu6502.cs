
using System;
using System.Collections.Generic;
using System.Linq;

namespace Emulator6502
{

    public enum EnumTstate
    {
        T0, T1, T2, T0T2, T3, T4, T5, T6
    }

    public enum EnumOperation
    {
        ADC, AND, ASL, BCC, BCS, BEQ, BIT, BMI, BNE, BPL, BRK, BVC, BVS, CLC, CLD, CLI, CLV, CMP, CPX,
        CPY, DEC, DEX, DEY, EOR, INC, INX, INY, JMP, JSR, LDA, LDX, LDY, LSR, NOP, ORA, PHA, PHP, PLA,
        PLP, ROL, ROR, RTI, RTS, SBC, SEC, SED, SEI, STA, STX, STY, TAX, TAY, TYA, TSX, TXA, TXS
    }



    public enum EnumAddressingMode
    {
        Immediate, ZeroPage, ZeroPage_X, Absolute, Absolute_X, Absolute_Y, IndexedIndirect_X,
        IndirectIndexed_Y, Accumulator, Relative, Implied, AbsoluteIndirect
    }


    public class Instruction
    {
        public byte Opcode;
        public EnumOperation Operation { get; set; }
        public EnumAddressingMode AddressingMode { get; set; }
        public int Bytes { get; set; }
        public int Cycles { get; set; }
        public bool ExtraCycle { get; set; }
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
                // Instruktioner med T0+T2 verkar alla vara de som är 'Implied' eller 'Immediate'.



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

                if (IR.Equals(0x6D) || IR.Equals(0x8D))
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

                if (IR.Equals(0x6D) || IR.Equals(0x8D))
                {
                    _alu = _pd;
                    _pd = _dataBus.Data;
                    Console.WriteLine($"IR: {IR:X2} - T3 Execute - High byte: {_pd:X2}");
                    PC++;

                    _adh = _pd;
                    _adl = _alu;
                }
            }

            // TODO: Borde man flytta _pd = _dataBus.Data, bort till preRunCycle istället? OCh hur gör man i så fall åt andra hållet?

            if (_tState.Equals(EnumTstate.T0))
            {
                // Execute T0 

                if (IR.Equals(0x6D))
                {
                    A += _dataBus.Data;     //  Direkt från minnet till accumulatorn eller via PD? TODO: Gör en bättre add som påverkar flaggorna.
                    Console.WriteLine($"IR: {IR:X2} - T0 Execute - ADH: {_adh:X2} ADL: {_adl:X2} - Accu: {A:X2}");
                    LoadAddress(PC);
                }

                if (IR.Equals(0x8D))
                {
                    //TODO: Måste skriva till minnet i cycle 11.                   

                    RW = false;
                    _dataBus.Data = A;
                    Console.WriteLine($"Write to memory - IR: {IR:X2} - T0 Execute - ADH: {_adh:X2} ADL: {_adl:X2} - Accu: {A:X2}");
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
            var instructions = new List<Instruction>();
            {
                new Instruction { Operation = EnumOperation.ADC, AddressingMode = EnumAddressingMode.Immediate, Opcode = 0x69, Bytes = 2, Cycles = 2, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.ADC, AddressingMode = EnumAddressingMode.ZeroPage, Opcode = 0x65, Bytes = 2, Cycles = 3, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.ADC, AddressingMode = EnumAddressingMode.ZeroPage_X, Opcode = 0x75, Bytes = 2, Cycles = 4, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.ADC, AddressingMode = EnumAddressingMode.Absolute, Opcode = 0x6D, Bytes = 3, Cycles = 4, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.ADC, AddressingMode = EnumAddressingMode.Absolute_X, Opcode = 0x7D, Bytes = 3, Cycles = 4, ExtraCycle = true };
                new Instruction { Operation = EnumOperation.ADC, AddressingMode = EnumAddressingMode.Absolute_Y, Opcode = 0x79, Bytes = 3, Cycles = 4, ExtraCycle = true };
                new Instruction { Operation = EnumOperation.ADC, AddressingMode = EnumAddressingMode.IndexedIndirect_X, Opcode = 0x61, Bytes = 2, Cycles = 6, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.ADC, AddressingMode = EnumAddressingMode.IndirectIndexed_Y, Opcode = 0x71, Bytes = 2, Cycles = 5, ExtraCycle = true };
                new Instruction { Operation = EnumOperation.AND, AddressingMode = EnumAddressingMode.Immediate, Opcode = 0x29, Bytes = 2, Cycles = 2, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.AND, AddressingMode = EnumAddressingMode.ZeroPage, Opcode = 0x25, Bytes = 2, Cycles = 3, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.AND, AddressingMode = EnumAddressingMode.ZeroPage_X, Opcode = 0x35, Bytes = 2, Cycles = 4, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.AND, AddressingMode = EnumAddressingMode.Absolute, Opcode = 0x2D, Bytes = 3, Cycles = 4, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.AND, AddressingMode = EnumAddressingMode.Absolute_X, Opcode = 0x3D, Bytes = 3, Cycles = 4, ExtraCycle = true };
                new Instruction { Operation = EnumOperation.AND, AddressingMode = EnumAddressingMode.Absolute_Y, Opcode = 0x39, Bytes = 3, Cycles = 4, ExtraCycle = true };
                new Instruction { Operation = EnumOperation.AND, AddressingMode = EnumAddressingMode.IndexedIndirect_X, Opcode = 0x21, Bytes = 2, Cycles = 6, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.AND, AddressingMode = EnumAddressingMode.IndirectIndexed_Y, Opcode = 0x31, Bytes = 2, Cycles = 5, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.ASL, AddressingMode = EnumAddressingMode.Accumulator, Opcode = 0x0A, Bytes = 1, Cycles = 2, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.ASL, AddressingMode = EnumAddressingMode.ZeroPage, Opcode = 0x06, Bytes = 2, Cycles = 5, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.ASL, AddressingMode = EnumAddressingMode.ZeroPage_X, Opcode = 0x16, Bytes = 2, Cycles = 6, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.ASL, AddressingMode = EnumAddressingMode.Absolute, Opcode = 0x0E, Bytes = 3, Cycles = 6, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.ASL, AddressingMode = EnumAddressingMode.Absolute_X, Opcode = 0x1E, Bytes = 3, Cycles = 7, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.BCC, AddressingMode = EnumAddressingMode.Relative, Opcode = 0x90, Bytes = 2, Cycles = 2, ExtraCycle = true };
                new Instruction { Operation = EnumOperation.BCS, AddressingMode = EnumAddressingMode.Relative, Opcode = 0xB0, Bytes = 2, Cycles = 2, ExtraCycle = true };
                new Instruction { Operation = EnumOperation.BEQ, AddressingMode = EnumAddressingMode.Relative, Opcode = 0xF0, Bytes = 2, Cycles = 2, ExtraCycle = true };
                new Instruction { Operation = EnumOperation.BIT, AddressingMode = EnumAddressingMode.ZeroPage, Opcode = 0x24, Bytes = 2, Cycles = 3, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.BIT, AddressingMode = EnumAddressingMode.Absolute, Opcode = 0x2C, Bytes = 3, Cycles = 4, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.BMI, AddressingMode = EnumAddressingMode.Relative, Opcode = 0x30, Bytes = 2, Cycles = 2, ExtraCycle = true };
                new Instruction { Operation = EnumOperation.BNE, AddressingMode = EnumAddressingMode.Relative, Opcode = 0xD0, Bytes = 2, Cycles = 4, ExtraCycle = true };
                new Instruction { Operation = EnumOperation.BPL, AddressingMode = EnumAddressingMode.Relative, Opcode = 0x10, Bytes = 2, Cycles = 2, ExtraCycle = true };
                new Instruction { Operation = EnumOperation.BRK, AddressingMode = EnumAddressingMode.Implied, Opcode = 0x00, Bytes = 1, Cycles = 7, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.BVC, AddressingMode = EnumAddressingMode.Relative, Opcode = 0x40, Bytes = 2, Cycles = 2, ExtraCycle = true };
                new Instruction { Operation = EnumOperation.BVS, AddressingMode = EnumAddressingMode.Relative, Opcode = 0x70, Bytes = 2, Cycles = 2, ExtraCycle = true };
                new Instruction { Operation = EnumOperation.CLC, AddressingMode = EnumAddressingMode.Implied, Opcode = 0x18, Bytes = 1, Cycles = 2, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.CLD, AddressingMode = EnumAddressingMode.Implied, Opcode = 0xD8, Bytes = 1, Cycles = 2, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.CLI, AddressingMode = EnumAddressingMode.Implied, Opcode = 0x58, Bytes = 1, Cycles = 2, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.CLV, AddressingMode = EnumAddressingMode.Implied, Opcode = 0xB8, Bytes = 1, Cycles = 2, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.CMP, AddressingMode = EnumAddressingMode.Immediate, Opcode = 0xC9, Bytes = 2, Cycles = 2, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.CMP, AddressingMode = EnumAddressingMode.ZeroPage, Opcode = 0xC5, Bytes = 2, Cycles = 3, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.CMP, AddressingMode = EnumAddressingMode.ZeroPage_X, Opcode = 0xD5, Bytes = 2, Cycles = 4, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.CMP, AddressingMode = EnumAddressingMode.Absolute, Opcode = 0xCD, Bytes = 3, Cycles = 4, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.CMP, AddressingMode = EnumAddressingMode.Absolute_X, Opcode = 0xDD, Bytes = 3, Cycles = 4, ExtraCycle = true };
                new Instruction { Operation = EnumOperation.CMP, AddressingMode = EnumAddressingMode.Absolute_Y, Opcode = 0xD9, Bytes = 3, Cycles = 4, ExtraCycle = true };
                new Instruction { Operation = EnumOperation.CMP, AddressingMode = EnumAddressingMode.IndexedIndirect_X, Opcode = 0xC1, Bytes = 2, Cycles = 6, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.CMP, AddressingMode = EnumAddressingMode.IndirectIndexed_Y, Opcode = 0xD1, Bytes = 2, Cycles = 5, ExtraCycle = true };
                new Instruction { Operation = EnumOperation.CPX, AddressingMode = EnumAddressingMode.Immediate, Opcode = 0xE0, Bytes = 2, Cycles = 2, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.CPX, AddressingMode = EnumAddressingMode.ZeroPage, Opcode = 0xE4, Bytes = 2, Cycles = 3, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.CPX, AddressingMode = EnumAddressingMode.Absolute, Opcode = 0xEC, Bytes = 3, Cycles = 4, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.CPY, AddressingMode = EnumAddressingMode.Immediate, Opcode = 0xE0, Bytes = 2, Cycles = 2, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.CPY, AddressingMode = EnumAddressingMode.ZeroPage, Opcode = 0xE4, Bytes = 2, Cycles = 3, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.CPY, AddressingMode = EnumAddressingMode.Absolute, Opcode = 0xEC, Bytes = 3, Cycles = 4, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.DEC, AddressingMode = EnumAddressingMode.ZeroPage, Opcode = 0xC6, Bytes = 2, Cycles = 5, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.DEC, AddressingMode = EnumAddressingMode.ZeroPage_X, Opcode = 0xD6, Bytes = 2, Cycles = 6, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.DEC, AddressingMode = EnumAddressingMode.Absolute, Opcode = 0xCE, Bytes = 3, Cycles = 6, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.DEC, AddressingMode = EnumAddressingMode.Absolute_X, Opcode = 0xDE, Bytes = 3, Cycles = 7, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.DEX, AddressingMode = EnumAddressingMode.Implied, Opcode = 0xCA, Bytes = 1, Cycles = 2, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.DEY, AddressingMode = EnumAddressingMode.Implied, Opcode = 0x88, Bytes = 1, Cycles = 2, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.EOR, AddressingMode = EnumAddressingMode.Immediate, Opcode = 0x49, Bytes = 2, Cycles = 2, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.EOR, AddressingMode = EnumAddressingMode.ZeroPage, Opcode = 0x45, Bytes = 2, Cycles = 3, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.EOR, AddressingMode = EnumAddressingMode.ZeroPage_X, Opcode = 0x55, Bytes = 2, Cycles = 4, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.EOR, AddressingMode = EnumAddressingMode.Absolute, Opcode = 0x4D, Bytes = 3, Cycles = 4, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.EOR, AddressingMode = EnumAddressingMode.Absolute_X, Opcode = 0x5D, Bytes = 3, Cycles = 4, ExtraCycle = true };
                new Instruction { Operation = EnumOperation.EOR, AddressingMode = EnumAddressingMode.Absolute_Y, Opcode = 0x59, Bytes = 3, Cycles = 4, ExtraCycle = true };
                new Instruction { Operation = EnumOperation.EOR, AddressingMode = EnumAddressingMode.IndexedIndirect_X, Opcode = 0x41, Bytes = 2, Cycles = 6, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.EOR, AddressingMode = EnumAddressingMode.IndirectIndexed_Y, Opcode = 0x51, Bytes = 2, Cycles = 5, ExtraCycle = true };
                new Instruction { Operation = EnumOperation.INC, AddressingMode = EnumAddressingMode.ZeroPage, Opcode = 0xE6, Bytes = 2, Cycles = 5, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.INC, AddressingMode = EnumAddressingMode.ZeroPage_X, Opcode = 0xF6, Bytes = 2, Cycles = 6, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.INC, AddressingMode = EnumAddressingMode.Absolute, Opcode = 0xEE, Bytes = 3, Cycles = 6, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.INC, AddressingMode = EnumAddressingMode.Absolute_X, Opcode = 0xFE, Bytes = 3, Cycles = 7, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.INX, AddressingMode = EnumAddressingMode.Implied, Opcode = 0xE8, Bytes = 1, Cycles = 2, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.INY, AddressingMode = EnumAddressingMode.Implied, Opcode = 0xC8, Bytes = 1, Cycles = 2, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.JMP, AddressingMode = EnumAddressingMode.Absolute, Opcode = 0x4C, Bytes = 3, Cycles = 3, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.JMP, AddressingMode = EnumAddressingMode.AbsoluteIndirect, Opcode = 0x6C, Bytes = 3, Cycles = 5, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.JSR, AddressingMode = EnumAddressingMode.Absolute, Opcode = 0x20, Bytes = 3, Cycles = 6, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.LDA, AddressingMode = EnumAddressingMode.Immediate, Opcode = 0xA9, Bytes = 2, Cycles = 2, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.LDA, AddressingMode = EnumAddressingMode.ZeroPage, Opcode = 0xA5, Bytes = 2, Cycles = 3, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.LDA, AddressingMode = EnumAddressingMode.ZeroPage_X, Opcode = 0xB5, Bytes = 2, Cycles = 4, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.LDA, AddressingMode = EnumAddressingMode.Absolute, Opcode = 0xAD, Bytes = 3, Cycles = 4, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.LDA, AddressingMode = EnumAddressingMode.Absolute_X, Opcode = 0xBD, Bytes = 3, Cycles = 4, ExtraCycle = true };
                new Instruction { Operation = EnumOperation.LDA, AddressingMode = EnumAddressingMode.Absolute_Y, Opcode = 0xB9, Bytes = 3, Cycles = 4, ExtraCycle = true };
                new Instruction { Operation = EnumOperation.LDA, AddressingMode = EnumAddressingMode.IndexedIndirect_X, Opcode = 0xA1, Bytes = 2, Cycles = 6, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.LDA, AddressingMode = EnumAddressingMode.IndirectIndexed_Y, Opcode = 0xB1, Bytes = 2, Cycles = 5, ExtraCycle = true };
                new Instruction { Operation = EnumOperation.LDX, AddressingMode = EnumAddressingMode.Immediate, Opcode = 0xA2, Bytes = 2, Cycles = 2, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.LDX, AddressingMode = EnumAddressingMode.ZeroPage, Opcode = 0xA6, Bytes = 2, Cycles = 3, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.LDX, AddressingMode = EnumAddressingMode.ZeroPage_X, Opcode = 0xB6, Bytes = 2, Cycles = 4, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.LDX, AddressingMode = EnumAddressingMode.Absolute, Opcode = 0xAE, Bytes = 3, Cycles = 4, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.LDX, AddressingMode = EnumAddressingMode.Absolute_X, Opcode = 0xBE, Bytes = 3, Cycles = 4, ExtraCycle = true };
                new Instruction { Operation = EnumOperation.LDY, AddressingMode = EnumAddressingMode.Immediate, Opcode = 0xA0, Bytes = 2, Cycles = 2, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.LDY, AddressingMode = EnumAddressingMode.ZeroPage, Opcode = 0xA4, Bytes = 2, Cycles = 3, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.LDY, AddressingMode = EnumAddressingMode.ZeroPage_X, Opcode = 0xB4, Bytes = 2, Cycles = 4, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.LDY, AddressingMode = EnumAddressingMode.Absolute, Opcode = 0xAC, Bytes = 3, Cycles = 4, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.LDY, AddressingMode = EnumAddressingMode.Absolute_X, Opcode = 0xBC, Bytes = 3, Cycles = 4, ExtraCycle = true };
                new Instruction { Operation = EnumOperation.LSR, AddressingMode = EnumAddressingMode.Accumulator, Opcode = 0x4A, Bytes = 1, Cycles = 2, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.LSR, AddressingMode = EnumAddressingMode.ZeroPage, Opcode = 0x46, Bytes = 2, Cycles = 5, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.LSR, AddressingMode = EnumAddressingMode.ZeroPage_X, Opcode = 0x56, Bytes = 2, Cycles = 6, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.LSR, AddressingMode = EnumAddressingMode.Absolute, Opcode = 0x4E, Bytes = 3, Cycles = 6, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.LSR, AddressingMode = EnumAddressingMode.Absolute_X, Opcode = 0x5E, Bytes = 3, Cycles = 7, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.NOP, AddressingMode = EnumAddressingMode.Implied, Opcode = 0xEA, Bytes = 1, Cycles = 2, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.ORA, AddressingMode = EnumAddressingMode.Immediate, Opcode = 0x09, Bytes = 2, Cycles = 2, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.ORA, AddressingMode = EnumAddressingMode.ZeroPage, Opcode = 0x05, Bytes = 2, Cycles = 3, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.ORA, AddressingMode = EnumAddressingMode.ZeroPage_X, Opcode = 0x15, Bytes = 2, Cycles = 4, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.ORA, AddressingMode = EnumAddressingMode.Absolute, Opcode = 0x0D, Bytes = 3, Cycles = 4, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.ORA, AddressingMode = EnumAddressingMode.Absolute_X, Opcode = 0x1D, Bytes = 3, Cycles = 4, ExtraCycle = true };
                new Instruction { Operation = EnumOperation.ORA, AddressingMode = EnumAddressingMode.Absolute_Y, Opcode = 0x19, Bytes = 3, Cycles = 4, ExtraCycle = true };
                new Instruction { Operation = EnumOperation.ORA, AddressingMode = EnumAddressingMode.IndexedIndirect_X, Opcode = 0x01, Bytes = 2, Cycles = 6, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.ORA, AddressingMode = EnumAddressingMode.IndirectIndexed_Y, Opcode = 0x11, Bytes = 2, Cycles = 5, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.PHA, AddressingMode = EnumAddressingMode.Implied, Opcode = 0x48, Bytes = 1, Cycles = 3, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.PHP, AddressingMode = EnumAddressingMode.Implied, Opcode = 0x08, Bytes = 1, Cycles = 3, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.PLA, AddressingMode = EnumAddressingMode.Implied, Opcode = 0x68, Bytes = 1, Cycles = 4, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.PLP, AddressingMode = EnumAddressingMode.Implied, Opcode = 0x28, Bytes = 1, Cycles = 4, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.ROL, AddressingMode = EnumAddressingMode.Accumulator, Opcode = 0x2A, Bytes = 1, Cycles = 2, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.ROL, AddressingMode = EnumAddressingMode.ZeroPage, Opcode = 0x26, Bytes = 2, Cycles = 5, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.ROL, AddressingMode = EnumAddressingMode.ZeroPage_X, Opcode = 0x36, Bytes = 2, Cycles = 6, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.ROL, AddressingMode = EnumAddressingMode.Absolute, Opcode = 0x2E, Bytes = 3, Cycles = 6, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.ROL, AddressingMode = EnumAddressingMode.Absolute_X, Opcode = 0x3E, Bytes = 3, Cycles = 7, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.ROR, AddressingMode = EnumAddressingMode.Accumulator, Opcode = 0x6A, Bytes = 1, Cycles = 2, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.ROR, AddressingMode = EnumAddressingMode.ZeroPage, Opcode = 0x66, Bytes = 2, Cycles = 5, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.ROR, AddressingMode = EnumAddressingMode.ZeroPage_X, Opcode = 0x76, Bytes = 2, Cycles = 6, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.ROR, AddressingMode = EnumAddressingMode.Absolute, Opcode = 0x6E, Bytes = 3, Cycles = 6, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.ROR, AddressingMode = EnumAddressingMode.Absolute_X, Opcode = 0x7E, Bytes = 3, Cycles = 7, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.RTI, AddressingMode = EnumAddressingMode.Implied, Opcode = 0x40, Bytes = 1, Cycles = 6, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.RTS, AddressingMode = EnumAddressingMode.Implied, Opcode = 0x60, Bytes = 1, Cycles = 6, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.SBC, AddressingMode = EnumAddressingMode.Immediate, Opcode = 0xE9, Bytes = 2, Cycles = 2, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.SBC, AddressingMode = EnumAddressingMode.ZeroPage, Opcode = 0xE5, Bytes = 2, Cycles = 3, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.SBC, AddressingMode = EnumAddressingMode.ZeroPage_X, Opcode = 0xF5, Bytes = 2, Cycles = 4, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.SBC, AddressingMode = EnumAddressingMode.Absolute, Opcode = 0xED, Bytes = 3, Cycles = 4, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.SBC, AddressingMode = EnumAddressingMode.Absolute_X, Opcode = 0xFD, Bytes = 3, Cycles = 4, ExtraCycle = true };
                new Instruction { Operation = EnumOperation.SBC, AddressingMode = EnumAddressingMode.Absolute_Y, Opcode = 0xF9, Bytes = 3, Cycles = 4, ExtraCycle = true };
                new Instruction { Operation = EnumOperation.SBC, AddressingMode = EnumAddressingMode.IndexedIndirect_X, Opcode = 0xE1, Bytes = 2, Cycles = 6, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.SBC, AddressingMode = EnumAddressingMode.IndirectIndexed_Y, Opcode = 0xF1, Bytes = 2, Cycles = 5, ExtraCycle = true };
                new Instruction { Operation = EnumOperation.SEC, AddressingMode = EnumAddressingMode.Implied, Opcode = 0x38, Bytes = 1, Cycles = 2, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.SED, AddressingMode = EnumAddressingMode.Implied, Opcode = 0xF8, Bytes = 1, Cycles = 2, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.SEI, AddressingMode = EnumAddressingMode.Implied, Opcode = 0x78, Bytes = 1, Cycles = 2, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.STA, AddressingMode = EnumAddressingMode.ZeroPage, Opcode = 0x85, Bytes = 2, Cycles = 3, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.STA, AddressingMode = EnumAddressingMode.ZeroPage_X, Opcode = 0x95, Bytes = 2, Cycles = 4, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.STA, AddressingMode = EnumAddressingMode.Absolute, Opcode = 0x8D, Bytes = 3, Cycles = 4, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.STA, AddressingMode = EnumAddressingMode.Absolute_X, Opcode = 0x9D, Bytes = 3, Cycles = 5, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.STA, AddressingMode = EnumAddressingMode.Absolute_Y, Opcode = 0x99, Bytes = 3, Cycles = 5, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.STA, AddressingMode = EnumAddressingMode.IndexedIndirect_X, Opcode = 0x81, Bytes = 2, Cycles = 6, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.STA, AddressingMode = EnumAddressingMode.IndirectIndexed_Y, Opcode = 0x91, Bytes = 2, Cycles = 6, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.STX, AddressingMode = EnumAddressingMode.ZeroPage, Opcode = 0x86, Bytes = 2, Cycles = 3, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.STX, AddressingMode = EnumAddressingMode.ZeroPage_X, Opcode = 0x96, Bytes = 2, Cycles = 4, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.STX, AddressingMode = EnumAddressingMode.Absolute, Opcode = 0x8E, Bytes = 3, Cycles = 4, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.STY, AddressingMode = EnumAddressingMode.ZeroPage, Opcode = 0x84, Bytes = 2, Cycles = 3, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.STY, AddressingMode = EnumAddressingMode.ZeroPage_X, Opcode = 0x94, Bytes = 2, Cycles = 4, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.STY, AddressingMode = EnumAddressingMode.Absolute, Opcode = 0x8C, Bytes = 3, Cycles = 4, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.TAX, AddressingMode = EnumAddressingMode.Implied, Opcode = 0xAA, Bytes = 1, Cycles = 2, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.TAY, AddressingMode = EnumAddressingMode.Implied, Opcode = 0xA8, Bytes = 1, Cycles = 2, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.TYA, AddressingMode = EnumAddressingMode.Implied, Opcode = 0x98, Bytes = 1, Cycles = 2, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.TSX, AddressingMode = EnumAddressingMode.Implied, Opcode = 0xBA, Bytes = 1, Cycles = 2, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.TXA, AddressingMode = EnumAddressingMode.Implied, Opcode = 0x8A, Bytes = 1, Cycles = 2, ExtraCycle = false };
                new Instruction { Operation = EnumOperation.TXS, AddressingMode = EnumAddressingMode.Implied, Opcode = 0x9A, Bytes = 1, Cycles = 2, ExtraCycle = false };
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
                    return instruction.AddressingMode == EnumAddressingMode.Immediate ||
                        instruction.AddressingMode == EnumAddressingMode.Implied ? EnumTstate.T0T2 : EnumTstate.T2;

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