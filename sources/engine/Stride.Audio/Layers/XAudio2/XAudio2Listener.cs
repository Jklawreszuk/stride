// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using Stride.Core.Mathematics;

namespace Stride.Audio;

public class XAudio2Listener : IAudioListener
{
    public XAudio2Device Device;
    public X3DAudioListener XAudioListener;
    public Matrix WorldTransform;
}
