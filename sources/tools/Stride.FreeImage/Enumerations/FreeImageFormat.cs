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
// $Revision: 1.2 $
// $Date: 2009/09/15 11:44:42 $
// $Id: FREE_IMAGE_FORMAT.cs,v 1.2 2009/09/15 11:44:42 cklein05 Exp $
// ==========================================================

namespace FreeImageAPI
{
	/// <summary>
	/// I/O image format identifiers.
	/// </summary>
	public enum FreeImageFormat
	{
		/// <summary>
		/// Unknown format (returned value only, never use it as input value)
		/// </summary>
		Unknown = -1,
		/// <summary>
		/// Windows or OS/2 Bitmap File (*.BMP)
		/// </summary>
		Bmp = 0,
		/// <summary>
		/// Windows Icon (*.ICO)
		/// </summary>
		Ico = 1,
		/// <summary>
		/// Independent JPEG Group (*.JPG, *.JIF, *.JPEG, *.JPE)
		/// </summary>
		Jpeg = 2,
		/// <summary>
		/// JPEG Network Graphics (*.JNG)
		/// </summary>
		Jng = 3,
		/// <summary>
		/// Commodore 64 Koala format (*.KOA)
		/// </summary>
		Koala = 4,
		/// <summary>
		/// Amiga IFF (*.IFF, *.LBM)
		/// </summary>
		Lbm = 5,
		/// <summary>
		/// Amiga IFF (*.IFF, *.LBM)
		/// </summary>
		Iff = 5,
		/// <summary>
		/// Multiple Network Graphics (*.MNG)
		/// </summary>
		Mng = 6,
		/// <summary>
		/// Portable Bitmap (ASCII) (*.PBM)
		/// </summary>
		Pbm = 7,
		/// <summary>
		/// Portable Bitmap (BINARY) (*.PBM)
		/// </summary>
		PBMRaw = 8,
		/// <summary>
		/// Kodak PhotoCD (*.PCD)
		/// </summary>
		Pcd = 9,
		/// <summary>
		/// Zsoft Paintbrush PCX bitmap format (*.PCX)
		/// </summary>
		Pcx = 10,
		/// <summary>
		/// Portable Graymap (ASCII) (*.PGM)
		/// </summary>
		Pgm = 11,
		/// <summary>
		/// Portable Graymap (BINARY) (*.PGM)
		/// </summary>
		PgmRaw = 12,
		/// <summary>
		/// Portable Network Graphics (*.PNG)
		/// </summary>
		Png = 13,
		/// <summary>
		/// Portable Pixelmap (ASCII) (*.PPM)
		/// </summary>
		Ppm = 14,
		/// <summary>
		/// Portable Pixelmap (BINARY) (*.PPM)
		/// </summary>
		PpmRaw = 15,
		/// <summary>
		/// Sun Rasterfile (*.RAS)
		/// </summary>
		Ras = 16,
		/// <summary>
		/// truevision Targa files (*.TGA, *.TARGA)
		/// </summary>
		Targa = 17,
		/// <summary>
		/// Tagged Image File Format (*.TIF, *.TIFF)
		/// </summary>
		Tiff = 18,
		/// <summary>
		/// Wireless Bitmap (*.WBMP)
		/// </summary>
		WBmp = 19,
		/// <summary>
		/// Adobe Photoshop (*.PSD)
		/// </summary>
		Psd = 20,
		/// <summary>
		/// Dr. Halo (*.CUT)
		/// </summary>
		Cut = 21,
		/// <summary>
		/// X11 Bitmap Format (*.XBM)
		/// </summary>
		Xbm = 22,
		/// <summary>
		/// X11 Pixmap Format (*.XPM)
		/// </summary>
		Xpm = 23,
		/// <summary>
		/// DirectDraw Surface (*.DDS)
		/// </summary>
		Dds = 24,
		/// <summary>
		/// Graphics Interchange Format (*.GIF)
		/// </summary>
		Gif = 25,
		/// <summary>
		/// High Dynamic Range (*.HDR)
		/// </summary>
		Hdr = 26,
		/// <summary>
		/// Raw Fax format CCITT G3 (*.G3)
		/// </summary>
		FaxG3 = 27,
		/// <summary>
		/// Silicon Graphics SGI image format (*.SGI)
		/// </summary>
		Sgi = 28,
		/// <summary>
		/// OpenEXR format (*.EXR)
		/// </summary>
		Exr = 29,
		/// <summary>
		/// JPEG-2000 format (*.J2K, *.J2C)
		/// </summary>
		J2K = 30,
		/// <summary>
		/// JPEG-2000 format (*.JP2)
		/// </summary>
		Jp2 = 31,
		/// <summary>
		/// Portable FloatMap (*.PFM)
		/// </summary>
		Pfm = 32,
		/// <summary>
		/// Macintosh PICT (*.PICT)
		/// </summary>
		Pict = 33,
		/// <summary>
		/// RAW camera image (*.*)
		/// </summary>
		Raw = 34,
	}
}