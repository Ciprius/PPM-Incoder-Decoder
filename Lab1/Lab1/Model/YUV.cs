using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab1.Model
{
    class YUV
    {
        private double[,] Y;
        private double[,] U;
        private double[,] V;
        private int width;
        private int height;
        private int pos = 0;
        private List<Matrix<double>> Y8X8;
        private List<Matrix<double>> U8X8;
        private List<Matrix<double>> V8X8;
        private List<Matrix<double>> U4X4;
        private List<Matrix<double>> V4X4;
        private List<Matrix<double>> DCTY;
        private List<Matrix<double>> DCTU;
        private List<Matrix<double>> DCTV;
        private List<Matrix<int>> QDCTY;
        private List<Matrix<int>> QDCTU;
        private List<Matrix<int>> QDCTV;
        private Dictionary<int, List<int>> amplitudes = new Dictionary<int, List<int>>();
        private List<int> entropy;
        private int[,] quantizer = new int[,] {
                {6,4,4,6,10,16,20,24},
                {5,5,6,8,10,23,24,22},
                {6,5,6,10,16,23,28,22},
                {6,7,9,12,20,35,32,25},
                {7,9,15,22,27,44,41,31},
                {10,14,22,26,32,42,45,37},
                {20,26,31,35,41,48,48,40},
                {29,37,38,39,45,40,41,40}};

        public YUV(int width, int height)
        {
            this.Y = new double[height, width];
            this.U = new double[height, width];
            this.V = new double[height, width];
            this.width = width;
            this.height = height;
            this.Y8X8 = new List<Matrix<double>>();
            this.U8X8 = new List<Matrix<double>>();
            this.V8X8 = new List<Matrix<double>>();
            this.U4X4 = new List<Matrix<double>>();
            this.V4X4 = new List<Matrix<double>>();
            this.DCTY = new List<Matrix<double>>();
            this.DCTU = new List<Matrix<double>>();
            this.DCTV = new List<Matrix<double>>();
            this.QDCTY = new List<Matrix<int>>();
            this.QDCTU = new List<Matrix<int>>();
            this.QDCTV = new List<Matrix<int>>();
            this.entropy = new List<int>();

            this.amplitudes.Add(1, new List<int> { -1, 1 });
            this.amplitudes.Add(2, new List<int> { -3, -2, 2, 3 });
            this.amplitudes.Add(3, new List<int> { -7, -4, 4, 7 });
            this.amplitudes.Add(4, new List<int> { -15, -8, 8, 15 });
            this.amplitudes.Add(5, new List<int> { -31, -16, 16, 31 });
            this.amplitudes.Add(6, new List<int> { -63, -32, 32, 63 });
            this.amplitudes.Add(7, new List<int> { -127, -64, 64, 127 });
            this.amplitudes.Add(8, new List<int> { -255, -128, 128, 255 });
            this.amplitudes.Add(9, new List<int> { -511, -256, 256, 511 });
            this.amplitudes.Add(10, new List<int> { -1023, -512, 512, 1023 });
        }

        public void Init(int row, int col, int R, int G, int B)
        {
            this.Y[row, col] = 0.257 * R + 0.504 * G + 0.098 * B + 16;
            this.U[row, col] = -0.148 * R - 0.291 * G + 0.439 * B + 128;
            this.V[row, col] = 0.439 * R - 0.368 * G - 0.071 * B + 128;
        }

        public void Converts()
        {
            for (int i = 0; i < this.height; i = i + 8)
            {
                for (int j = 0; j < this.width; j = j + 8)
                {
                    double[,] matY = new double[8, 8];
                    double[,] matU = new double[8, 8];
                    double[,] matV = new double[8, 8];
                    int row = 0, col = 0;
                    for (int k = i; k < i + 8; k++)
                    {
                        for (int t = j; t < j + 8; t++)
                        {
                            matY[row, col] = this.Y[k, t];
                            matU[row, col] = this.U[k, t];
                            matV[row, col] = this.V[k, t];
                            col++;
                        }
                        col = 0;
                        row++;
                    }

                    Matrix<double> matrixY = new Matrix<double>(i, j, matY, "Y", 8);
                    Matrix<double> matrixU = new Matrix<double>(i, j, matU, "U", 8);
                    Matrix<double> matrixV = new Matrix<double>(i, j, matV, "V", 8);
                    this.Y8X8.Add(matrixY);
                    this.U8X8.Add(matrixU);
                    this.V8X8.Add(matrixV);
                }
            }
            ConvertU();
            ConvertV();
            ConvertUVTo8x8();
            ConvertToDCT();
            Quantization();
            Entropy();
        }

        private void ConvertU()
        {
            foreach (Matrix<double> mat in this.U8X8)
            {
                double[,] matrix = mat.GetMatrix();
                double[,] mat4x4 = new double[4, 4];
                int row = 0, col = 0;
                for (int i = 0; i < 8; i = i + 2)
                {
                    for (int j = 0; j < 8; j = j + 2)
                    {
                        mat4x4[row, col] = (matrix[i, j] + matrix[i + 1, j] + matrix[i, j + 1] + matrix[i + 1, j + 1]) / 4;
                        //Math.Max(Math.Max(matrix[i, j],matrix[i + 1, j]),Math.Max(matrix[i, j + 1],matrix[i + 1, j + 1]));
                        col++;
                    }
                    row++;
                    col = 0;
                }
                this.U4X4.Add(new Matrix<double>(mat.GetRowIndex(), mat.GetColIndex(), mat4x4, "U", 4));
            }
        }

        private void ConvertV()
        {
            foreach (Matrix<double> mat in this.V8X8)
            {
                double[,] matrix = mat.GetMatrix();
                double[,] mat4x4 = new double[4, 4];
                int row = 0, col = 0;
                for (int i = 0; i < 8; i = i + 2)
                {
                    for (int j = 0; j < 8; j = j + 2)
                    {
                        mat4x4[row, col] = (matrix[i, j] + matrix[i + 1, j] + matrix[i, j + 1] + matrix[i + 1, j + 1]) / 4;
                        //Math.Max(Math.Max(matrix[i, j], matrix[i + 1, j]), Math.Max(matrix[i, j + 1], matrix[i + 1, j + 1]));
                        col++;
                    }
                    row++;
                    col = 0;
                }
                this.V4X4.Add(new Matrix<double>(mat.GetRowIndex(), mat.GetColIndex(), mat4x4, "V", 4));
            }
        }

        private void ConvertUVTo8x8()
        {
            for (int k = 0; k < this.U4X4.Count; k++)
            {
                double[,] matU = new double[8, 8];
                double[,] matV = new double[8, 8];
                int row = 0, col = 0;
                for (int i = 0; i < 8; i = i + 2)
                {
                    for (int j = 0; j < 8; j = j + 2)
                    {
                        matU[i, j] = matU[i + 1, j] = matU[i, j + 1] = matU[i + 1, j + 1] = this.U4X4[k].GetMatrix()[row, col];
                        matV[i, j] = matV[i + 1, j] = matV[i, j + 1] = matV[i + 1, j + 1] = this.V4X4[k].GetMatrix()[row, col];
                        col++;
                    }
                    row++;
                    col = 0;
                }
                this.U8X8[k].SetMatrix(matU);
                this.V8X8[k].SetMatrix(matV);
            }
        }

        private void ConvertToDCT()
        {
            for (int k = 0; k < this.Y8X8.Count; k++)
            {
                double[,] matrixY = this.Y8X8[k].GetMatrix();
                double[,] matrixU = this.U8X8[k].GetMatrix();
                double[,] matrixV = this.V8X8[k].GetMatrix();
                for (int i = 0; i < 8; i++)
                    for (int j = 0; j < 8; j++)
                    {
                        matrixY[i, j] = matrixY[i, j] - 128.0;
                        matrixU[i, j] = matrixU[i, j] - 128.0;
                        matrixV[i, j] = matrixV[i, j] - 128.0;
                    }
                this.Y8X8[k].SetMatrix(matrixY);
                this.U8X8[k].SetMatrix(matrixU);
                this.V8X8[k].SetMatrix(matrixV);
            }

            for (int k = 0; k < this.Y8X8.Count; k++)
            {
                double[,] matDCTY = new double[8, 8];
                double[,] matDCTU = new double[8, 8];
                double[,] matDCTV = new double[8, 8];

                for (int i = 0; i < 8; i++)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        double sumY = 0, sumU = 0, sumV = 0;
                        for (int x = 0; x < 8; x++)
                        {
                            for (int y = 0; y < 8; y++)
                            {
                                sumY += this.Y8X8[k].GetMatrix()[x, y] *
                                    Math.Cos(((2 * x + 1) * i * Math.PI) / 16) *
                                    Math.Cos(((2 * y + 1) * j * Math.PI) / 16);
                                sumU += this.U8X8[k].GetMatrix()[x, y] *
                                    Math.Cos(((2 * x + 1) * i * Math.PI) / 16) *
                                    Math.Cos(((2 * y + 1) * j * Math.PI) / 16);
                                sumV += this.V8X8[k].GetMatrix()[x, y] *
                                    Math.Cos(((2 * x + 1) * i * Math.PI) / 16) *
                                    Math.Cos(((2 * y + 1) * j * Math.PI) / 16);
                            }
                        }
                        if (i.Equals(0))
                        {
                            matDCTY[i, j] = Math.Ceiling(0.125 * sumY);
                            matDCTU[i, j] = Math.Ceiling(0.125 * sumU);
                            matDCTV[i, j] = Math.Ceiling(0.125 * sumV);
                        }
                        else
                        {
                            matDCTY[i, j] = Math.Ceiling(0.25 * sumY);
                            matDCTU[i, j] = Math.Ceiling(0.25 * sumU);
                            matDCTV[i, j] = Math.Ceiling(0.25 * sumV);
                        }
                    }
                }
                this.DCTY.Add(new Matrix<double>(this.Y8X8[k].GetRowIndex(), this.Y8X8[k].GetColIndex(), matDCTY, "Y", 8));
                this.DCTU.Add(new Matrix<double>(this.U8X8[k].GetRowIndex(), this.Y8X8[k].GetColIndex(), matDCTU, "U", 8));
                this.DCTV.Add(new Matrix<double>(this.V8X8[k].GetRowIndex(), this.Y8X8[k].GetColIndex(), matDCTV, "V", 8));
            }
        }

        private void Quantization()
        {
            for (int k = 0; k < this.DCTY.Count; k++)
            {
                int[,] QDTCy = new int[8, 8];
                int[,] QDTCu = new int[8, 8];
                int[,] QDTCv = new int[8, 8];

                for (int i = 0; i < 8; i++)
                    for (int j = 0; j < 8; j++)
                    {
                        QDTCy[i, j] = Convert.ToInt32(this.DCTY[k].GetMatrix()[i, j] / this.quantizer[i, j]);
                        QDTCu[i, j] = Convert.ToInt32(this.DCTU[k].GetMatrix()[i, j] / this.quantizer[i, j]);
                        QDTCv[i, j] = Convert.ToInt32(this.DCTV[k].GetMatrix()[i, j] / this.quantizer[i, j]);
                    }
                this.QDCTY.Add(new Matrix<int>(this.DCTY[k].GetRowIndex(), this.DCTY[k].GetColIndex(), QDTCy, "Y", 8));
                this.QDCTU.Add(new Matrix<int>(this.DCTU[k].GetRowIndex(), this.DCTU[k].GetColIndex(), QDTCu, "U", 8));
                this.QDCTV.Add(new Matrix<int>(this.DCTV[k].GetRowIndex(), this.DCTV[k].GetColIndex(), QDTCv, "V", 8));
            }
        }

        private int GetAmplitude(int value)
        {
            if (value == 1 || value == -1)
                return 1;
            foreach (var elem in this.amplitudes)
                if (!elem.Key.Equals(1))
                    if ((elem.Value[0] <= value && value <= elem.Value[1]) ||
                        (elem.Value[2] <= value && value <= elem.Value[3]))
                        return elem.Key;
            return 0;
        }

        private void Entropy()
        {
            for (int i = 0; i < this.QDCTY.Count; i++)
            {
                AddEntropy(QDCTY[i].GetMatrix());
                AddEntropy(QDCTU[i].GetMatrix());
                AddEntropy(QDCTV[i].GetMatrix());
            }
        }

        private void AddEntropy(int[,] matrix)
        {
            int[] result = ZicZac(matrix);
            this.entropy.AddRange(new List<int>{ GetAmplitude(result[0]), result[0] });

            for (int i = 1; i < 64; i++)
            {
                int count = 0;
                while (result[i].Equals(0))
                {
                    count++;
                    i++;
                    if (i.Equals(64))
                        break;
                }
                if (i.Equals(64))
                    this.entropy.AddRange(new List<int> { 0, 0 });
                else
                    this.entropy.AddRange(new List<int> { count, GetAmplitude(result[i]), result[i] });
            }

        }

        private int[] ZicZac(int[,] matrix)
        {
            int m = 8;
            int n = 8;
            int index = 0;
            int[] result = new int[m * n];
            int i, j;

            for (int sum = 0; sum <= m + n - 2; sum++)
            {
                if (sum % 2 == 0)
                {
                    i = Math.Min(sum, m - 1);
                    j = sum - i;
                    while (i >= 0 && j < n)
                    {
                        result[index++] = matrix[i--, j++];
                    }
                }
                else
                {
                    j = Math.Min(sum, n - 1);
                    i = sum - j;
                    while (j >= 0 && i < m)
                    {
                        result[index++] = matrix[i++, j--];
                    }
                }
            }
            return result;
        }


        // DECODER 

        private int[] MakeVector()
        {
            int[] vec = new int[64];
            int index = 0;
            vec[index] = this.entropy[pos + 1];
            index++;
            pos += 2;

            while (!this.entropy[pos].Equals(0) || !this.entropy[pos + 1].Equals(0))
            {
                int zeros = this.entropy[pos];
                while (zeros != 0)
                {
                    vec[index] = 0;
                    zeros--;
                    index++;
                }
                vec[index] = this.entropy[pos + 2];
                index++;
                pos += 3;
            }
            pos += 2;

            return vec;
        }

        private int[,] MakeMatrix (int[] vec)
        {
            int index = 0;
            int m = 8;
            int n = 8;
            int i, j;
            int[,] matrix = new int[8, 8];

            for (int sum = 0; sum <= m + n - 2; sum++)
            {
                if (sum % 2 == 0)
                {
                    i = Math.Min(sum, m - 1);
                    j = sum - i;
                    while (i >= 0 && j < n)
                    {
                        matrix[i--, j++] = vec[index++];
                    }
                }
                else
                {
                    j = Math.Min(sum, n - 1);
                    i = sum - j;
                    while (j >= 0 && i < m)
                    {
                        matrix[i++, j--] = vec[index++];
                    }
                }
            }
            return matrix;
        }
 
        public void DecodeEntropy()
        {
            int index = 0;
            while (pos < this.entropy.Count)
            {
                this.QDCTY[index].SetMatrix(MakeMatrix(MakeVector()));
                this.QDCTU[index].SetMatrix(MakeMatrix(MakeVector()));
                this.QDCTV[index].SetMatrix(MakeMatrix(MakeVector()));
                index++;
            }
        }

        public void DeQuantization()
        {
            for (int k = 0; k < this.QDCTY.Count; k++)
            {
                double[,] DTCy = new double[8, 8];
                double[,] DTCu = new double[8, 8];
                double[,] DTCv = new double[8, 8];

                for (int i = 0; i < 8; i++)
                    for (int j = 0; j < 8; j++)
                    {
                        DTCy[i, j] = Convert.ToDouble(this.QDCTY[k].GetMatrix()[i, j] * this.quantizer[i, j]);
                        DTCu[i, j] = Convert.ToDouble(this.QDCTU[k].GetMatrix()[i, j] * this.quantizer[i, j]);
                        DTCv[i, j] = Convert.ToDouble(this.QDCTV[k].GetMatrix()[i, j] * this.quantizer[i, j]);
                    }
                this.DCTY[k].SetMatrix(DTCy);
                this.DCTU[k].SetMatrix(DTCu);
                this.DCTV[k].SetMatrix(DTCv);
            }
        }

        public void ConvertDCTToYUV()
        {
            for (int k = 0; k < this.DCTY.Count; k++)
            {
                double[,] matY = new double[8, 8];
                double[,] matU = new double[8, 8];
                double[,] matV = new double[8, 8];

                for (int i = 0; i < 8; i++)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        double sumY = 0, sumU = 0, sumV = 0;
                        for (int x = 0; x < 8; x++)
                        {
                            for (int y = 0; y < 8; y++)
                            {
                                if (x.Equals(0))
                                {
                                    sumY += 0.5*this.DCTY[k].GetMatrix()[x, y] *
                                        Math.Cos(((2 * i + 1) * x * Math.PI) / 16) *
                                        Math.Cos(((2 * j + 1) * y * Math.PI) / 16);
                                    sumU += 0.5*this.DCTU[k].GetMatrix()[x, y] *
                                        Math.Cos(((2 * i + 1) * x * Math.PI) / 16) *
                                        Math.Cos(((2 * j + 1) * y * Math.PI) / 16);
                                    sumV += 0.5*this.DCTV[k].GetMatrix()[x, y] *
                                        Math.Cos(((2 * i + 1) * x * Math.PI) / 16) *
                                        Math.Cos(((2 * j + 1) * y * Math.PI) / 16);
                                }
                                else
                                {
                                    sumY += this.DCTY[k].GetMatrix()[x, y] *
                                        Math.Cos(((2 * i + 1) * x * Math.PI) / 16) *
                                        Math.Cos(((2 * j + 1) * y * Math.PI) / 16);
                                    sumU += this.DCTU[k].GetMatrix()[x, y] *
                                        Math.Cos(((2 * i + 1) * x * Math.PI) / 16) *
                                        Math.Cos(((2 * j + 1) * y * Math.PI) / 16);
                                    sumV += this.DCTV[k].GetMatrix()[x, y] *
                                        Math.Cos(((2 * i + 1) * x * Math.PI) / 16) *
                                        Math.Cos(((2 * j + 1) * y * Math.PI) / 16);
                                }
                            }
                        }

                        matY[i, j] = 0.25 * sumY + 128.0;
                        matU[i, j] = 0.25 * sumU + 128.0;
                        matV[i, j] = 0.25 * sumV + 128.0;
                    }
                }
                this.Y8X8[k].SetMatrix(matY);
                this.U8X8[k].SetMatrix(matU);
                this.V8X8[k].SetMatrix(matV);
            }
        }

        public void ConvertBackToYUV()
        {
            for (int k = 0; k < this.Y8X8.Count; k++)
            {
                int row = 0, col = 0;
                for (int i = this.Y8X8[k].GetRowIndex(); i < this.Y8X8[k].GetRowIndex() + 8; i++)
                {
                    for (int j = this.Y8X8[k].GetColIndex(); j < this.Y8X8[k].GetColIndex() + 8; j++)
                    {
                        this.Y[i, j] = this.Y8X8[k].GetMatrix()[row, col];
                        this.U[i, j] = this.U8X8[k].GetMatrix()[row, col];
                        this.V[i, j] = this.V8X8[k].GetMatrix()[row, col];
                        col++;
                    }
                    row++;
                    col = 0;
                }
            }
        }

        public int Red(int row,int col)
        {
            return Convert.ToInt32(1.164 * (this.Y[row, col] - 16) + 1.596 * (this.V[row, col] - 128)); 
        }

        public int Blue(int row, int col)
        {
            return Convert.ToInt32(1.164 * (this.Y[row, col] - 16) + 2.018 * (this.U[row, col] - 128));
        }

        public int Green(int row, int col)
        {
            return Convert.ToInt32(1.164 * (this.Y[row, col] - 16) - 0.813 * (this.V[row, col] - 128) - 0.391 * (this.U[row, col] - 128));
        }

        public void DisplayY()
        {
            foreach (Matrix<double> mat in this.Y8X8)
                Console.WriteLine(mat.ToString());
        }

        public void DisplayU()
        {
            foreach(Matrix<double> mat in this.U4X4)
                Console.WriteLine(mat.ToString());
        }

        public void DisplayV()
        {
            foreach (Matrix<double> mat in this.V4X4)
                Console.WriteLine(mat.ToString());
        }

        public void DisplayEntropy()
        {

            for (int i = 0; i < this.entropy.Count; i = i + 2)
            {
                Console.WriteLine(entropy[i] + " " + entropy[i+1]);
                i += 2;
                while (!this.entropy[i].Equals(0) || !this.entropy[i + 1].Equals(0))
                {
                    Console.WriteLine(entropy[i] + " " + entropy[i + 1] + " " + entropy[i + 2]);
                    i += 3;
                }
                Console.WriteLine(entropy[i] + " " + entropy[i + 1]);
            }
        }
    }
}
