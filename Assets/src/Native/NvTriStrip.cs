using System.Runtime.InteropServices;

public static class NvTriStrip
{
    public enum PrimType
    {
        PT_LIST,
        PT_STRIP,
        PT_FAN
    };

    public unsafe struct PrimitiveGroup
    {
        public PrimType type;
        public uint numIndices;
        public ushort* indices;
    }

    ////////////////////////////////////////////////////////////////////////////////////////
    // EnableRestart()
    //
    // For GPUs that support primitive restart, this sets a value as the restart index
    //
    // Restart is meaningless if strips are not being stitched together, so enabling restart
    //  makes NvTriStrip forcing stitching.  So, you'll get back one strip.
    //
    // Default value: disabled
    //
    [DllImport("NvTriStrip.dll")]
    public unsafe static extern void EnableRestart(uint restartVal);

    ////////////////////////////////////////////////////////////////////////////////////////
    // DisableRestart()
    //
    // For GPUs that support primitive restart, this disables using primitive restart
    //
    [DllImport("NvTriStrip.dll")]
    public unsafe static extern void DisableRestart();


    ////////////////////////////////////////////////////////////////////////////////////////
    // SetCacheSize()
    //
    // Sets the cache size which the stripfier uses to optimize the data.
    // Controls the length of the generated individual strips.
    // This is the "actual" cache size, so 24 for GeForce3 and 16 for GeForce1/2
    // You may want to play around with this number to tweak performance.
    //
    // Default value: 16
    //
    [DllImport("NvTriStrip.dll")]
    public unsafe static extern void SetCacheSize(uint cacheSize);


    ////////////////////////////////////////////////////////////////////////////////////////
    // SetStitchStrips()
    //
    // bool to indicate whether to stitch together strips into one huge strip or not.
    // If set to true, you'll get back one huge strip stitched together using degenerate
    //  triangles.
    // If set to false, you'll get back a large number of separate strips.
    //
    // Default value: true
    //
    [DllImport("NvTriStrip.dll")]
    public unsafe static extern void SetStitchStrips(bool bStitchStrips);


    ////////////////////////////////////////////////////////////////////////////////////////
    // SetMinStripSize()
    //
    // Sets the minimum acceptable size for a strip, in triangles.
    // All strips generated which are shorter than this will be thrown into one big, separate list.
    //
    // Default value: 0
    //
    [DllImport("NvTriStrip.dll")]
    public unsafe static extern void SetMinStripSize(uint minSize);


    ////////////////////////////////////////////////////////////////////////////////////////
    // SetListsOnly()
    //
    // If set to true, will return an optimized list, with no strips at all.
    //
    // Default value: false
    //
    [DllImport("NvTriStrip.dll")]
    public unsafe static extern void SetListsOnly(bool bListsOnly);


    ////////////////////////////////////////////////////////////////////////////////////////
    // GenerateStrips()
    //
    // in_indices: input index list, the indices you would use to render
    // in_numIndices: number of entries in in_indices
    // primGroups: array of optimized/stripified PrimitiveGroups
    // numGroups: number of groups returned
    //
    // Be sure to call delete[] on the returned primGroups to avoid leaking mem
    //
    [DllImport("NvTriStrip.dll")]
    public unsafe static extern bool GenerateStrips(ushort* in_indices, uint in_numIndices, out PrimitiveGroup* primGroups, out ushort numGroups, bool validateEnabled = false);


    ////////////////////////////////////////////////////////////////////////////////////////
    // DeletePrimitives()
    //
    // primGroups: the pointer given by GenerateStrips() to delete
    //
    [DllImport("NvTriStrip.dll")]
    public unsafe static extern void DeletePrimitives(PrimitiveGroup* primGroups);


    ////////////////////////////////////////////////////////////////////////////////////////
    // RemapIndices()
    //
    // Function to remap your indices to improve spatial locality in your vertex buffer.
    //
    // in_primGroups: array of PrimitiveGroups you want remapped
    // numGroups: number of entries in in_primGroups
    // numVerts: number of vertices in your vertex buffer, also can be thought of as the range
    //  of acceptable values for indices in your primitive groups.
    // remappedGroups: array of remapped PrimitiveGroups
    //
    // Note that, according to the remapping handed back to you, you must reorder your 
    //  vertex buffer.
    //
    // Credit goes to the MS Xbox crew for the idea for this interface.
    //
    [DllImport("NvTriStrip.dll")]
    public unsafe static extern void RemapIndices(PrimitiveGroup* in_primGroups, ushort numGroups, ushort numVerts, out PrimitiveGroup* remappedGroups);
}