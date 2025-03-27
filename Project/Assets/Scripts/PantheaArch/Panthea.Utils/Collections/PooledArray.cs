using System.Buffers;

namespace Panthea.Utils
{
    public readonly ref struct PooledArray<T>
    {
        private readonly T[] _array;
        public T[] Array => _array;

        public PooledArray(int length)
        {
            _array = ArrayPool<T>.Shared.Rent(length);
            System.Array.Clear(_array, 0, length);
        }
    
        public void Dispose()
        {
            ArrayPool<T>.Shared.Return(_array);
        }
    }
}