using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;

namespace TestConsol
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var mmf = MemoryMappedFile.CreateFromFile("ImgA", FileMode.OpenOrCreate, null, 900000L))
            {
                using (var accessor = mmf.CreateViewAccessor(0, 900000))
                {
                    int colorSize = Marshal.SizeOf(typeof(MyColor));
                    MyColor color;

                    color.G = 10;
                    color.R = 10;
                    color.B = 10;
                    
                    accessor.Write(0, ref color);

                    color.G = -100;

                    accessor.Read(0,out MyColor myColor2);
                    
                    
                    
                }
            }
        }
    }

    struct MyColor
    {
        public int R;
        public int G;
        public int B;
    }
}