using System;
using System.Collections.Generic;
using System.IO;

namespace csg_NET
{
    public class MapFile
    {
        public List<Entity> entityList;
        public string FilePath { get; private set; }
        int entities = 0;
        int polygons = 0;
        int textures = 0;

        public MapFile(string path)
        {
            entityList = new List<Entity>();
            FilePath = path;
        }

        public void Load(Tokenizer tokenizer)
        {
            entityList.Clear();
            entities = 0;
            polygons = 0;
            textures = 0;

            while (tokenizer.PeekNextToken().Type != Tokenizer.TokenType.EndOfStream)
            {
                var token = tokenizer.GetNextToken();
                if (token.Type == Tokenizer.TokenType.StartBlock)
                {
                    Entity e = new Entity();
                    Brush brushList = null;

                    token = tokenizer.GetNextToken();
                    while (token.Type != Tokenizer.TokenType.EndBlock)
                    {
                        switch (token.Type)
                        {
                            case Tokenizer.TokenType.Value: // Key/value pair
                                var value = tokenizer.GetNextToken();
                                if (value.Type == Tokenizer.TokenType.Value)
                                {
                                    e.Properties.Add(token.Contents, value.Contents);
                                }
                                else
                                {
                                    throw new FormatException(String.Format("Expected a value, received a {0}", value));
                                }
                                break;
                            case Tokenizer.TokenType.StartBlock: // Brush

                                Brush b = new Brush();
                                Face faces = null;
                                int uiFaces = 0;

                                while (tokenizer.PeekNextToken().Type != Tokenizer.TokenType.EndBlock)
                                {
                                    Face face = new Face();

                                    Vector3 v1 = Vector3Extension.FromToken(tokenizer);
                                    Vector3 v2 = Vector3Extension.FromToken(tokenizer);
                                    Vector3 v3 = Vector3Extension.FromToken(tokenizer);
                                    face.plane = new Plane(v1, v3, v2);

                                    // TODO: read texture maybe?
                                    string textureName = tokenizer.GetNextValue();

                                    // parsing
                                    int xOffset = Convert.ToInt32(tokenizer.GetNextValue());
                                    int yOffset = Convert.ToInt32(tokenizer.GetNextValue());
                                    int rotation = Convert.ToInt32(tokenizer.GetNextValue());
                                    float xScale = Convert.ToSingle(tokenizer.GetNextValue());
                                    float yScale = Convert.ToSingle(tokenizer.GetNextValue());

                                    face.texAxis = new Plane[]
                                    {
                                        new Plane { d = xOffset },
                                        new Plane { d = yOffset }
                                    };

                                    face.texRotation = rotation;
                                    face.texScale = new float[] { xScale, yScale };

                                    if(faces == null)
                                    {
                                        // assign as the fist face
                                        faces = face;
                                    }
                                    else
                                    {
                                        // add face to the list
                                        faces.AddFace(face);
                                    }
                                    uiFaces++;

                                    //var newFace = new Face(v1, v2, v3, textureName, xOffset, yOffset, rotation, xScale, yScale);
                                    //b.AddFace(newFace);
                                }

                                Poly polyList = faces.GetPolys();

                                // Sort vertices and calculate texture coordinates for every polygon
                                Poly pi = polyList;
                                Face f = faces;

                                for (int c = 0; c < uiFaces; c++)
                                {
                                    pi.plane = f.plane;
                                    pi.TextureID = f.texture.ID;

                                    pi.SortVerticesCW();

                                    //pi.CalculateTextureCoordinates(
                                    //    f.texture.GetWidth,
                                    //    f.texture.GetHeight,
                                    //    f.texAxis[0], f.texAxis[1],
                                    //    f.texScale[0], f.texScale[1]);

                                    f = f.Next;
                                    pi = pi.Next;
                                }

                                b.AddPoly(polyList);
                                b.CalculateAABB();

                                if (brushList == null)
                                {
                                    brushList = b;
                                }
                                else
                                {
                                    Brush temp = brushList;
                                    while (!temp.IsLast) temp = temp.Next;
                                    temp.SetNext(b);
                                }

                                tokenizer.GetNextToken(); // Brush end block
                                break;
                            default:
                                throw new FormatException(String.Format("Expected either a block start or a value, received a {0}", token));
                        }
                        token = tokenizer.GetNextToken();
                    }

                    // End of entity

                    // Perform CSG union
                    if (brushList != null)
                    {
                        e.AddPoly(brushList.MergeList());
                        brushList = null;
                        polygons += e.GetNumberOfPolys();
                    }

                    entityList.Add(e);   
                }
            }

            Console.WriteLine("Map created.");
            Console.WriteLine(" -" + entities + " entities loaded.");
            Console.WriteLine(" -" + polygons + " polygons loaded.");
        }

        public void Reload()
        {
            using (StreamReader fStream = new StreamReader(FilePath))
            {
                string mapStr = fStream.ReadToEnd();
                Tokenizer tokenizer = new Tokenizer(mapStr);
                Load(tokenizer);
            }
        }
    }
}
