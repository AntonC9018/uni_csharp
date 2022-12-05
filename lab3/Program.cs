/*
    Tema: Clase și obiecte

    De cercetat funcționalitatea clasei String.
    De realizat clasa Mystring similară ca funcţionalitate cu clasa String.
    Clasa Mystring va conţine 3-5 constructori, funcţie-indexator, proprietăţi, operatori, 10-15 metode.
    Pentru realizarea clasei Mystring nu se recurge la clasa String.
    De elaborat o aplicație ce permite exemplificarea funcționalității clasei Mystring.
*/

using System;
using System.Collections.Generic;

namespace Laborator3;

public class MyString
{
    private char[] _string;

    public MyString(char[] str)
    {
        _string = str;
    }

    public MyString(ReadOnlySpan<char> str)
    {
        _string = new char[str.Length];
        for (int i = 0; i < str.Length; i++)
            _string[i] = str[i];
    }

    public MyString(MyString str)
    {
        _string = str._string;
    }

    public char this[int index]
    {
        get
        {
            return _string[index];
        }
        set
        {
            _string[index] = value;
        }
    }

    public int Length
    {
        get
        {
            return _string.Length;
        }
    }

    public static implicit operator ReadOnlySpan<char>(MyString str)
    {
        return str._string;
    }

    public static MyString operator +(MyString string1, ReadOnlySpan<char> string2)
    {
        char[] newString = new char[string1.Length + string2.Length];
        for (int i = 0; i < string1.Length; i++)
            newString[i] = string1[i];
        for (int i = 0; i < string2.Length; i++)
            newString[string1.Length + i] = string2[i];
        return new MyString(newString);
    }

    public static MyString operator +(MyString string1, char ch)
    {
        char[] newString = new char[string1.Length + 1];
        for (int i = 0; i < string1.Length; i++)
            newString[i] = string1[i];
        newString[string1.Length] = (char)ch;
        return new MyString(newString);
    }

    public int IndexOf(char ch, int startIndex = 0)
        => IndexOf(ch, startIndex, _string.Length - startIndex);

    public int IndexOf(char ch, int startIndex, int count)
    {
        for (int i = startIndex; i < startIndex + count; i++)
            if (_string[i] == ch)
                return i;
        return -1;
    
    }

    public int IndexOf(ReadOnlySpan<char> str, int startIndex, int length)
    {
        for (int i = startIndex; i < startIndex + length; i++)
        {
            bool found = true;
            for (int j = 0; j < str.Length; j++)
            {
                if (_string[i + j] != str[j])
                {
                    found = false;
                    break;
                }
            }
            if (found)
                return i;
        }
        return -1;
    }

    public int IndexOf(ReadOnlySpan<char> str, int startIndex = 0)
        => IndexOf(str, startIndex, _string.Length - startIndex);


    public MyString Substring(int startIndex)
        => Substring(startIndex, _string.Length - startIndex);

    public MyString Substring(int startIndex, int length)
    {
        char[] newString = new char[length];
        for (int i = startIndex; i < startIndex + length; i++)
            newString[i - startIndex] = _string[i];
        return new MyString(newString);
    }

    private int GetLeftTrimIndex()
    {
        int startIndex = 0;
        while (_string[startIndex] == ' ')
            startIndex++;
        return startIndex;
    }

    private int GetRightTrimIndex()
    {
        int endIndex = _string.Length - 1;
        while (_string[endIndex] == ' ')
            endIndex--;
        return endIndex;
    }

    public MyString TrimLeft()
        => Substring(GetLeftTrimIndex());

    public MyString TrimRight()
        => Substring(0, GetRightTrimIndex() + 1);


    public MyString Trim()
    {
        int left = GetLeftTrimIndex();
        int right = GetRightTrimIndex();
        return Substring(left, right - left + 1);
    }

    public MyString Remove(ReadOnlySpan<char> part)
    {
        int indexStart = IndexOf(part);
        if (indexStart == -1)
            return this;
        var newString = new char[_string.Length - part.Length];
        for (int i = 0; i < indexStart; i++)
            newString[i] = _string[i];
        for (int i = indexStart + part.Length; i < _string.Length; i++)
            newString[i - part.Length] = _string[i];
        return new MyString(newString);
    }

    public MyString Replace(ReadOnlySpan<char> part, ReadOnlySpan<char> newPart)
    {
        int indexStart = IndexOf(part);
        if (indexStart == -1)
            return this;
        var newString = new char[_string.Length - part.Length + newPart.Length];
        for (int i = 0; i < indexStart; i++)
            newString[i] = _string[i];
        for (int i = 0; i < newPart.Length; i++)
            newString[indexStart + i] = newPart[i];
        for (int i = indexStart + part.Length; i < _string.Length; i++)
            newString[i - part.Length + newPart.Length] = _string[i];
        return new MyString(newString);
    }

    public MyString ReplaceAll(ReadOnlySpan<char> part, ReadOnlySpan<char> newPart)
    {
        int indexStart = IndexOf(part);
        if (indexStart == -1)
            return this;
        int endIndex = 0;
        List<char> t = new();

        do
        {
            for (int i = endIndex; i < indexStart; i++)
                t.Add(_string[i]);
            for (int i = 0; i < newPart.Length; i++)
                t.Add(newPart[i]);
            endIndex = indexStart + part.Length;
            indexStart = IndexOf(part, endIndex);
        }
        while (indexStart != -1);

        for (int i = endIndex; i < _string.Length; i++)
            t.Add(_string[i]);
        
        return new MyString(t.ToArray());
    }

