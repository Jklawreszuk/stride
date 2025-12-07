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
// $Revision: 1.9 $
// $Date: 2009/02/27 16:35:12 $
// $Id: MetadataTag.cs,v 1.9 2009/02/27 16:35:12 cklein05 Exp $
// ==========================================================

using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Diagnostics;
using Stride.Core;

namespace FreeImageAPI.Metadata
{
	/// <summary>
	/// Manages metadata objects and operations.
	/// </summary>
	public sealed class MetadataTag : IComparable, IComparable<MetadataTag>, ICloneable, IEquatable<MetadataTag>, IDisposable
	{
		/// <summary>
		/// The encapsulated FreeImage-tag.
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		internal FITAG tag;

		/// <summary>
		/// The metadata model of <see cref="tag"/>.
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private FreeImageMetadataModel model;

		/// <summary>
		/// Indicates whether this instance has already been disposed.
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private bool disposed = false;

		/// <summary>
		/// Indicates whether this instance was created by FreeImage or
		/// by the user.
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private bool selfCreated;

		/// <summary>
		/// List linking metadata-model and Type.
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static readonly Dictionary<FreeImageMdType, Type> idList;

		/// <summary>
		/// List linking Type and metadata-model.
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static readonly Dictionary<Type, FreeImageMdType> typeList;

		/// <summary>
		/// Initializes a new instance of this class.
		/// </summary>
		private MetadataTag()
		{
		}

		/// <summary>
		/// Initializes a new instance of this class.
		/// </summary>
		/// <param name="model">The new model the tag should be of.</param>
		public MetadataTag(FreeImageMetadataModel model)
		{
			this.model = model;
			tag = FreeImage.CreateTag();
			selfCreated = true;

			if (model == FreeImageMetadataModel.Xmp)
			{
				Key = "XMLPacket";
			}
		}

		/// <summary>
		/// Initializes a new instance of this class.
		/// </summary>
		/// <param name="tag">The <see cref="FITAG"/> to represent.</param>
		/// <param name="dib">The bitmap <paramref name="tag"/> was extracted from.</param>
		public MetadataTag(FITAG tag, FIBITMAP dib)
		{
			if (tag.IsNull)
			{
				throw new ArgumentNullException("tag");
			}
			if (dib.IsNull)
			{
				throw new ArgumentNullException("dib");
			}
			this.tag = tag;
			model = GetModel(dib, tag);
			selfCreated = false;

			if (model == FreeImageMetadataModel.Xmp)
			{
				Key = "XMLPacket";
			}
		}

		/// <summary>
		/// Initializes a new instance of this class.
		/// </summary>
		/// <param name="tag">The <see cref="FITAG"/> to represent.</param>
		/// <param name="model">The model of <paramref name="tag"/>.</param>
		public MetadataTag(FITAG tag, FreeImageMetadataModel model)
		{
			if (tag.IsNull)
			{
				throw new ArgumentNullException("tag");
			}
			this.tag = tag;
			this.model = model;
			selfCreated = false;

			if (model == FreeImageMetadataModel.Xmp)
			{
				Key = "XMLPacket";
			}
		}

		static MetadataTag()
		{
			idList = new Dictionary<FreeImageMdType, Type>();
			idList.Add(FreeImageMdType.Byte, typeof(byte));
			idList.Add(FreeImageMdType.Short, typeof(ushort));
			idList.Add(FreeImageMdType.Long, typeof(uint));
			idList.Add(FreeImageMdType.Rational, typeof(FIURational));
			idList.Add(FreeImageMdType.SByte, typeof(sbyte));
			idList.Add(FreeImageMdType.Undefined, typeof(byte));
			idList.Add(FreeImageMdType.SShort, typeof(short));
			idList.Add(FreeImageMdType.SLong, typeof(int));
			idList.Add(FreeImageMdType.SRational, typeof(FIRational));
			idList.Add(FreeImageMdType.Float, typeof(float));
			idList.Add(FreeImageMdType.Double, typeof(double));
			idList.Add(FreeImageMdType.Ifd, typeof(uint));
			idList.Add(FreeImageMdType.Palette, typeof(RGBQUAD));

			typeList = new Dictionary<Type, FreeImageMdType>();
			typeList.Add(typeof(ushort), FreeImageMdType.Short);
			typeList.Add(typeof(ushort[]), FreeImageMdType.Short);
			typeList.Add(typeof(string), FreeImageMdType.Ascii);
			typeList.Add(typeof(uint), FreeImageMdType.Long);
			typeList.Add(typeof(uint[]), FreeImageMdType.Long);
			typeList.Add(typeof(FIURational), FreeImageMdType.Rational);
			typeList.Add(typeof(FIURational[]), FreeImageMdType.Rational);
			typeList.Add(typeof(sbyte), FreeImageMdType.SByte);
			typeList.Add(typeof(sbyte[]), FreeImageMdType.SByte);
			typeList.Add(typeof(byte), FreeImageMdType.Byte);
			typeList.Add(typeof(byte[]), FreeImageMdType.Byte);
			typeList.Add(typeof(short), FreeImageMdType.SShort);
			typeList.Add(typeof(short[]), FreeImageMdType.SShort);
			typeList.Add(typeof(int), FreeImageMdType.SLong);
			typeList.Add(typeof(int[]), FreeImageMdType.SLong);
			typeList.Add(typeof(FIRational), FreeImageMdType.SRational);
			typeList.Add(typeof(FIRational[]), FreeImageMdType.SRational);
			typeList.Add(typeof(float), FreeImageMdType.Float);
			typeList.Add(typeof(float[]), FreeImageMdType.Float);
			typeList.Add(typeof(double), FreeImageMdType.Double);
			typeList.Add(typeof(double[]), FreeImageMdType.Double);
			typeList.Add(typeof(RGBQUAD), FreeImageMdType.Palette);
			typeList.Add(typeof(RGBQUAD[]), FreeImageMdType.Palette);
		}

