using System.Globalization;

namespace Emulator6502
{
    public static class ExtensionMethods
    {
        public static ushort? ToMemoryAddress(this string memoryAddressHexString)
        {
            memoryAddressHexString = memoryAddressHexString.ToLower().Replace("0x", string.Empty);

            ushort address = 0;
            var validAddress = ushort.TryParse(memoryAddressHexString, NumberStyles.AllowHexSpecifier, null, out address);

            if (validAddress)
                return address;
            else
                return null;
        }
    }
}