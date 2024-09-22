using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public struct BitVector
{
    private const int BitsPerLong = sizeof(ulong) * 8;
    private readonly ulong[] _bits;
    private readonly int _length;

    public int Length => _length;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => $"Length = {_length}, Bits = {ToString()}";

    public BitVector(int length)
    {
        if (length <= 0)
            throw new ArgumentOutOfRangeException(nameof(length), "Length must be positive.");

        _length = length;
        int longCount = (length + BitsPerLong - 1) / BitsPerLong;
        _bits = new ulong[longCount];
    }

    public bool this[int index]
    {
        get
        {
            if ((uint)index >= (uint)_length)
                throw new ArgumentOutOfRangeException(nameof(index));

            return (_bits[index / BitsPerLong] & (1UL << (index % BitsPerLong))) != 0;
        }
        set
        {
            if ((uint)index >= (uint)_length)
                throw new ArgumentOutOfRangeException(nameof(index));

            if (value)
                _bits[index / BitsPerLong] |= 1UL << (index % BitsPerLong);
            else
                _bits[index / BitsPerLong] &= ~(1UL << (index % BitsPerLong));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Set(int index, bool value) => this[index] = value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Get(int index) => this[index];

    public void SetAll(bool value)
    {
        ulong fill = value ? ulong.MaxValue : 0;
        int fullLongCount = _length / BitsPerLong;
        int remainingBits = _length % BitsPerLong;

        // Fill all complete ulongs
        for (int i = 0; i < fullLongCount; i++)
            _bits[i] = fill;

        // Handle the last, potentially partial ulong
        if (remainingBits > 0)
        {
            ulong mask = (1UL << remainingBits) - 1;
            _bits[fullLongCount] = value ? mask : 0;
        }
    }

    public int PopCount()
    {
        int count = 0;
        for (int i = 0; i < _bits.Length; i++)
            count += PopCount(_bits[i]);
        return count;
    }

    private static int PopCount(ulong x)
    {
        x -= (x >> 1) & 0x5555555555555555UL;
        x = (x & 0x3333333333333333UL) + ((x >> 2) & 0x3333333333333333UL);
        x = (x + (x >> 4)) & 0x0f0f0f0f0f0f0f0fUL;
        return (int)((x * 0x0101010101010101UL) >> 56);
    }

    public static BitVector operator &(BitVector left, BitVector right)
    {
        if (left._length != right._length)
            throw new ArgumentException("Vectors must have the same length.");

        var result = new BitVector(left._length);
        for (int i = 0; i < left._bits.Length; i++)
            result._bits[i] = left._bits[i] & right._bits[i];

        return result;
    }

    public static BitVector operator |(BitVector left, BitVector right)
    {
        if (left._length != right._length)
            throw new ArgumentException("Vectors must have the same length.");

        var result = new BitVector(left._length);
        for (int i = 0; i < left._bits.Length; i++)
            result._bits[i] = left._bits[i] | right._bits[i];

        return result;
    }

    public static BitVector operator ^(BitVector left, BitVector right)
    {
        if (left._length != right._length)
            throw new ArgumentException("Vectors must have the same length.");

        var result = new BitVector(left._length);
        for (int i = 0; i < left._bits.Length; i++)
            result._bits[i] = left._bits[i] ^ right._bits[i];

        return result;
    }

    public static BitVector operator ~(BitVector vector)
    {
        var result = new BitVector(vector._length);
        for (int i = 0; i < vector._bits.Length; i++)
            result._bits[i] = ~vector._bits[i];

        return result;
    }
    
    public override string ToString()
    {
        int spacesCount = (_length - 1) / 4;
        int totalLength = _length + spacesCount;
        char[] result = new char[totalLength];

        for (int i = 0, j = totalLength - 1; i < _length; i++)
        {
            if (i > 0 && i % 4 == 0)
            {
                result[j] = '|';
                j--;
            }

            result[j] = this[i] ? 'X' : '_';
            j--;
        }

        return new string(result);
    }
    
    public string Print => ToString();
}