
using System;

namespace Emulator6502
{
    public interface IDataBus
    {
        byte Data { get; set; }
    }

    public class DataBus : IDataBus
    {
        public byte Data { get; set; }
    }
}
