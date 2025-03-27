using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Panthea.Utils
{
    public static class BitArrayUtils
    {
        public static bool AllTrue(this BitArray bitArray)
        {
            if (bitArray == null)
            {
                throw new ArgumentNullException(nameof(bitArray));
            }

            foreach (bool bit in bitArray)
            {
                if (!bit)
                {
                    return false;
                }
            }

            return true;
        }

        public static int TotalTrue(this BitArray bitArray)
        {
            if (bitArray == null) throw new ArgumentNullException(nameof(bitArray));
            int number = 0;
            foreach (bool bit in bitArray)
            {
                if (bit) number++;
            }

            return number;
        }
        
        public static int TotalFalse(this BitArray bitArray)
        {
            if (bitArray == null) throw new ArgumentNullException(nameof(bitArray));
            int number = 0;
            foreach (bool bit in bitArray)
            {
                if (!bit) number++;
            }

            return number;
        }
        
        public static BitArray ResizeBitArray(this BitArray bitArray, int count)
        {
            if (bitArray == null)
            {
                throw new ArgumentNullException(nameof(bitArray), "BitArray cannot be null.");
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), "Count cannot be negative.");
            }

            int currentLength = bitArray.Length;

            if (currentLength == count)
            {
                return bitArray; // 如果长度相同，直接返回原 BitArray
            }

            BitArray newBitArray = new BitArray(count);

            // 复制现有位到新 BitArray
            int copyLength = Mathf.Min(currentLength, count);
            for (int i = 0; i < copyLength; i++)
            {
                newBitArray[i] = bitArray[i];
            }

            // 如果扩容，新位默认为 false
            if (count > currentLength)
            {
                for (int i = currentLength; i < count; i++)
                {
                    newBitArray[i] = false;
                }
            }

            return newBitArray;
        }
        
        
        public static byte[] ToByteArray(this BitArray bits)
        {
            const int BYTE = 8;
            int length = bits.Count / BYTE + ( bits.Count % BYTE == 0 ? 0 : 1 );
            var bytes  = new byte[ length ];

            for ( int i = 0; i < bits.Length; i++ ) {

                int bitIndex  = i % BYTE;
                int byteIndex = i / BYTE;

                int mask = (bits[ i ] ? 1 : 0) << bitIndex;
                bytes[ byteIndex ] |= (byte)mask;
            }
            return bytes;
        }
        
        public static int[] ToIntArray(this BitArray bits)
        {
            const int INT_BITS = 32; // int 是 32 位
            int length = bits.Length / INT_BITS + (bits.Length % INT_BITS == 0 ? 0 : 1);
            var intArray = new int[length];

            for (int i = 0; i < bits.Length; i++)
            {
                int bitIndex = i % INT_BITS; // 当前 bit 在 int 中的位置
                int intIndex = i / INT_BITS; // 当前 bit 属于哪个 int

                int mask = (bits[i] ? 1 : 0) << bitIndex; // 将 bit 转换为掩码
                intArray[intIndex] |= mask; // 将掩码应用到对应的 int
            }

            return intArray;
        }
        
        public static BitArray ToBitArray(byte[] bytes)
        {
            int length = bytes.Length * 8;
            BitArray bits = new BitArray(length);

            for (int i = 0; i < bytes.Length; i++)
            {
                byte currentByte = bytes[i];
                for (int j = 0; j < 8; j++)
                {
                    int bitIndex = (i * 8) + j;
                    bool bitValue = (currentByte & (1 << j)) != 0;
                    bits[bitIndex] = bitValue;
                }
            }

            return bits;
        }
        
        public static BitArray ToBitArray(int[] intArray)
        {
            if (intArray == null) return null;
            int length = intArray.Length * 32;
            BitArray bits = new BitArray(length);
            for (int i = 0; i < intArray.Length; i++)
            {
                int currentInt = intArray[i];
                for (int j = 0; j < 32; j++)
                {
                    int bitIndex = (i * 32) + j;
                    if (bitIndex >= length)
                        break;
                    bool bitValue = (currentInt & (1 << j)) != 0;
                    bits[bitIndex] = bitValue;
                }
            }
            return bits;
        }
        
        /// <summary> 从 BitArray 中随机查找一个值为 False 的位置的索引。 </summary>
        public static int GetRandomFalseIndex(this BitArray bitArray)
        {
            using var falseIndices = ListPool<int>.Create();
            for (int i = 0; i < bitArray.Length; i++)
            {
                if (!bitArray[i])
                {
                    falseIndices.Add(i);
                }
            }

            if (falseIndices.Count == 0) return -1;

            var randomIndex = Random.Range(0, falseIndices.Count);
            return falseIndices[randomIndex];
        }
    }
}