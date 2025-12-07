// ==========================================================
// FreeImage 3 .NET wrapper
// Original FreeImage 3 functions and .NET compatible derived functions
//
// Design and implementation by
// - Jean-Philippe Goerke (jpgoerke@users.sourceforge.net)
// - Carsten Klein (cklein05@users.sourceforge.net)
//
// Contributors:
// - David Boland (davidboland@vodafone.ie)
//
// Main reference : MSDN Knowlede Base
//
// This file is part of FreeImage 3
//
// COVERED CODE IS PROVIDED UNDER THIS LICENSE ON AN "AS IS" BASIS, WITHOUT WARRANTY
// OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING, WITHOUT LIMITATION, WARRANTIES
// THAT THE COVERED CODE IS FREE OF DEFECTS, MERCHANTABLE, FIT FOR A PARTICULAR PURPOSE
// OR NON-INFRINGING. THE ENTIRE RISK AS TO THE QUALITY AND PERFORMANCE OF THE COVERED
// CODE IS WITH YOU. SHOULD ANY COVERED CODE PROVE DEFECTIVE IN ANY RESPECT, YOU (NOT
// THE INITIAL DEVELOPER OR ANY OTHER CONTRIBUTOR) ASSUME THE COST OF ANY NECESSARY
// SERVICING, REPAIR OR CORRECTION. THIS DISCLAIMER OF WARRANTY CONSTITUTES AN ESSENTIAL
// PART OF THIS LICENSE. NO USE OF ANY COVERED CODE IS AUTHORIZED HEREUNDER EXCEPT UNDER
// THIS DISCLAIMER.
//
// Use at your own risk!
// ==========================================================

// ==========================================================
// CVS
// $Revision: 1.3 $
// $Date: 2011/12/22 14:53:28 $
// $Id: FREE_IMAGE_SAVE_FLAGS.cs,v 1.3 2011/12/22 14:53:28 drolon Exp $
// ==========================================================

namespace FreeImageAPI
{
	/// <summary>
	/// Flags used in save functions.
	/// </summary>
	[System.Flags]
	public enum FreeImageSaveFlags
	{
		/// <summary>
		/// Default option for all types.
		/// </summary>
		Default = 0,
		/// <summary>
		/// Save with run length encoding.
		/// </summary>
		BmpSaveRLe = 1,
		/// <summary>
		/// Save data as float instead of as half (not recommended).
		/// </summary>
		ExrFloat = 0x0001,
		/// <summary>
		/// Save with no compression.
		/// </summary>
		ExrNone = 0x0002,
		/// <summary>
		/// Save with zlib compression, in blocks of 16 scan lines.
		/// </summary>
		ExrZip = 0x0004,
		/// <summary>
		/// Save with piz-based wavelet compression.
		/// </summary>
		ExrPiz = 0x0008,
		/// <summary>
		/// Save with lossy 24-bit float compression.
		/// </summary>
		ExrPxr24 = 0x0010,
		/// <summary>
		/// Save with lossy 44% float compression - goes to 22% when combined with EXR_LC.
		/// </summary>
		ExrB44 = 0x0020,
		/// <summary>
		/// Save images with one luminance and two chroma channels, rather than as RGB (lossy compression).
		/// </summary>
		ExrLc = 0x0040,
		/// <summary>
		/// Save with superb quality (100:1).
		/// </summary>
		JpegQualitySuperb = 0x80,
		/// <summary>
		/// Save with good quality (75:1).
		/// </summary>
		JpegQualityGood = 0x0100,
		/// <summary>
		/// Save with normal quality (50:1).
		/// </summary>
		JpegQualityNormal = 0x0200,
		/// <summary>
		/// Save with average quality (25:1).
		/// </summary>
		JpegQualityAverage = 0x0400,
		/// <summary>
		/// Save with bad quality (10:1).
		/// </summary>
		JpegQualityBad = 0x0800,
		/// <summary>
		/// Save as a progressive-JPEG (use | to combine with other save flags).
		/// </summary>
		JpegProgressive = 0x2000,
		/// <summary>
		/// Save with high 4x1 chroma subsampling (4:1:1).
		/// </summary>
		JpegSubsampling411 = 0x1000,
		/// <summary>
		/// Save with medium 2x2 medium chroma (4:2:0).
		/// </summary>
		JpegSubsampling420 = 0x4000,
		/// <summary>
		/// Save with low 2x1 chroma subsampling (4:2:2).
		/// </summary>
		JpegSubsampling422 = 0x8000,
		/// <summary>
		/// Save with no chroma subsampling (4:4:4).
		/// </summary>
		JpegSubsampling444 = 0x10000,
        /// <summary>
        /// On saving, compute optimal Huffman coding tables (can reduce a few percent of file size).
        /// </summary>
        JpegOptimize = 0x20000,
        /// <summary>
        /// save basic JPEG, without metadata or any markers.
        /// </summary>
        JpegBaseline = 0x40000,
		/// <summary>
		/// Save using ZLib level 1 compression flag
		/// (default value is <see cref="PngZDefaultCompression"/>).
		/// </summary>
		PngZBestSpeed = 0x0001,
		/// <summary>
		/// Save using ZLib level 6 compression flag (default recommended value).
		/// </summary>
		PngZDefaultCompression = 0x0006,
		/// <summary>
		/// save using ZLib level 9 compression flag
		/// (default value is <see cref="PngZDefaultCompression"/>).
		/// </summary>
		PngZBestCompression = 0x0009,
		/// <summary>
		/// Save without ZLib compression.
		/// </summary>
		PngZNoCompression = 0x0100,
		/// <summary>
		/// Save using Adam7 interlacing (use | to combine with other save flags).
		/// </summary>
		PngInterlaced = 0x0200,
		/// <summary>
		/// If set the writer saves in ASCII format (i.e. P1, P2 or P3).
		/// </summary>
		PnmSaveAscii = 1,
		/// <summary>
		/// Stores tags for separated CMYK (use | to combine with compression flags).
		/// </summary>
		TiffCmyk = 0x0001,
		/// <summary>
		/// Save using PACKBITS compression.
		/// </summary>
		TiffPackbits = 0x0100,
		/// <summary>
		/// Save using DEFLATE compression (a.k.a. ZLIB compression).
		/// </summary>
		TiffDeflate = 0x0200,
		/// <summary>
		/// Save using ADOBE DEFLATE compression.
		/// </summary>
		TiffAdobeDeflate = 0x0400,
		/// <summary>
		/// Save without any compression.
		/// </summary>
		TiffNone = 0x0800,
		/// <summary>
		/// Save using CCITT Group 3 fax encoding.
		/// </summary>
		TiffCcittFax3 = 0x1000,
		/// <summary>
		/// Save using CCITT Group 4 fax encoding.
		/// </summary>
		TiffCcittFax4 = 0x2000,
		/// <summary>
		/// Save using LZW compression.
		/// </summary>
		TiffLzw = 0x4000,
		/// <summary>
		/// Save using JPEG compression.
		/// </summary>
		TiffJpeg = 0x8000
	}
}
