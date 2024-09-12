using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

public unsafe struct BitVector
{
    private const int MaxStackAllocSize = 256; // 2048 bits
    private const int BitsPerLong = sizeof(ulong) * 8;

    private readonly int _length;
    private readonly int _longCount;
    private fixed ulong _stackBits[MaxStackAllocSize / sizeof(ulong)];
    private ulong* _heapBits;

    public int Length => _length;

    public BitVector(int length)
    {
        if (length <= 0)
            throw new ArgumentOutOfRangeException(nameof(length), "Length must be positive.");

        _length = length;
        _longCount = (length + BitsPerLong - 1) / BitsPerLong;
        _heapBits = null;

        if (_longCount > MaxStackAllocSize / sizeof(ulong))
        {
            _heapBits = (ulong*)Marshal.AllocHGlobal(_longCount * sizeof(ulong));
        }
    }

    public void Dispose()
    {
        if (_heapBits != null)
        {
            Marshal.FreeHGlobal((IntPtr)_heapBits);
            _heapBits = null;
        }
    }

    private ulong* GetBits()
    {
        if (_heapBits != null)
            return _heapBits;
        
        fixed (ulong* stackBits = _stackBits)
        {
            return stackBits;
        }
    }

    public bool this[int index]
    {
        get
        {
            if ((uint)index >= (uint)_length)
                throw new ArgumentOutOfRangeException(nameof(index));

            ulong* bits = GetBits();
            return (bits[index / BitsPerLong] & (1UL << (index % BitsPerLong))) != 0;
        }
        set
        {
            if ((uint)index >= (uint)_length)
                throw new ArgumentOutOfRangeException(nameof(index));

            ulong* bits = GetBits();
            if (value)
                bits[index / BitsPerLong] |= 1UL << (index % BitsPerLong);
            else
                bits[index / BitsPerLong] &= ~(1UL << (index % BitsPerLong));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Set(int index, bool value) => this[index] = value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Get(int index) => this[index];

    public void SetAll(bool value)
    {
        ulong fill = value ? ulong.MaxValue : 0;
        ulong* bits = GetBits();
        for (int i = 0; i < _longCount; i++)
            bits[i] = fill;
    }

    public int PopCount()
    {
        int count = 0;
        ulong* bits = GetBits();
        for (int i = 0; i < _longCount; i++)
            count += PopCount(bits[i]);
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
        ulong* leftBits = left.GetBits();
        ulong* rightBits = right.GetBits();
        ulong* resultBits = result.GetBits();

        for (int i = 0; i < left._longCount; i++)
            resultBits[i] = leftBits[i] & rightBits[i];

        return result;
    }

    public static BitVector operator |(BitVector left, BitVector right)
    {
        if (left._length != right._length)
            throw new ArgumentException("Vectors must have the same length.");

        var result = new BitVector(left._length);
        ulong* leftBits = left.GetBits();
        ulong* rightBits = right.GetBits();
        ulong* resultBits = result.GetBits();

        for (int i = 0; i < left._longCount; i++)
            resultBits[i] = leftBits[i] | rightBits[i];

        return result;
    }

    public static BitVector operator ^(BitVector left, BitVector right)
    {
        if (left._length != right._length)
            throw new ArgumentException("Vectors must have the same length.");

        var result = new BitVector(left._length);
        ulong* leftBits = left.GetBits();
        ulong* rightBits = right.GetBits();
        ulong* resultBits = result.GetBits();

        for (int i = 0; i < left._longCount; i++)
            resultBits[i] = leftBits[i] ^ rightBits[i];

        return result;
    }

    public static BitVector operator ~(BitVector vector)
    {
        var result = new BitVector(vector._length);
        ulong* vectorBits = vector.GetBits();
        ulong* resultBits = result.GetBits();

        for (int i = 0; i < vector._longCount; i++)
            resultBits[i] = ~vectorBits[i];

        return result;
    }
    
    public override string ToString()
    {
        char[] result = new char[_length];
        ulong* bits = GetBits();

        for (int i = 0; i < _length; i++)
        {
            int longIndex = i / BitsPerLong;
            int bitIndex = i % BitsPerLong;
            bool isSet = (bits[longIndex] & (1UL << bitIndex)) != 0;
            result[i] = isSet ? 'X' : '_';
        }

        return new string(result);
    }
}
