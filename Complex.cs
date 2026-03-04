// AForge Math Library
// Copyright © Andrew Kirillov, 2005-2007
// andrew.kirillov@gmail.com

namespace DemoApp;

/// <summary>
/// Complex number.
/// </summary>
/// <remarks>The struct encapsulates a complex number and provides basic complex operators.</remarks>
public readonly struct Complex
{
    /// <summary>Real part of the complex number.</summary>
    public double Re { get; }

    /// <summary>Imaginary part of the complex number.</summary>
    public double Im { get; }

    /// <summary>Represents complex zero (both real and imaginary parts equal to zero).</summary>
    public static Complex Zero => new(0, 0);

    /// <summary>Magnitude value of the complex number.</summary>
    public double Magnitude => Math.Sqrt(Re * Re + Im * Im);

    public Complex(double re, double im)
    {
        Re = re;
        Im = im;
    }

    public Complex(Complex c)
    {
        Re = c.Re;
        Im = c.Im;
    }

    public override readonly string ToString() =>
        $"{Re}{(Im < 0 ? '-' : '+')}{Math.Abs(Im)}i";

    public static Complex operator +(Complex a, Complex b) =>
        new(a.Re + b.Re, a.Im + b.Im);

    public static Complex operator -(Complex a, Complex b) =>
        new(a.Re - b.Re, a.Im - b.Im);

    public static Complex operator *(Complex a, Complex b) =>
        new(
            a.Re * b.Re - a.Im * b.Im,
            a.Re * b.Im + a.Im * b.Re);

    public static Complex operator /(Complex a, Complex b)
    {
        double divider = b.Re * b.Re + b.Im * b.Im;
        if (divider == 0)
            throw new DivideByZeroException();

        return new Complex(
            (a.Re * b.Re + a.Im * b.Im) / divider,
            (a.Im * b.Re - a.Re * b.Im) / divider);
    }
}
