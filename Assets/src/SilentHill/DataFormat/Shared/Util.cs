
namespace SH.DataFormat.Shared
{
    public static class Util
    {
        public static void AlignOffsetToNext(ref int offset, int alignment = 0x10)
        {
            if (offset % alignment != 0)
            {
                offset += alignment - (offset % alignment);
            }
        }


        //Taken from https://github.com/hglm/detex/blob/master/half-float.c
        public static unsafe float HalfToSingleFloat(ushort source)
        {
            uint target;
            ushort* hp = (ushort*)&source; // Type pun input as an unsigned 16-bit int
            uint* xp = (uint*)&target; // Type pun output as an unsigned 32-bit int
            ushort h, hs, he, hm;
            uint xs, xe, xm;
            int xes;
            int e;

            h = *hp++;
            if ((h & 0x7FFFu) == 0)
            {  // Signed zero
                *xp++ = ((uint)h) << 16;  // Return the signed zero
            }
            else
            { // Not zero
                hs = (ushort)(h & 0x8000u);  // Pick off sign bit
                he = (ushort)(h & 0x7C00u);  // Pick off exponent bits
                hm = (ushort)(h & 0x03FFu);  // Pick off mantissa bits
                if (he == 0)
                {  // Denormal will convert to normalized
                    e = -1; // The following loop figures out how much extra to adjust the exponent
                    do
                    {
                        e++;
                        hm <<= 1;
                    } while ((hm & 0x0400u) == 0); // Shift until leading bit overflows into exponent bit
                    xs = ((uint)hs) << 16; // Sign bit
                    xes = (int)(((uint)(he >> 10)) - 15 + 127 - e); // Exponent unbias the halfp, then bias the single
                    xe = (uint)(xes << 23); // Exponent
                    xm = ((uint)(hm & 0x03FFu)) << 13; // Mantissa
                    *xp++ = (xs | xe | xm); // Combine sign bit, exponent bits, and mantissa bits
                }
                else if (he == 0x7C00u)
                {  // Inf or NaN (all the exponent bits are set)
                    if (hm == 0)
                    { // If mantissa is zero ...
                        *xp++ = (((uint)hs) << 16) | ((uint)0x7F800000u); // Signed Inf
                    }
                    else
                    {
                        *xp++ = (uint)0xFFC00000u; // NaN, only 1st mantissa bit set
                    }
                }
                else
                { // Normalized number
                    xs = ((uint)hs) << 16; // Sign bit
                    xes = (int)(((uint)(he >> 10)) - 15 + 127); // Exponent unbias the halfp, then bias the single
                    xe = (uint)(xes << 23); // Exponent
                    xm = ((uint)hm) << 13; // Mantissa
                    *xp++ = (xs | xe | xm); // Combine sign bit, exponent bits, and mantissa bits
                }
            }
            return *(float*)&target;
        }

        //Taken from https://github.com/hglm/detex/blob/master/half-float.c
        public static unsafe ushort SingleToHalfFloat(float source)
        {
            ushort target;
            ushort* hp = (ushort*)&target; // Type pun output as an unsigned 16-bit int
            uint* xp = (uint*)&source; // Type pun input as an unsigned 32-bit int
            ushort hs, he, hm;
            uint x, xs, xe, xm;
            int hes;

            x = *xp++;
            if ((x & 0x7FFFFFFFu) == 0)
            {  // Signed zero
                *hp++ = (ushort)(x >> 16);  // Return the signed zero
            }
            else
            { // Not zero
                xs = x & 0x80000000u;  // Pick off sign bit
                xe = x & 0x7F800000u;  // Pick off exponent bits
                xm = x & 0x007FFFFFu;  // Pick off mantissa bits
                if (xe == 0)
                {  // Denormal will underflow, return a signed zero
                    *hp++ = (ushort)(xs >> 16);
                }
                else if (xe == 0x7F800000u)
                {  // Inf or NaN (all the exponent bits are set)
                    if (xm == 0)
                    { // If mantissa is zero ...
                        *hp++ = (ushort)((xs >> 16) | 0x7C00u); // Signed Inf
                    }
                    else
                    {
                        *hp++ = (ushort)0xFE00u; // NaN, only 1st mantissa bit set
                    }
                }
                else
                { // Normalized number
                    hs = (ushort)(xs >> 16); // Sign bit
                    hes = ((int)(xe >> 23)) - 127 + 15; // Exponent unbias the single, then bias the halfp
                    if (hes >= 0x1F)
                    {  // Overflow
                        *hp++ = (ushort)((xs >> 16) | 0x7C00u); // Signed Inf
                    }
                    else if (hes <= 0)
                    {  // Underflow
                        if ((14 - hes) > 24)
                        {  // Mantissa shifted all the way off & no rounding possibility
                            hm = (ushort)0u;  // Set mantissa to zero
                        }
                        else
                        {
                            xm |= 0x00800000u;  // Add the hidden leading bit
                            hm = (ushort)(xm >> (14 - hes)); // Mantissa
                            if (((xm >> (13 - hes)) & 0x00000001u) != 0) // Check for rounding
                                hm += (ushort)1u; // Round, might overflow into exp bit, but this is OK
                        }
                        *hp++ = (ushort)(hs | hm); // Combine sign bit and mantissa bits, biased exponent is zero
                    }
                    else
                    {
                        he = (ushort)(hes << 10); // Exponent
                        hm = (ushort)(xm >> 13); // Mantissa
                        if ((xm & 0x00001000u) != 0) // Check for rounding
                            *hp++ = (ushort)((hs | he | hm) + 1); // Round, might overflow to inf, this is OK
                        else
                            *hp++ = (ushort)(hs | he | hm);  // No rounding
                    }
                }
            }
            return target;
        }
    }
}
