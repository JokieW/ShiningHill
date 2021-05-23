
struct sh2_map_header
{
    int magicByte; // 0x20010510
    int fielLength;
    int fileCount;
    int field_0C;
};

struct sh2_map_subfile_header
{
    int subFileType; // 1 = geometry, 2 = texture
    int fileLength;
    int field_08;
    int field_0C;
};



struct sh2_map
{
    struct sh2_map_header header;
    struct sh2_map_subfile_header* subFiles;
};


