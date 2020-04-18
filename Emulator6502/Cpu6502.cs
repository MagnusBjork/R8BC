
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Emulator6502
{

    public enum EnumTstate
    {
        T0, T1, T2, T0T2, T3, T4, T5, T6
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
        public StatusRegister P { get; private set; } = new StatusRegister();
        public Instruction Instruction { get; private set; } = new Instruction();


        private readonly IAddressBus _addressBus;
        private readonly IDataBus _dataBus;
        private readonly InstructionSet _instructionSet;
        private EnumTstate _tState;
        private byte _adl;
        private byte _adh;
        private byte _pd;
        private byte _alu;

        public Cpu6502(IAddressBus addressBus, IDataBus dataBus)
        {
            _addressBus = addressBus;
            _dataBus = dataBus;
            _instructionSet = new InstructionSet();

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

                Instruction = _instructionSet.GetInstruction(_dataBus.Data);
                IR = Instruction.Opcode;
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
                    RW = false;
                    _dataBus.Data = A;
                    Console.WriteLine($"Write to memory - IR: {IR:X2} - T0 Execute - ADH: {_adh:X2} ADL: {_adl:X2} - Accu: {A:X2}");
                }
            }




            _tState = GetNextTstate(_tState, Instruction);
        }

        private void LoadAddress(ushort address)
        {
            _adl = (byte)(address);
            _adh = (byte)(address >> 8);
        }




        // private Instruction LoadToDecoder(byte opcode)
        // {


        //     return instructions.Where(i => i.Opcode.Equals(opcode)).SingleOrDefault() ?? new Instruction();
        // }

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