
using System;

namespace Emulator6502
{
    public interface IAddressBus
    {
        ushort Address { get; set; }
    }

    public class AddressBus : IAddressBus
    {
        public ushort Address { get; set; }
    }
}
