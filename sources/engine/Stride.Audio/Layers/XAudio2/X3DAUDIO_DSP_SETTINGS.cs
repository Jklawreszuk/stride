// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

namespace Stride.Audio;

public class X3DAUDIO_DSP_SETTINGS
{
    public float[] pMatrixCoefficients;
    public float[] pDelayTimes;
    public int SrcChannelCount { get; set; }
    public int DstChannelCount { get; set; }
}
