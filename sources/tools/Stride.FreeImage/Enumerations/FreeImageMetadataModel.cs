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
// $Revision: 1.1 $
// $Date: 2007/11/28 15:33:39 $
// $Id: FREE_IMAGE_MDMODEL.cs,v 1.1 2007/11/28 15:33:39 cklein05 Exp $
// ==========================================================

namespace FreeImageAPI
{
	/// <summary>
	/// Metadata models supported by FreeImage.
	/// </summary>
	public enum FreeImageMetadataModel
	{
		/// <summary>
		/// No data
		/// </summary>
		NoData = -1,
		/// <summary>
		/// single comment or keywords
		/// </summary>
		Comments = 0,
		/// <summary>
		/// Exif-TIFF metadata
		/// </summary>
		ExifMain = 1,
		/// <summary>
		/// Exif-specific metadata
		/// </summary>
		ExifExif = 2,
		/// <summary>
		/// Exif GPS metadata
		/// </summary>
		ExifGps = 3,
		/// <summary>
		/// Exif maker note metadata
		/// </summary>
		ExifMakerNote = 4,
		/// <summary>
		/// Exif interoperability metadata
		/// </summary>
		ExifInterop = 5,
		/// <summary>
		/// IPTC/NAA metadata
		/// </summary>
		Iptc = 6,
		/// <summary>
		/// Adobe XMP metadata
		/// </summary>
		Xmp = 7,
		/// <summary>
		/// GeoTIFF metadata
		/// </summary>
		GeoTiff = 8,
		/// <summary>
		/// Animation metadata
		/// </summary>
		Animation = 9,
		/// <summary>
		/// Used to attach other metadata types to a dib
		/// </summary>
		Custom = 10
	}
}
