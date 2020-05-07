
namespace SH.GameData.SH3
{
    public static class Util
    {
        //Taken from the SH3 code
        public static unsafe float HalfToSingleFloatSHStyle(ushort source)
        {
            int i = ((((source & 0x7C00) + 0x1C000) << 13) | ((source & 0x3FF | 8 * (source & 0x8000)) << 13));
            return *(float*)&i;
        }
    }
}