    public char[] GetUnderlyingArray()
    {
        return _string;
    }

    public char[] ToCharArray()
    {
        char[] newString = new char[_string.Length];
        for (int i = 0; i < _string.Length; i++)
            newString[i] = _string[i];
        return newString;
    }

    public MyString Concatenate(ReadOnlySpan<char> str)
    {
        return this + str;
    }

    public MyString Concatenate(char ch)
    {
        return this + ch;
    }

    public int LastIndexOf(char ch)
    {
        for (int i = _string.Length - 1; i >= 0; i--)
            if (_string[i] == ch)
                return i;
        return -1;
    }

    public int LastIndexOf(ReadOnlySpan<char> str)
    {
        for (int i = _string.Length - 1; i >= 0; i--)
        {
            if (_string[i] == str[0])
            {
                bool found = true;
                for (int j = 1; j < str.Length; j++)
                {
                    if (_string[i + j] != str[j])
                    {
                        found = false;
                        break;
                    }
                }
                if (found)
                    return i;;
            }
        }
        return -1;
    }

    public override string ToString()
    {
        return new string(_string);
    }
}

class Program
{
    static void Main()
    {
        // Using constructors
        MyString str1 = new MyString("Hello");
        MyString str2 = new MyString(new char[] { 'H', 'e', 'l', 'l', 'o' });
        MyString str3 = new MyString(str1);

        // Writing to the console
        Console.WriteLine(str1);
        Console.WriteLine(str2);
        Console.WriteLine(str3);

        // Getting the underlying char array
        {
            Console.WriteLine("Before: " + str1);
            char[] array = str1.GetUnderlyingArray();
            array[0] = 'J';
            Console.WriteLine("After: " + str1);
        }

        // Getting the copy char array
        {
            Console.WriteLine("Before: " + str1);
            char[] array = str1.ToCharArray();
            array[0] = 'K';
            Console.WriteLine("After (remains with J): " + str1);
        }

        // Finding index of char
        {
            Console.WriteLine("Index of e: " + str1.IndexOf('e'));
            Console.WriteLine("Index of z: " + str1.IndexOf('z'));
        }

        // Finding index with offset
        {
            Console.WriteLine("Index of e (starting from index 2): " + str1.IndexOf('e', 2));
            Console.WriteLine("Index of z (starting from index 2): " + str1.IndexOf('z', 2));
        }

        // Finding index with max length
        {
            Console.WriteLine("Index of e (starting from index 2, max length 2): " + str1.IndexOf('e', 2, 2));
            Console.WriteLine("Index of z (starting from index 2, max length 2): " + str1.IndexOf('z', 2, 2));
        }

        // Finding index of string
        {
            Console.WriteLine("Index of ll: " + str1.IndexOf("ll"));
            Console.WriteLine("Index of zz: " + str1.IndexOf("zz"));
        }

        // Finding last index of char
        {
            Console.WriteLine("Last index of l: " + str1.LastIndexOf('l'));
            Console.WriteLine("Last index of z: " + str1.LastIndexOf('z'));
        }

        // Finding last index of string
        {
            Console.WriteLine("Last index of ll: " + str1.LastIndexOf("ll"));
            Console.WriteLine("Last index of zz: " + str1.LastIndexOf("zz"));
        }

        // Replacing a substring
        {
            Console.WriteLine("Before: " + str1);
            str1 = str1.Replace("ll", "LL");
            Console.WriteLine("After: " + str1);
        }

        // Removing a substring
        {
            Console.WriteLine("Before: " + str1);
            str1 = str1.Remove("LL");
            Console.WriteLine("After: " + str1);
        }
        
        // Creating a substring
        {
            Console.WriteLine("Before: " + str3);
            MyString str4 = str3.Substring(1, 2);
            Console.WriteLine("After: " + str3);
            Console.WriteLine("Substring: " + str4);
        }

        // Trimming left
        {
            MyString s = new MyString("   Hello");
            Console.WriteLine("Before: " + s);
            s = s.TrimLeft();
            Console.WriteLine("After: " + s);
        }

        // Trimming right
        {
            MyString s = new MyString("Hello   ");
            Console.WriteLine("Before: " + s + "(ends here)");
            s = s.TrimRight();
            Console.WriteLine("After: " + s + "(ends here)");
        }
        
        // Trimming both sides
        {
            MyString s = new MyString("    Hello   ");
            Console.WriteLine("Before: " + s + "(ends here)");
            s = s.Trim();
            Console.WriteLine("After: " + s + "(ends here)");
        }

        // ReplaceAll demo
        {
            MyString s = new MyString("Hello");
            MyString s2 = s.ReplaceAll("l", "LL");
            Console.WriteLine(s);
            Console.WriteLine(s2);
        }

        // Concatenate and the overloaded addition operators
        {
            MyString s = new MyString("Hello");
            MyString s2 = s + " World";
            MyString s3 = s2.Concatenate('!');
            Console.WriteLine(s3);
        }
    }
}