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
// $Date: 2007/11/28 15:33:40 $
// $Id: FREE_IMAGE_COLOR_CHANNEL.cs,v 1.1 2007/11/28 15:33:40 cklein05 Exp $
// ==========================================================

namespace FreeImageAPI
{
	/// <summary>
	/// Color channels. Constants used in color manipulation routines.
	/// </summary>
	public enum FreeImageColorChannel
	{
		/// <summary>
		/// Use red, green and blue channels
		/// </summary>
		Rgb = 0,
		/// <summary>
		/// Use red channel
		/// </summary>
		Red = 1,
		/// <summary>
		/// Use green channel
		/// </summary>
		Green = 2,
		/// <summary>
		/// Use blue channel
		/// </summary>
		Blue = 3,
		/// <summary>
		/// Use alpha channel
		/// </summary>
		Alpha = 4,
		/// <summary>
		/// Use black channel
		/// </summary>
		Black = 5,
		/// <summary>
		/// Complex images: use real part
		/// </summary>
		Real = 6,
		/// <summary>
		/// Complex images: use imaginary part
		/// </summary>
		Imag = 7,
		/// <summary>
		/// Complex images: use magnitude
		/// </summary>
		Mag = 8,
		/// <summary>
		/// Complex images: use phase
		/// </summary>
		Phase = 9
	}
}