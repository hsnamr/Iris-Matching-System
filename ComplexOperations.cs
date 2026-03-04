using System;

namespace DemoApp;

/// <summary>Pixelwise matrix-to-matrix operations and complex matching.</summary>
static class ComplexOperations
{
    public static Complex[,] Addition(Complex[,] a, Complex[,] b)
    {
        ValidateSameDimensions(a, b);
        int rows = a.GetLength(0), cols = a.GetLength(1);
        var c = new Complex[rows, cols];
        for (int i = 0; i < rows; i++)
            for (int j = 0; j < cols; j++)
                c[i, j] = a[i, j] + b[i, j];
        return c;
    }

    public static Complex[,] Subtraction(Complex[,] a, Complex[,] b)
    {
        ValidateSameDimensions(a, b);
        int rows = a.GetLength(0), cols = a.GetLength(1);
        var c = new Complex[rows, cols];
        for (int i = 0; i < rows; i++)
            for (int j = 0; j < cols; j++)
                c[i, j] = a[i, j] - b[i, j];
        return c;
    }

    public static Complex[,] Multiplication(Complex[,] a, Complex[,] b)
    {
        ValidateSameDimensions(a, b);
        int rows = a.GetLength(0), cols = a.GetLength(1);
        var c = new Complex[rows, cols];
        for (int i = 0; i < rows; i++)
            for (int j = 0; j < cols; j++)
                c[i, j] = a[i, j] * b[i, j];
        return c;
    }

    public static Complex[,] Division(Complex[,] a, Complex[,] b)
    {
        ValidateSameDimensions(a, b);
        int rows = a.GetLength(0), cols = a.GetLength(1);
        var c = new Complex[rows, cols];
        for (int i = 0; i < rows; i++)
            for (int j = 0; j < cols; j++)
                c[i, j] = a[i, j] / b[i, j];
        return c;
    }

    public static Complex Conjugate(Complex a) => new(a.Re, -a.Im);

    public static Complex[,] Conjugate(Complex[,] b)
    {
        int rows = b.GetLength(0), cols = b.GetLength(1);
        for (int i = 0; i < rows; i++)
            for (int j = 0; j < cols; j++)
                b[i, j] = Conjugate(b[i, j]);
        return b;
    }

    public static Complex[,] Magnitude(Complex[,] b)
    {
        int rows = b.GetLength(0), cols = b.GetLength(1);
        for (int i = 0; i < rows; i++)
            for (int j = 0; j < cols; j++)
                b[i, j] = new Complex(b[i, j].Magnitude, 0);
        return b;
    }

    /// <summary>Performs (A×B*)/|A×B*| phase correlation matching.</summary>
    public static Complex[,] Match(Complex[,] a, Complex[,] b)
    {
        ValidateSameDimensions(a, b);
        int rows = b.GetLength(0), cols = b.GetLength(1);

        var bCopy = (Complex[,])b.Clone();
        var bConj = Conjugate(bCopy);
        var c = Multiplication(a, bConj);
        var cCopy = (Complex[,])c.Clone();
        var d = Magnitude(cCopy);
        return Division(c, d);
    }

    static void ValidateSameDimensions(Complex[,] a, Complex[,] b)
    {
        if (a.GetLength(0) != b.GetLength(0) || a.GetLength(1) != b.GetLength(1))
            throw new ArgumentException("Matrices must have the same dimensions.", nameof(b));
    }
}
