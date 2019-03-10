using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Lab1.Model;

namespace Lab1
{
    class Program
    {
        static void Main(string[] args)
        { 
            string filename = @"D:\\Faculta\\An III\\Semestru_1\\PDAV\\nt-P3.ppm";
            StreamReader file = new StreamReader(filename);
            string P3, comment, size;
            P3 = file.ReadLine();
            comment = file.ReadLine();
            size = file.ReadLine();
            string[] strs = size.Split(' ');
            RGB rgb = new RGB(int.Parse(strs[0]), int.Parse(strs[1]));
            rgb.Init(file);
            rgb.ConvertToYUV();
            //rgb.Display();
            rgb.ConvertYUVToRGB();
            rgb.WriteToFile();
            Console.WriteLine("done!");
            Console.ReadLine();
        }
    }
}
