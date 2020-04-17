
using System;
using System.Collections.Generic;

namespace Emulator6502
{
    public interface IMemory
    {
        //  void Enable(bool RW);
        void BulkUploadMemoryImage(Dictionary<UInt16, byte> data);
        void SetByte(ushort pointer, byte data);
    }

    public class Memory : IMemory
    {

        private IAddressBus _addressBus;
        private IDataBus _dataBus;
        private byte[] _memoryArea;


        public Memory(IAddressBus addressBus, IDataBus dataBus)
        {
            _addressBus = addressBus;
            _dataBus = dataBus;

            _memoryArea = new byte[0xFFFF + 1];
        }


        // public void Enable(bool rw)
        // {
        //     if (rw)
        //         _dataBus.Data = _memoryArea[_addressBus.Address];   // rw = 1: Read from memory
        //     else
        //         _memoryArea[_addressBus.Address] = _dataBus.Data;   // rw = 0: Write to memory
        // }

        public void Read() => _dataBus.Data = _memoryArea[_addressBus.Address];

        public void Write() => _memoryArea[_addressBus.Address] = _dataBus.Data;



        public void SetByte(ushort pointer, byte data)
        {
            _memoryArea[pointer] = data;
        }


        public byte GetByte(ushort pointer)
        {
            return _memoryArea[pointer];
        }

        public void BulkUploadMemoryImage(Dictionary<UInt16, byte> data)
        {
            foreach (var dataRow in data)
            {
                _memoryArea[dataRow.Key] = dataRow.Value;
            }

        }
    }
}