using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace Lab1.Model
{
    class RGB
    {
        private int[,] Red;
        private int[,] Green;
        private int[,] Blue;
        private YUV yuv;
        private int width;
        private int height;

        public RGB(int width, int height)
        {
            this.Red = new int[height, width];
            this.Green = new int[height, width];
            this.Blue = new int[height, width];
            this.yuv = new YUV(width, height);
            this.width = width;
            this.height = height;
        }

        public void ConvertToYUV()
        {
            for (int i = 0; i < this.height; i++)
                for (int j = 0; j < this.width; j++)
                    this.yuv.Init(i,j,this.Red[i,j],this.Green[i,j], this.Blue[i,j]);
            this.yuv.Converts();
        }

        public void Init(StreamReader streamRead)
        {
            string line;
            int count = 0;
            int iR=0, jR=0, iG=0, jG=0, iB=0, jB=0; 

            while((line = streamRead.ReadLine()) != null)
            {
                if (count % 3 == 0 && iR != this.height)
                {
                    this.Red[iR, jR] = int.Parse(line);
                    jR++;
                    if (jR % this.width == 0)
                    {
                        iR++;
                        jR = 0;
                    }
                }
                if (count % 3 == 1 && iG != this.height)
                {
                    this.Green[iG, jG] = int.Parse(line);
                    jG++;
                    if (jG % this.width == 0)
                    {
                        iG++;
                        jG = 0;
                    }
                }
                if (count % 3 == 2 && iB != this.height)
                {
                    this.Blue[iB, jB] = int.Parse(line);
                    jB++;
                    if (jB % this.width == 0)
                    {
                        iB++;
                        jB = 0;
                    }
                }
                count++;
            }
            streamRead.Close();
        }

        public void Display()
        {
            this.yuv.DisplayEntropy();
        }

        public void ConvertYUVToRGB()
        {
            this.yuv.DecodeEntropy();
            this.yuv.DeQuantization();
            this.yuv.ConvertDCTToYUV();
            this.yuv.ConvertBackToYUV();

            for (int i = 0; i < this.height; i++)
                for(int j = 0; j < this.width; j++)
                {
                    int R = this.yuv.Red(i,j);
                    int B = this.yuv.Blue(i,j);
                    int G = this.yuv.Green(i,j);

                    R = Check(R);
                    G = Check(G);
                    B = Check(B);

                    this.Red[i, j] = R ;
                    this.Green[i, j] = G ;
                    this.Blue[i, j] = B ;
                }
        }

        private int Check(int elem)
        {
            if (elem < 0)
                return 0;
            if (elem > 255)
                return 255;
            return elem;
        }

        public void WriteToFile()
        {
            using (StreamWriter file = new StreamWriter(@"D:\\Faculta\\An III\\Semestru_1\\PDAV\\result_02.ppm"))
            {
                file.WriteLine("P3");
                file.WriteLine("# CREATOR: GIMP PNM Filter Version 1.1");
                file.WriteLine("800 600");

                for (int i = 0; i< this.height; i++)
                    for (int j = 0; j < this.width; j++)
                    {
                        file.WriteLine(this.Red[i,j]);
                        file.WriteLine(this.Green[i,j]);
                        file.WriteLine(this.Blue[i,j]);
                    }
                file.Close();
            }
        }

        public string ToStringRed()
        {
            string str = "";
            for (int i = 0; i < this.height; i++)
            {
                for (int j = 0; j < this.width; j++)
                    str = str + " " + this.Red[i,j];
                str = str + "\n";
            }
            return str;
        }

        public string ToStringGreen()
        {
            string str = "";
            for (int i = 0; i < this.height; i++)
            {
                for (int j = 0; j < this.width; j++)
                    str = str + " " + this.Green[i, j];
                str = str + "\n";
            }
            return str;
        }

        public string ToStringBlue()
        {
            string str = "";
            for (int i = 0; i < this.height; i++)
            {
                for (int j = 0; j < this.width; j++)
                    str = str + " " + this.Blue[i, j];
                str = str + "\n";
            }
            return str;
        }
    }
}
