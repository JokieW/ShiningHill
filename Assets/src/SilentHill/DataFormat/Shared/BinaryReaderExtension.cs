using System.IO;

namespace SH.DataFormat.Shared
{
    public static class BinaryReaderExtension
    {
        public static float ReadHalf(this BinaryReader reader)
        {
            return Util.HalfToSingleFloat(reader.ReadUInt16());
        }
    }
}