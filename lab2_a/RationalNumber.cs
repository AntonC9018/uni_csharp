using System;

namespace Laborator2;

using System.Diagnostics;
using INT = Int64;

public struct RationalNumber
{
    public INT Sign;
    public INT Top;
    public INT Bottom;

    public static RationalNumber FromDouble(double a, double precision)
    {
        double wholePart = Math.Truncate(a);
        double decimalPart = (a - wholePart) * precision;
        INT top = (INT) (wholePart * precision + decimalPart);
        INT bottom = (INT) precision;
        return new RationalNumber(top, bottom);
    }

    private static INT _GetSign(INT a)
    {
        return a < 0 ? -1 : 1;
    }

    public RationalNumber(INT a)
    {
        Top = Math.Abs(a);
        Sign = _GetSign(a);
        Bottom = 1;
    }

    public RationalNumber(INT a, INT b)
    {
        this = new RationalNumber(
            Math.Abs(a),
            Math.Abs(b),
            _GetSign(a) * _GetSign(b));
    }

    public RationalNumber(INT a, INT b, INT sign)
    {
        Debug.Assert(a >= 0);
        Debug.Assert(b > 0, "Constructing infinities or invalid numbers is not allowed");
        Debug.Assert(sign == 1 || sign == -1);
        var gcd = Helper.GCD(a, b);
        Top = a / gcd;
        Bottom = b / gcd;
        Sign = sign;
    }

    public static readonly RationalNumber One = new RationalNumber(1);
    public static readonly RationalNumber Zero = new RationalNumber(0);
    public static RationalNumber Invalid => default;
    public static RationalNumber PositiveInfinity => new()
    {
        Sign = 0,
        Top = 1,
        Bottom = 0,
    };
    public static RationalNumber NegativeInfinity => new()
    {
        Sign = 1,
        Top = 1,
        Bottom = 0,
    };

    public static implicit operator RationalNumber(INT i) => new RationalNumber(i);
    public static RationalNumber operator +(RationalNumber a, RationalNumber b)
    {
        INT top = a.Sign * a.Top * b.Bottom + b.Sign * b.Top * a.Bottom;
        INT bottom = a.Bottom * b.Bottom;
        return new RationalNumber(top, bottom);
    }
    public static RationalNumber operator -(RationalNumber a, RationalNumber b)
    {
        return a + -b;
    }
    public static RationalNumber operator *(RationalNumber a, RationalNumber b)
    {
        INT top = a.Top * b.Top;
        INT bottom = a.Bottom * b.Bottom;
        return new RationalNumber(top, bottom, a.Sign * b.Sign);
    }
    public static RationalNumber operator /(RationalNumber a, RationalNumber b)
    {
        INT top = a.Top * b.Bottom;
        INT bottom = a.Bottom * b.Top;
        return new RationalNumber(top, bottom, a.Sign * b.Sign);
    }
    public static RationalNumber operator -(RationalNumber a)
    {
        a.Sign = -a.Sign;
        return a;
    }
    public readonly override string ToString()
    {
        string signString = Sign == 1 ? "" : "-";
        if (Top == 0)
        {
            if (Bottom != 0)
                return "0";
            else
                return signString + "inf";
        }
        else
        {
            if (Bottom == 1)
                return signString + Top.ToString();
            else
                return signString + string.Format("{0}/{1}", Top, Bottom);
        }
    }

    public static bool operator==(RationalNumber a, RationalNumber b)
    {
        // infinity
        if (a.Bottom == 0 && a.Bottom == 0)
            return a.Sign == b.Sign;
        // zero
        if (a.Top == 0 && b.Top == 0)
            return true;
        return a.Top == b.Top
            && a.Bottom == b.Bottom
            && b.Sign == a.Sign;
    }
    public static bool operator!=(RationalNumber a, RationalNumber b) => !(a == b);
    public readonly override bool Equals(object? a)
    {
        if (a is not RationalNumber b)
            return false;
        return this == b;
    }
    public readonly override int GetHashCode()
    {
        return base.GetHashCode();
    }
}
