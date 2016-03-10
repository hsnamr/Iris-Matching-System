using System;
using System.Collections.Generic;
using System.Text;

namespace DemoApp
{
    class ComplexOperations
    {
        /*****************************************************
         * Pixelwise Matrix-to-Matrix Operations
         * Addition, Subtraction, Multiplication and Division
         *****************************************************/

        public static Complex[,] Addition(Complex[,] a, Complex[,] b)
        {
            int a_Rows = a.GetLength(0);	// rows
            int a_Columns = a.GetLength(1);	// columns

            int b_Rows = b.GetLength(0);	// rows
            int b_Columns = b.GetLength(1);	// columns

            if (!(a_Rows == b_Rows) || !(a_Columns == b_Columns))
                throw new Exception("Matrices must be of same size");

            Complex[,] c = new Complex[a_Rows, a_Columns];

            for (int i = 0; i < a_Rows; i++)
                for (int j = 0; j < a_Columns; j++)
                    c[i, j] = a[i, j] + b[i, j];

            return c;
        }

        public static Complex[,] Subtraction(Complex[,] a, Complex[,] b)
        {
            int a_Rows = a.GetLength(0);	// rows
            int a_Columns = a.GetLength(1);	// columns

            int b_Rows = b.GetLength(0);	// rows
            int b_Columns = b.GetLength(1);	// columns

            if (!(a_Rows == b_Rows) || !(a_Columns == b_Columns))
                throw new Exception("Matrices must be of same size");

            Complex[,] c = new Complex[a_Rows, a_Columns];

            for (int i = 0; i < a_Rows; i++)
                for (int j = 0; j < a_Columns; j++)
                    c[i, j] = a[i, j] - b[i, j];

            return c;
        }

        public static Complex[,] Multiplication(Complex[,] a, Complex[,] b)
        {
            int a_Rows = a.GetLength(0);	// rows
            int a_Columns = a.GetLength(1);	// columns

            int b_Rows = b.GetLength(0);	// rows
            int b_Columns = b.GetLength(1);	// columns

            if (!(a_Rows == b_Rows) || !(a_Columns == b_Columns))
                throw new Exception("Matrices must be of same size");

            Complex[,] c = new Complex[a_Rows, a_Columns];

            for (int i = 0; i < a_Rows; i++)
                for (int j = 0; j < a_Columns; j++)
                    c[i, j] = a[i, j] * b[i, j];

            return c;
        }

        public static Complex[,] Division(Complex[,] a, Complex[,] b)
        {
            int a_Rows = a.GetLength(0);	// rows
            int a_Columns = a.GetLength(1);	// columns

            int b_Rows = b.GetLength(0);	// rows
            int b_Columns = b.GetLength(1);	// columns

            if (!(a_Rows == b_Rows) || !(a_Columns == b_Columns))
                throw new Exception("Matrices must be of same size");

            Complex[,] c = new Complex[a_Rows, a_Columns];

            for (int i = 0; i < a_Rows; i++)
                for (int j = 0; j < a_Columns; j++)
                    c[i, j] = a[i, j] / b[i, j];

            return c;
        }

        /*****************************************************
         * Returns the conjugate of a complex number
         *****************************************************/
        public static Complex Conjugate(Complex a)
        {
            return new Complex(a.Re, -1 * a.Im);
        }

        /*****************************************************
         * Returns the conjugate of a complex matrix
         *****************************************************/
        public static Complex[,] Conjugate(Complex[,] b)
        {
            int b_Rows = b.GetLength(0);	// rows
            int b_Columns = b.GetLength(1);	// columns
            for (int i = 0; i < b_Rows; i++)
                for (int j = 0; j < b_Columns; j++)
                {
                    b[i, j] = Conjugate(b[i, j]);
                }
            return b;
        }

        /*****************************************************
         * Returns the magnitude of a complex matric
         *****************************************************/
        public static Complex[,] Magnitude(Complex[,] b)
        {
            int b_Rows = b.GetLength(0);	// rows
            int b_Columns = b.GetLength(1);	// columns
            for (int i = 0; i < b_Rows; i++)
                for (int j = 0; j < b_Columns; j++)
                {
                    b[i, j] = new Complex(b[i, j].Magnitude, 0);
                }
            return b;
        }

        public static double testMatching(Complex[,] e_copy)
        {
            int b_Rows = e_copy.GetLength(0);	// rows
            int b_Columns = e_copy.GetLength(1);	// columns

            double min = 0;
            double max = 1;

            for (int i = 0; i < b_Rows; i++)
                for (int j = 0; j < b_Columns; j++)
                {
                    max = Math.Max(max, e_copy[i, j].Re);
                    min = Math.Min(min, e_copy[i, j].Re);
                }

            double probability = 0.0;

            for (int i = 0; i < b_Rows; i++)
                for (int j = 0; j < b_Columns; j++)
                    probability += e_copy[i, j].Re;

            probability = probability / (b_Rows * b_Columns);

            return probability;

        }

        /*****************************************************
         * Performs:
         *                 AxB*
         *          ------------------
         *            |    AxB*   |
         *****************************************************/
        public static Complex[,] Match(Complex[,] a, Complex[,] b)
        {
            int a_Rows = a.GetLength(0);	// rows
            int a_Columns = a.GetLength(1);	// columns

            int b_Rows = b.GetLength(0);	// rows
            int b_Columns = b.GetLength(1);	// columns

            Complex[,] b_copy = new Complex[b_Rows, b_Columns];

            for (int i = 0; i < b_Rows; i++)
                for (int j = 0; j < b_Columns; j++)
                    b_copy[i, j] = b[i, j];

            Complex[,] b_conj = new Complex[b_Rows, b_Columns];
            
            b_conj = Conjugate(b_copy);

            Complex[,] c = new Complex[b_Rows, b_Columns];
            c = Multiplication(a, b_conj);

            Complex[,] c_copy = new Complex[b_Rows, b_Columns];

            for (int i = 0; i < b_Rows; i++)
                for (int j = 0; j < b_Columns; j++)
                    c_copy[i, j] = c[i, j];

            Complex[,] d = new Complex[b_Rows, b_Columns];
            d = Magnitude(c_copy);

            Complex[,] e = new Complex[b_Rows, b_Columns];
            e = Division(c, d);

            /**************************************************************
            Complex[,] e_copy = new Complex[b_Rows, b_Columns];

            for (int i = 0; i < b_Rows; i++)
                for (int j = 0; j < b_Columns; j++)
                    e_copy[i, j] = e[i, j];

            double probability = testMatching(e_copy); 
            ***************************************************************/

            return e;
        }
    }
}
