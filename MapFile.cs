using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace csg_NET
{
    public class MapFile
    {
        enum Result { RESULT_SUCCEED = 0, RESULT_FAIL, RESULT_EOF }
        string token;

        // TODO: what is this type ???
        // HANLDE handleFile;

        int WADFiles;
        // TODO: what is this ??
        // LPVOID m_pWad;
        Texture[] textureList;

        int entities;
        int polygons;
        int textures;

        Result GetToken() { }
        Result GetString() { }
    }
}
