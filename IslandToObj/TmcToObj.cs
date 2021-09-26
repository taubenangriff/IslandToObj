using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Numerics;
using System.IO;

namespace IslandToObj
{
    struct Vertex
    {
        public Vector3 Position;
        public Vector3 Normal;
    }

    struct Mesh 
    {
        public Vertex[] Vertices;
        public int[] Indices; 
    }

    class TmcToObj
    {
        public TmcToObj() 
        {
            GetMesh(); 
        }

        private Mesh MakeMesh(XmlNode x) 
        {
            var VertexNode = x.SelectSingleNode("./Vertices").InnerText;
            var IndexNode = x.SelectSingleNode("./Indices").InnerText;

            var Vertices = ToVertices(VertexNode);
            var Indices = ToIndices(IndexNode);

            return new Mesh() { Vertices = Vertices, Indices = Indices };
        }

        private void GetMesh() 
        {
            XmlDocument doc = new XmlDocument();
            doc.Load("tmc/0x0.xml");

            //for all
            var Mesh = doc.SelectNodes("//None[Vertices]");
            WriteObjFile(Mesh);
        }

        private void WriteObjFile(XmlNodeList Meshes) {
            using (StreamWriter writer = new StreamWriter(File.Create("fuck.obj"))) {
                int faceoffset = 0; 
                foreach (XmlNode n in Meshes)
                {
                    //make a mesh from n first so we can ask it for facecount later!
                    Mesh mesh = MakeMesh(n);
                    if (mesh.Vertices != null) {
                        writer.Write(ConvertToObj(mesh, faceoffset));
                        faceoffset += mesh.Vertices.Length;
                    }
                }
            } 
        }

        private void WriteObjFile(XmlNode Meshes)
        {
            using (StreamWriter writer = new StreamWriter(File.Create("fuck.obj")))
            {
                writer.Write(ConvertToObj(MakeMesh(Meshes), 0));
            }
        }

        private String ConvertToObj(Mesh Mesh, int FaceOffset)
        {
            StringBuilder s = new StringBuilder(); 
            
            //write vertices
            foreach (Vertex v in Mesh.Vertices)
            {
                s.Append("v " + v.Position.X / 32F  + " " + v.Position.Y / 128F + " " + v.Position.Z / 32F+ "\n");
            }
            //write indices
            for (int i = 0; i < Mesh.Indices.Length-2; i+=3) 
            {
                s.Append("f " + (Mesh.Indices[i] + FaceOffset) + " " + (Mesh.Indices[i+1] + FaceOffset) + " " + (Mesh.Indices[i+2] + FaceOffset) + "\n");
            }
            
            return s.ToString(); 
        }

        private int[] ToIndices(String IndexList) {
            if (IndexList.Equals("")) {
                return null; 
            }
            String[] indexStr = IndexList.Split(' ');
            int[] indices = new int[indexStr.Length];
            for (int i = 0; i < indexStr.Length; i++)
            {
                //imagine starting offsets at 1 bruh
                indices[i] = Int32.Parse(indexStr[i]) + 1;
            }
            return indices;
        }

        private Vertex[] ToVertices(String VertexList)
        {
            if (VertexList.Equals(""))
            {
                return null;
            }
            //Split up the string data
            String[] StringArr = VertexList.Split(' ');

            int vertexCount = StringArr.Length / 8;

            Vertex[] vertices = new Vertex[vertexCount];

            for (int i = 0; i < vertexCount; i++)
            {
                Vertex v = new Vertex();
                Vector3 Position = new Vector3();
                Position.X = ToInt(StringArr[i*8]);
                Position.Y = (ushort)ToInt(StringArr[i * 8 + 1]) - short.MaxValue;
                Position.Z = ToInt(StringArr[i * 8 + 2]);
                v.Position = Position;

                vertices[i] = v; 
            }

            return vertices; 

            //position 
        }

        /// <summary>
        /// accounts for the uint storing of height
        /// </summary>
        /// <param name="Height"></param>
        /// <returns></returns>
        private Int16 ToInt(String s)
        {
            return Int16.Parse(s);
        }


        private int GetMaxIndex(int[] indices) {
            int maxIndex = -1;
            for (int i = 0; i < indices.Length; i++) 
            {
                if (indices[i] > maxIndex) 
                    maxIndex = indices[i];
            }
            return maxIndex; 
        }
    }
}