		/// <summary>
		/// Releases all resources used by the instance.
		/// </summary>
		~MetadataTag()
		{
			Dispose();
		}

		/// <summary>
		/// Determines whether two specified <see cref="MetadataTag"/> objects have the same value.
		/// </summary>
		/// <param name="left">A <see cref="MetadataTag"/> or a null reference (<b>Nothing</b> in Visual Basic).</param>
		/// <param name="right">A <see cref="MetadataTag"/> or a null reference (<b>Nothing</b> in Visual Basic).</param>
		/// <returns>
		/// <b>true</b> if the value of left is the same as the value of right; otherwise, <b>false</b>.
		/// </returns>
		public static bool operator ==(MetadataTag left, MetadataTag right)
		{
			// Check whether both are null
			if ((object)left == (object)right)
			{
				return true;
			}
			else if ((object)left == null || (object)right == null)
			{
				return false;
			}
			left.CheckDisposed();
			right.CheckDisposed();
			// Check all properties
			if ((left.Key != right.Key) ||
				(left.ID != right.ID) ||
				(left.Description != right.Description) ||
				(left.Count != right.Count) ||
				(left.Length != right.Length) ||
				(left.Model != right.Model) ||
				(left.Type != right.Type))
			{
				return false;
			}
			if (left.Length == 0)
			{
				return true;
			}
			IntPtr ptr1 = FreeImage.GetTagValue(left.tag);
			IntPtr ptr2 = FreeImage.GetTagValue(right.tag);
			return FreeImage.CompareMemory(ptr1, ptr2, left.Length);
		}

		/// <summary>
		/// Determines whether two specified <see cref="MetadataTag"/> objects have different values.
		/// </summary>
		/// <param name="left">A <see cref="MetadataTag"/> or a null reference (<b>Nothing</b> in Visual Basic).</param>
		/// <param name="right">A <see cref="MetadataTag"/> or a null reference (<b>Nothing</b> in Visual Basic).</param>
		/// <returns>
		/// true if the value of left is different from the value of right; otherwise, <b>false</b>.
		/// </returns>
		public static bool operator !=(MetadataTag left, MetadataTag right)
		{
			return !(left == right);
		}

		/// <summary>
		/// Extracts the value of a <see cref="MetadataTag"/> instance to a <see cref="FITAG"/> handle.
		/// </summary>
		/// <param name="value">A <see cref="MetadataTag"/> instance.</param>
		/// <returns>A new instance of <see cref="FITAG"/> initialized to <paramref name="value"/>.</returns>
		public static implicit operator FITAG(MetadataTag value)
		{
			return value.tag;
		}

		private static FreeImageMetadataModel GetModel(FIBITMAP dib, FITAG tag)
		{
			FITAG value;
			foreach (FreeImageMetadataModel model in FreeImage.FREE_IMAGE_MDMODELS)
			{
				FIMETADATA mData = FreeImage.FindFirstMetadata(model, dib, out value);
				if (mData.IsNull)
				{
					continue;
				}
				try
				{
					do
					{
						if (value == tag)
						{
							return model;
						}
					}
					while (FreeImage.FindNextMetadata(mData, out value));
				}
				finally
				{
					if (!mData.IsNull)
					{
						FreeImage.FindCloseMetadata(mData);
					}
				}
			}
			throw new ArgumentException("'tag' is no metadata object of 'dib'");
		}

