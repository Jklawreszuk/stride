// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.
#if STRIDE_PLATFORM_DESKTOP
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SkiaSharp;
using Stride.Core;

namespace Stride.Graphics
{
    /// <summary>
    /// This class is responsible to provide image loader for png, gif, bmp.
    /// </summary>
    partial class StandardImageHelper
    {
        public static unsafe Image LoadFromMemory(IntPtr pSource, int size, bool makeACopy, GCHandle? handle)
        {
            using var memoryStream = new UnmanagedMemoryStream((byte*)pSource, size, capacity: size, access: FileAccess.Read);
            var bitmap = SKImage.FromEncodedData (memoryStream);

            var image = Image.New2D(bitmap.Width, bitmap.Height, 1, PixelFormat.B8G8R8A8_UNorm, 1, 0);

            try
            {
                // Directly load image as RGBA instead of BGRA, because OpenGL ES devices don't support it out of the box (extension).
                bitmap.ReadPixels (bitmap.Info, image.PixelBuffer[0].DataPointer);
            }
            finally
            {
                if (handle != null)
                    handle.Value.Free();
                else if (!makeACopy)
                    Utilities.FreeMemory(pSource);
            }

            return image;
        }

        public static void SaveGifFromMemory(PixelBuffer[] pixelBuffers, int count, ImageDescription description, Stream imageStream)
        {
            SaveFromMemory(pixelBuffers, description, imageStream, SKEncodedImageFormat.Gif);
        }

        public static void SaveTiffFromMemory(PixelBuffer[] pixelBuffers, int count, ImageDescription description, Stream imageStream)
        {
            throw new NotImplementedException();
        }

        public static void SaveBmpFromMemory(PixelBuffer[] pixelBuffers, int count, ImageDescription description, Stream imageStream)
        {
            SaveFromMemory(pixelBuffers, description, imageStream, SKEncodedImageFormat.Bmp);
        }

        public static void SaveJpgFromMemory(PixelBuffer[] pixelBuffers, int count, ImageDescription description, Stream imageStream)
        {
            SaveFromMemory(pixelBuffers, description, imageStream, SKEncodedImageFormat.Jpeg);
        }

        public static void SavePngFromMemory(PixelBuffer[] pixelBuffers, int count, ImageDescription description, Stream imageStream)
        {
            SaveFromMemory(pixelBuffers, description, imageStream, SKEncodedImageFormat.Png);
        }

        public static void SaveWmpFromMemory(PixelBuffer[] pixelBuffers, int count, ImageDescription description, Stream imageStream)
        {
            throw new NotImplementedException();
        }

        private static unsafe void SaveFromMemory(PixelBuffer[] pixelBuffers, ImageDescription description, Stream imageStream, SKEncodedImageFormat imageFormat)
        {
            using var bitmap = new SKBitmap(description.Width, description.Height);

            // Copy memory
            var format = description.Format;
            if (format is PixelFormat.R8G8B8A8_UNorm or PixelFormat.R8G8B8A8_UNorm_SRgb)
            {
                CopyMemoryBGRA(bitmap.GetPixels(), pixelBuffers[0].DataPointer, pixelBuffers[0].BufferStride);
            }
            else if (format is PixelFormat.B8G8R8A8_UNorm or PixelFormat.B8G8R8A8_UNorm_SRgb)
            {
                Unsafe.CopyBlockUnaligned((void*)bitmap.GetPixels(), (void*)pixelBuffers[0].DataPointer, (uint)pixelBuffers[0].BufferStride);
            }
            else if (format is PixelFormat.R8_UNorm or PixelFormat.A8_UNorm)
            {
                // TODO Ideally we will want to support grayscale images, but the SpriteBatch can only render RGBA for now
                // so convert the grayscale image as an RGBA and save it
                CopyMemoryRRR1(bitmap.GetPixels(), pixelBuffers[0].DataPointer, pixelBuffers[0].BufferStride);
            }
            else
            {
                throw new ArgumentException(
                    message: $"The pixel format {format} is not supported. Supported formats are {PixelFormat.B8G8R8A8_UNorm}, {PixelFormat.B8G8R8A8_UNorm_SRgb}, {PixelFormat.R8G8B8A8_UNorm}, {PixelFormat.R8G8B8A8_UNorm_SRgb}, {PixelFormat.R8_UNorm}, {PixelFormat.A8_UNorm}",
                    paramName: nameof(description));
            }

            // Save
            bitmap.Encode(imageStream, imageFormat, 100);
        }
    }
}
#endif
