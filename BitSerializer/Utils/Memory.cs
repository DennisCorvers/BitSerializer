using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#if UNITY
using Unity.Collections.LowLevel.Unsafe;
#endif

namespace BitSerializer.Utils
{
    internal unsafe static class Memory
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IntPtr Alloc(int size)
        {
#if UNITY
            return (IntPtr)UnsafeUtility.Malloc(size, 8, Unity.Collections.Allocator.Persistent);
#else
            return Marshal.AllocHGlobal(size);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Free(IntPtr ptr)
        {
#if UNITY
            UnsafeUtility.Free((void*)ptr, Unity.Collections.Allocator.Persistent);
#else
            Marshal.FreeHGlobal(ptr);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Free(void* ptr)
        {
#if UNITY
            UnsafeUtility.Free(ptr, Unity.Collections.Allocator.Persistent);
#else
            Marshal.FreeHGlobal((IntPtr)ptr);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IntPtr Realloc(IntPtr ptr, int size, int newSize)
        {
            Debug.Assert(newSize > size);

            // Create new buffer and copy old contents to new.
            IntPtr newBuffer = Alloc(newSize);
            CopyMemory((void*)ptr, (void*)newBuffer, size);

            // Free old buffer and return new buffer.
            Free(ptr);
            return newBuffer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ZeroMem(void* ptr, long size)
        {
#if UNITY
            UnsafeUtility.MemClear(ptr, size);
#else
            long c = size >> 3; // longs

            int i = 0;
            for (; i < c; i++)
                *((ulong*)ptr + i) = 0;

            i = i << 3;
            for (; i < size; i++)
                *((byte*)ptr + i) = 0;
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IntPtr AllocZeroed(int size)
        {
            var memory = Alloc(size);
            ZeroMem((void*)memory, size);
            return memory;
        }

        public static IntPtr ReallocZeroed(IntPtr ptr, int size, int newSize)
        {
            // Realloc existing buffer.
            var newBuffer = Realloc(ptr, size, newSize);

            // Zero newly allocated bytes after copy.
            ZeroMem(((byte*)newBuffer) + size, newSize - size);

            return newBuffer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static void CopyMemory(void* source, void* destination, int length)
        {
#if UNITY
            UnsafeUtility.MemCpy(destination, source, length);
#else
            Buffer.MemoryCopy(source, destination, length, length);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static void CopyMemory(byte[] source, int sourceIndex, IntPtr destination, int length)
        {
#if UNITY
            fixed (byte* ptr = &source[sourceIndex])
            {
                CopyMemory(ptr, (void*)destination, length);
            }
#else
            Marshal.Copy(source, sourceIndex, destination, length);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static void CopyMemory(IntPtr source, byte[] destination, int destinationIndex, int length)
        {
#if UNITY
            fixed (byte* ptr = &destination[destinationIndex])
            {
                CopyMemory((void*)source, ptr, length);
            }
#else
            Marshal.Copy(source, destination, destinationIndex, length);
#endif
        }
    }
}
