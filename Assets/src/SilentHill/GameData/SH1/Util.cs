using System.Collections.Generic;

namespace SH.GameData.SH1
{
    public static class Util
    {
        public static unsafe string DecodeSH1Name(uint n1, uint n2)
        {
            List<char> name = new List<char>();

            n1 >>= 4;
            int i;
            for (i = 0; i < 4 && n1 != 0; i++)
            {
                char c = (char)((n1 & 0x3F) + 0x20);
                if (c != '\0') name.Add(c);
                n1 >>= 6;
            }

            n2 &= 0xFFFFFF;
            int l = i + 4;
            for (; i < l && n2 != 0; i++)
            {
                char c = (char)((n2 & 0x3F) + 0x20);
                if (c != '\0') name.Add(c);
                n2 >>= 6;
            }
            return new string(name.ToArray());
        }

        public static unsafe uint DecodeSH1Size(uint v)
        {
            return ((v >> 0x13) & 0xFFFF) << 8;
        }
    }
}
