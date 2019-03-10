using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab1.Model
{
    class Matrix<T>
    {
        private T[,] matrix;
        private int rowindex;
        private int colindex;
        private string type;
        private int size;

        public Matrix(int row, int col, T[,] matrix, string type, int size)
        {
            this.colindex = col;
            this.rowindex = row;
            this.type = type;
            this.matrix = matrix;
            this.size = size;
        }

        public T[,] GetMatrix() { return this.matrix; }
        public int GetSize() { return this.size; }
        public int GetRowIndex() { return this.rowindex; }
        public int GetColIndex() { return this.colindex; }
        public void SetMatrix(T[,] matrix) { this.matrix = matrix; }

        public override string ToString()
        {
            string str = "";
            str += "position in mat: " + this.rowindex + " " + this.colindex + "\n";
            str += "Type: " + this.type + "\n";

            for(int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                    str += this.matrix[i, j] + " ";
                str += "\n";
            }

            return str;
        }
    }
}