		/// <summary>
		/// Gets the model of the metadata.
		/// </summary>
		public FreeImageMetadataModel Model
		{
			get { CheckDisposed(); return model; }
		}

		/// <summary>
		/// Gets or sets the key of the metadata.
		/// </summary>
		public string Key
		{
			get { CheckDisposed(); return FreeImage.GetTagKey(tag); }
			set
			{
				CheckDisposed();
				if ((model != FreeImageMetadataModel.Xmp) || (value == "XMLPacket"))
				{
					FreeImage.SetTagKey(tag, value);
				}
			}
		}

		/// <summary>
		/// Gets or sets the description of the metadata.
		/// </summary>
		public string Description
		{
			get { CheckDisposed(); return FreeImage.GetTagDescription(tag); }
			set { CheckDisposed(); FreeImage.SetTagDescription(tag, value); }
		}

		/// <summary>
		/// Gets or sets the ID of the metadata.
		/// </summary>
		public ushort ID
		{
			get { CheckDisposed(); return FreeImage.GetTagID(tag); }
			set { CheckDisposed(); FreeImage.SetTagID(tag, value); }
		}

		/// <summary>
		/// Gets the type of the metadata.
		/// </summary>
		public FreeImageMdType Type
		{
			get { CheckDisposed(); return FreeImage.GetTagType(tag); }
			internal set { FreeImage.SetTagType(tag, value); }
		}

		/// <summary>
		/// Gets the number of elements the metadata object contains.
		/// </summary>
		public uint Count
		{
			get { CheckDisposed(); return FreeImage.GetTagCount(tag); }
			private set { FreeImage.SetTagCount(tag, value); }
		}

		/// <summary>
		/// Gets the length of the value in bytes.
		/// </summary>
		public uint Length
		{
			get { CheckDisposed(); return FreeImage.GetTagLength(tag); }
			private set { FreeImage.SetTagLength(tag, value); }
		}

		private unsafe byte[] GetData()
		{
			uint length = Length;
			byte[] value = new byte[length];
			byte* ptr = (byte*)FreeImage.GetTagValue(tag);
			for (int i = 0; i < length; i++)
			{
				value[i] = ptr[i];
			}
			return value;
		}

		/// <summary>
		/// Gets or sets the value of the metadata.
		/// </summary>
		public object Value
		{
			get
			{
				unsafe
				{
					CheckDisposed();
					int cnt = (int)Count;

					if (Type == FreeImageMdType.Ascii)
					{
						byte* value = (byte*)FreeImage.GetTagValue(tag);
						StringBuilder sb = new StringBuilder();
						for (int i = 0; i < cnt; i++)
						{
							sb.Append(Convert.ToChar(value[i]));
						}
						return sb.ToString();
					}
					else if (Type == FreeImageMdType.NoType)
					{
						return null;
					}

					Array array = Array.CreateInstance(idList[Type], Count);

					ref byte dst = ref MemoryMarshal.GetArrayDataReference(array);
					ref byte src = ref Unsafe.AsRef<byte>((void*) FreeImage.GetTagValue(tag));
					Utilities.CopyWithAlignmentFallback(ref dst, ref src, Length);

					return array;
				}
			}
			set
			{
				SetValue(value);
			}
		}

		/// <summary>
		/// Sets the value of the metadata.
		/// <para> In case value is of byte or byte[] <see cref="FreeImageMdType.Undefined"/> is assumed.</para>
		/// <para> In case value is of uint or uint[] <see cref="FreeImageMdType.Long"/> is assumed.</para>
		/// </summary>
		/// <param name="value">New data of the metadata.</param>
		/// <returns>True on success, false on failure.</returns>
		/// <exception cref="NotSupportedException">
		/// The data format is not supported.</exception>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="value"/> is null.</exception>
		public bool SetValue(object value)
		{
			Type type = value.GetType();
			if (!typeList.TryGetValue(type, out var v))
			{
				throw new NotSupportedException("The type of value is not supported");
			}
			return SetValue(value, v);
		}

