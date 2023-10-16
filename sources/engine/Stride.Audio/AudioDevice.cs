// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System.Collections.Generic;
using Silk.NET.OpenAL;

namespace Stride.Audio
{
    /// <summary>
    /// Reprensent an Audio Hardware Device.
    /// Can be used when creating an <see cref="AudioEngine"/> to specify the device on which to play the sound.
    /// </summary>
    public class AudioDevice
    {
        /// <summary>
        /// Returns the name of the current device.
        /// </summary>
        public string Name { get; set; }
        public unsafe Device* ALDevice { get; internal set; }
        public object DeviceLock { get; internal set; }
        public HashSet<object> Listeners { get; internal set; }

        public AudioDevice()
        {
            Name = "default";
        }
    }
}
