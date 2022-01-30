﻿using Kernel;
using System;

abstract unsafe class Heap
{
    internal static unsafe void ZeroMemory(IntPtr data, ulong size)
    {
        Native.Stosb((void*)data, 0, size);
    }

    internal static void Free(IntPtr intPtr)
    {
        ulong p = (ulong)intPtr;
        if (p < (ulong)_Info.Start) return;
        p -= (ulong)_Info.Start;
        if ((p % PageSize) != 0) return;
        /*
         * This will get wrong if the size is larger than PageSize
         * and however the allocated address should be aligned
         */
        //p &= ~PageSize; 
        p /= PageSize;
        ulong pages = _Info.Pages[p];
        if (pages != 0 && pages != PageSignature)
        {
            for (ulong i = 0; i < pages; i++)
                _Info.Pages[p + i] = 0;
        }
    }

    public const ulong PageSignature = 0x2E61666E6166696E;

    /*
     * NumPages = Memory Size / PageSize
     * This should be a const because there will be allocations during initializing modules 👇_Info
     */
    public const int NumPages = 131072;
    public const ulong PageSize = 4096;

    public struct Info
    {
        public IntPtr Start;
        public fixed ulong Pages[NumPages]; //Max 512MiB
    }

    public static Info _Info;

    public static void Initialize(IntPtr Start)
    {
        _Info.Start = Start;
    }

    /// <summary>
    /// Returns a 4KB aligned address
    /// </summary>
    /// <param name="size"></param>
    /// <returns></returns>
    internal static unsafe IntPtr Allocate(ulong size)
    {
        ulong pages = 1;

        if (size > PageSize)
        {
            pages = (size / PageSize) + ((size % 4096) != 0 ? 1UL : 0);
        }

        ulong i = 0;
        bool found = false;

        for (i = 0; i < NumPages; i++)
        {
            if (_Info.Pages[i] == 0)
            {
                found = true;
                for (ulong k = 0; k < pages; k++)
                {
                    if (_Info.Pages[i] != 0)
                    {
                        found = false;
                        break;
                    }
                }
                if (found) break;
            }
        }
        if (!found) 
        {
            Console.WriteLine("Out of Memory!");
            Native.Cli();
            for (; ; ) Native.Hlt();
            return IntPtr.Zero; 
        }

        for (ulong k = 0; k < pages; k++)
        {
            _Info.Pages[i + k] = PageSignature;
        }
        _Info.Pages[i] = pages;

        IntPtr ptr = _Info.Start + (i * PageSize);
        return ptr;
    }

    internal static unsafe void MemoryCopy(IntPtr dst, IntPtr src, ulong size)
    {
        Native.Movsb((void*)dst, (void*)src, size);
    }
}