		/// <summary>
		/// Sets the value of the metadata.
		/// </summary>
		/// <param name="value">New data of the metadata.</param>
		/// <param name="type">Type of the data.</param>
		/// <returns>True on success, false on failure.</returns>
		/// <exception cref="NotSupportedException">
		/// The data type is not supported.</exception>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="value"/> is null.</exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="value"/> and <paramref name="type"/> to not fit.</exception>
		public bool SetValue(object value, FreeImageMdType type)
		{
			CheckDisposed();
			if ((!value.GetType().IsArray) && (value is not string))
			{
				Array array = Array.CreateInstance(value.GetType(), 1);
				array.SetValue(value, 0);
				return SetArrayValue(array, type);
			}
			return SetArrayValue(value, type);
		}

		/// <summary>
		/// Sets the value of this tag to the value of <paramref name="value"/>
		/// using the given type.
		/// </summary>
		/// <param name="value">New value of the tag.</param>
		/// <param name="type">Data-type of the tag.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="value"/> is a null reference.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="type"/> is FIDT_ASCII and
		/// <paramref name="value"/> is not String.
		/// <paramref name="type"/> is not FIDT_ASCII and
		/// <paramref name="value"/> is not Array.</exception>
		/// <exception cref="NotSupportedException">
		/// <paramref name="type"/> is FIDT_NOTYPE.</exception>
		private unsafe bool SetArrayValue(object value, FreeImageMdType type)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}

			byte[] data = null;

			if (type == FreeImageMdType.Ascii)
			{
				if (value is not string tempValue)
				{
					throw new ArgumentException("value");
				}
				Type = type;
				Length = Count = (uint)tempValue.Length;
				data = new byte[Length];

				for (int i = 0; i < tempValue.Length; i++)
				{
					data[i] = (byte)tempValue[i];
				}
			}
			else if (type == FreeImageMdType.NoType)
			{
				throw new NotSupportedException("type is not supported.");
			}
			else
			{
				if (value is not Array array)
				{
					throw new ArgumentException(nameof(value));
				}

				if (array.Length != 0)
					if (!CheckType(array.GetValue(0).GetType(), type))
						throw new ArgumentException("The type of value is incorrect.");

				Type = type;
				Count = (uint)array.Length;
				Length = (uint)(array.Length * Marshal.SizeOf(idList[type]));

				data = new byte[Length];

				ref byte dst = ref data[0];
				ref byte src = ref MemoryMarshal.GetArrayDataReference(array);
				Utilities.CopyWithAlignmentFallback(ref dst, ref src, Length);
			}

