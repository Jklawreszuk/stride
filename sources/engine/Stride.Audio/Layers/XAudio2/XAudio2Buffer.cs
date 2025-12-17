// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Buffer = Silk.NET.XAudio.Buffer;

namespace Stride.Audio;

public unsafe struct XAudio2Buffer : IAudioBuffer, IDisposable
{
    public Buffer* Buffer;
    public int Size;
    public BufferType Type { get; set; }
    public XAudio2Buffer(int maxBufferSize)
    {
        Size = maxBufferSize;

        Buffer = (Buffer*)Marshal.AllocHGlobal(sizeof(Buffer));
        Unsafe.InitBlock(Buffer, 0, (uint)sizeof(Buffer));

        Buffer->PAudioData = (byte*)Marshal.AllocHGlobal(maxBufferSize);

        Buffer->PContext = Buffer;
    }

    public void Dispose()
    {
        if (Buffer == null)
            return;

        Marshal.FreeHGlobal((IntPtr)Buffer->PAudioData);
        Marshal.FreeHGlobal((IntPtr)Buffer);
    }
}