			return FreeImage.SetTagValue(tag, data);
		}

		private static bool CheckType(Type dataType, FreeImageMdType type)
		{
			if (dataType != null)
				switch (type)
				{
					case FreeImageMdType.Ascii:
						return dataType == typeof(string);
					case FreeImageMdType.Byte:
						return dataType == typeof(byte);
					case FreeImageMdType.Double:
						return dataType == typeof(double);
					case FreeImageMdType.Float:
						return dataType == typeof(float);
					case FreeImageMdType.Ifd:
						return dataType == typeof(uint);
					case FreeImageMdType.Long:
						return dataType == typeof(uint);
					case FreeImageMdType.NoType:
						return false;
					case FreeImageMdType.Palette:
						return dataType == typeof(RGBQUAD);
					case FreeImageMdType.Rational:
						return dataType == typeof(FIURational);
					case FreeImageMdType.SByte:
						return dataType == typeof(sbyte);
					case FreeImageMdType.Short:
						return dataType == typeof(ushort);
					case FreeImageMdType.SLong:
						return dataType == typeof(int);
					case FreeImageMdType.SRational:
						return dataType == typeof(FIRational);
					case FreeImageMdType.SShort:
						return dataType == typeof(short);
					case FreeImageMdType.Undefined:
						return dataType == typeof(byte);
				}
			return false;
		}

		/// <summary>
		/// Add this metadata to an image.
		/// </summary>
		/// <param name="dib">Handle to a FreeImage bitmap.</param>
		/// <returns>True on success, false on failure.</returns>
		public bool AddToImage(FIBITMAP dib)
		{
			CheckDisposed();
			if (dib.IsNull)
			{
				throw new ArgumentNullException("dib");
			}
			if (Key == null)
			{
				throw new ArgumentNullException("Key");
			}
			if (!selfCreated)
			{
				tag = FreeImage.CloneTag(tag);
				if (tag.IsNull)
				{
					throw new Exception("FreeImage.CloneTag() failed.");
				}
				selfCreated = true;
			}
			if (!FreeImage.SetMetadata(Model, dib, Key, tag))
			{
				return false;
			}
			FreeImageMetadataModel _model = Model;
			string _key = Key;
			selfCreated = false;
			FreeImage.DeleteTag(tag);
			return FreeImage.GetMetadata(_model, dib, _key, out tag);
		}

		/// <summary>
		/// Gets a .NET PropertyItem for this metadata tag.
		/// </summary>
		/// <returns>The .NET PropertyItem.</returns>
		public unsafe PropertyItem GetPropertyItem()
		{
			PropertyItem item = FreeImage.CreatePropertyItem();
			item.Id = ID;
			item.Len = (int)Length;
			item.Type = (short)Type;
			item.Value = new byte[item.Len];

			ref byte dst = ref item.Value[0];
			ref byte src = ref Unsafe.AsRef<byte>((void*) FreeImage.GetTagValue(tag));
			Utilities.CopyWithAlignmentFallback(ref dst, ref src, Length);

			return item;
		}

		/// <summary>
		/// Converts the value of the <see cref="MetadataTag"/> object
		/// to its equivalent string representation.
		/// </summary>
		/// <returns>The string representation of the value of this instance.</returns>
		public override string ToString()
		{
			CheckDisposed();
			string fiString = FreeImage.TagToString(model, tag, 0);

			return string.IsNullOrEmpty(fiString) ? tag.ToString() : fiString;
		}

		/// <summary>
		/// Creates a deep copy of this <see cref="MetadataTag"/>.
		/// </summary>
		/// <returns>A deep copy of this <see cref="MetadataTag"/>.</returns>
		public object Clone()
		{
			CheckDisposed();
			MetadataTag clone = new MetadataTag();
			clone.model = model;
			clone.tag = FreeImage.CloneTag(tag);
			clone.selfCreated = true;
			return clone;
		}

		/// <summary>
		/// Tests whether the specified object is a <see cref="MetadataTag"/> instance
		/// and is equivalent to this <see cref="MetadataTag"/> instance.
		/// </summary>
		/// <param name="obj">The object to test.</param>
		/// <returns><b>true</b> if <paramref name="obj"/> is a <see cref="MetadataTag"/> instance
		/// equivalent to this <see cref="MetadataTag"/> instance; otherwise, <b>false</b>.</returns>
		public override bool Equals(object obj)
		{
			return ((obj is MetadataTag) && (Equals((MetadataTag)obj)));
		}

		/// <summary>
		/// Tests whether the specified <see cref="MetadataTag"/> instance is equivalent to this <see cref="MetadataTag"/> instance.
		/// </summary>
		/// <param name="other">A <see cref="MetadataTag"/> instance to compare to this instance.</param>
		/// <returns><b>true</b> if  equivalent to this <see cref="MetadataTag"/> instance;
		/// otherwise, <b>false</b>.</returns>
		public bool Equals(MetadataTag other)
		{
			return (this == other);
		}

		/// <summary>
		/// Returns a hash code for this <see cref="MetadataTag"/> structure.
		/// </summary>
		/// <returns>An integer value that specifies the hash code for this <see cref="MetadataTag"/>.</returns>
		public override int GetHashCode()
		{
			return tag.GetHashCode();
		}

		/// <summary>
		/// Compares this instance with a specified <see cref="Object"/>.
		/// </summary>
		/// <param name="obj">An object to compare with this instance.</param>
		/// <returns>A 32-bit signed integer indicating the lexical relationship between the two comparands.</returns>
		/// <exception cref="ArgumentException"><paramref name="obj"/> is not a <see cref="MetadataTag"/>.</exception>
		public int CompareTo(object obj)
		{
			if (obj == null)
			{
				return 1;
			}
			if (!(obj is MetadataTag))
			{
				throw new ArgumentException("obj");
			}
			return CompareTo((MetadataTag)obj);
		}

		/// <summary>
		/// Compares the current instance with another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this instance.</param>
		/// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
		public int CompareTo(MetadataTag other)
		{
			CheckDisposed();
			other.CheckDisposed();
			return tag.CompareTo(other.tag);
		}

		/// <summary>
		/// Releases all resources used by the instance.
		/// </summary>
		public void Dispose()
		{
			if (!disposed)
			{
				disposed = true;
				if (selfCreated)
				{
					FreeImage.DeleteTag(tag);
					tag = FITAG.Zero;
				}
			}
		}

		/// <summary>
		/// Gets whether this instance has already been disposed.
		/// </summary>
		public bool Disposed
		{
			get { return disposed; }
		}

		/// <summary>
		/// Throwns an <see cref="ObjectDisposedException"/> in case
		/// this instance has already been disposed.
		/// </summary>
		private void CheckDisposed()
		{
			if (disposed)
			{
				throw new ObjectDisposedException("The object has already been disposed.");
			}
		}
	}
}
