﻿// 
//  Copyright 2010-2015 Deveel
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//

using System;
using System.IO;
using System.Text;

using Deveel.Data.Sql.Objects;

namespace Deveel.Data.Types {
	public sealed class NumericType : DataType, ISizeableType {
		public NumericType(SqlTypeCode sqlType, int size, byte scale) 
			: base("NUMERIC", sqlType) {
			AssertIsNumeric(sqlType);
			Size = size;
			Scale = scale;
		}

		public NumericType(SqlTypeCode sqlType)
			: this(sqlType, -1) {
		}

		public NumericType(SqlTypeCode sqlType, int size)
			: this(sqlType, size, 0) {
		}

		public int Size { get; private set; }

		public byte Scale { get; private set; }

		public override bool IsStorable {
			get { return true; }
		}

		public override bool Equals(object obj) {
			var other = obj as NumericType;
			if (other == null)
				return false;

			return SqlType == other.SqlType &&
			       Size == other.Size &&
			       Scale == other.Scale;
		}

		public override bool IsCacheable(ISqlObject value) {
			return value is SqlNumber || value is SqlNull;
		}

		public override int GetHashCode() {
			return (SqlType.GetHashCode() * Scale) + Size.GetHashCode();
		}

		private static void AssertIsNumeric(SqlTypeCode typeCode) {
			if (!IsNumericType(typeCode))
				throw new ArgumentException(String.Format("The type '{0}' is not a valid NUMERIC type.", typeCode));
		}

		private static int GetIntSize(SqlTypeCode sqlType) {
			switch (sqlType) {
				case SqlTypeCode.TinyInt:
					return 1;
				case SqlTypeCode.SmallInt:
					return 2;
				case SqlTypeCode.Integer:
					return 4;
				case SqlTypeCode.BigInt:
					return 8;
				default:
					return 0;
			}
		}


		private static int GetFloatSize(SqlTypeCode sqlType) {
			switch (sqlType) {
				default:
					return 0;
				case SqlTypeCode.Real:
					return 4;
				case SqlTypeCode.Float:
				case SqlTypeCode.Double:
					return 8;
			}
		}

		public override DataType Wider(DataType otherType) {
			SqlTypeCode t1SqlType = SqlType;
			SqlTypeCode t2SqlType = otherType.SqlType;
			if (t1SqlType == SqlTypeCode.Decimal) {
				return this;
			}
			if (t2SqlType == SqlTypeCode.Decimal) {
				return otherType;
			}
			if (t1SqlType == SqlTypeCode.Numeric) {
				return this;
			}
			if (t2SqlType == SqlTypeCode.Numeric) {
				return otherType;
			}

			if (t1SqlType == SqlTypeCode.Bit) {
				return otherType; // It can't be any smaller than a Bit
			}
			if (t2SqlType == SqlTypeCode.Bit) {
				return this;
			}

			int t1IntSize = GetIntSize(t1SqlType);
			int t2IntSize = GetIntSize(t2SqlType);
			if (t1IntSize > 0 && t2IntSize > 0) {
				// Both are int types, use the largest size
				return (t1IntSize > t2IntSize) ? this : otherType;
			}

			int t1FloatSize = GetFloatSize(t1SqlType);
			int t2FloatSize = GetFloatSize(t2SqlType);
			if (t1FloatSize > 0 && t2FloatSize > 0) {
				// Both are floating types, use the largest size
				return (t1FloatSize > t2FloatSize) ? this : otherType;
			}

			if (t1FloatSize > t2IntSize) {
				return this;
			}
			if (t2FloatSize > t1IntSize) {
				return otherType;
			}
			if (t1IntSize >= t2FloatSize || t2IntSize >= t1FloatSize) {
				// Must be a long (8 bytes) and a real (4 bytes), widen to a double
				return new NumericType(SqlTypeCode.Double, 8, 0);
			}

			// NOTREACHED - can't get here, the last three if statements cover
			// all possibilities.
			throw new InvalidOperationException("Widest type error.");
		}

		public override bool IsComparable(DataType type) {
			return type is NumericType || type is BooleanType;
		}

		public override int Compare(ISqlObject x, ISqlObject y) {
			var n1 = (SqlNumber)x;
			SqlNumber n2;

			if (y is SqlNumber) {
				n2 = (SqlNumber)y;
			} else if (y is SqlBoolean) {
				n2 = (SqlBoolean) y ? SqlNumber.One : SqlNumber.Zero;
			} else {
				throw new NotSupportedException();
			}

			return n1.CompareTo(n2);
		}

		private static SqlDateTime ToDate(long time) {
			return new SqlDateTime(time);
		}

		public override bool CanCastTo(DataType destType) {
			return destType.SqlType != SqlTypeCode.Array &&
			       destType.SqlType != SqlTypeCode.ColumnType &&
			       destType.SqlType != SqlTypeCode.RowType &&
			       destType.SqlType != SqlTypeCode.Object;
		}

		public override DataObject CastTo(DataObject value, DataType destType) {
			var n = (SqlNumber) value.Value;
			var sqlType = destType.SqlType;
			ISqlObject casted;

			switch (sqlType) {
				case (SqlTypeCode.Bit):
				case (SqlTypeCode.Boolean):
					casted = new SqlBoolean(n.ToBoolean());
					break;
				case (SqlTypeCode.TinyInt):
				case (SqlTypeCode.SmallInt):
				case (SqlTypeCode.Integer):
					casted = new SqlNumber(n.ToInt32());
					break;
				case (SqlTypeCode.BigInt):
					casted = new SqlNumber(n.ToInt64());
					break;
				case (SqlTypeCode.Float):
				case (SqlTypeCode.Real):
				case (SqlTypeCode.Double):
					double d;
					if (n.State == NumericState.NotANumber) {
						casted = new SqlNumber(Double.NaN);
					} else if (n.State == NumericState.PositiveInfinity) {
						casted = new SqlNumber(Double.PositiveInfinity);
					} else if (n.State == NumericState.NegativeInfinity) {
						casted = new SqlNumber(Double.NegativeInfinity);
					} else {
						casted = new SqlNumber(n.ToDouble());
					}

					break;
				case (SqlTypeCode.Numeric):
				case (SqlTypeCode.Decimal):
					casted = n;
					break;
				case (SqlTypeCode.Char):
					casted = new SqlString(n.ToString().PadRight(((StringType) destType).MaxSize));
					break;
				case (SqlTypeCode.VarChar):
				case (SqlTypeCode.LongVarChar):
				case (SqlTypeCode.String):
					casted = new SqlString(n.ToString());
					break;
				case (SqlTypeCode.Date):
				case (SqlTypeCode.Time):
				case (SqlTypeCode.TimeStamp):
					casted = ToDate(n.ToInt64());
					break;
				case (SqlTypeCode.Blob):
				case (SqlTypeCode.Binary):
				case (SqlTypeCode.VarBinary):
				case (SqlTypeCode.LongVarBinary):
					casted = new SqlBinary(n.ToByteArray());
					break;
				case (SqlTypeCode.Null):
					casted = SqlNull.Value;
					break;
				default:
					throw new InvalidCastException();
			}

			return new DataObject(destType, casted);
		}

		public override string ToString() {
			var sb = new StringBuilder(Name);
			if (Size != -1) {
				sb.Append('(');
				sb.Append(Size);
				if (Scale > 0) {
					sb.Append(',');
					sb.Append(Scale);
				}

				sb.Append(')');
			}
			return sb.ToString();
		}

		public override object ConvertTo(ISqlObject obj, Type destType) {
			if (!(obj is SqlNumber))
				throw new ArgumentException();

			var number = (SqlNumber)obj;
			if (number.IsNull)
				return null;

			if (destType == typeof(byte))
				return number.ToByte();
			if (destType == typeof(short))
				return number.ToInt16();
			if (destType == typeof(int))
				return number.ToInt32();
			if (destType == typeof(long))
				return number.ToInt64();
			if (destType == typeof(float))
				return number.ToSingle();
			if (destType == typeof(double))
				return number.ToDouble();

			if (destType == typeof(bool))
				return number.ToBoolean();

			if (destType == typeof(string))
				return number.ToString();

			return base.ConvertTo(obj, destType);
		}

		public override ISqlObject Add(ISqlObject a, ISqlObject b) {
			if (!(a is SqlNumber))
				throw new ArgumentException();
			if (b is SqlNull || b.IsNull)
				return SqlNumber.Null;

			var num1 = (SqlNumber) a;
			SqlNumber num2;

			if (b is SqlBoolean) {
				if ((SqlBoolean) b) {
					num2 = SqlNumber.One;
				} else if (!(SqlBoolean) b) {
					num2 = SqlNumber.Zero;
				} else {
					num2 = SqlNumber.Null;
				}
			} else if (b is SqlNumber) {
				num2 = (SqlNumber) b;
			} else {
				throw new ArgumentException();
			}

			return num1.Add(num2);
		}

		public override ISqlObject Subtract(ISqlObject a, ISqlObject b) {
			if (!(a is SqlNumber))
				throw new ArgumentException();
			if (b is SqlNull || b.IsNull)
				return SqlNumber.Null;

			var num1 = (SqlNumber) a;
			SqlNumber num2;

			if (b is SqlBoolean) {
				if ((SqlBoolean) b) {
					num2 = SqlNumber.One;
				} else if (!(SqlBoolean) b) {
					num2 = SqlNumber.Zero;
				} else {
					num2 = SqlNumber.Null;
				}
			} else if (b is SqlNumber) {
				num2 = (SqlNumber) b;
			} else {
				throw new ArgumentException();
			}

			return num1.Subtract(num2);
		}

		public override ISqlObject Multiply(ISqlObject a, ISqlObject b) {
			if (!(a is SqlNumber) ||
				!(b is SqlNumber))
				throw new ArgumentException();
			if (b.IsNull)
				return SqlNumber.Null;

			if (a.IsNull)
				return a;

			var num1 = (SqlNumber) a;
			var num2 = (SqlNumber) b;

			return num1.Multiply(num2);
		}

		public override ISqlObject Divide(ISqlObject a, ISqlObject b) {
			if (!(a is SqlNumber) ||
			    !(b is SqlNumber))
				throw new ArgumentException();

			if (b.IsNull)
				return SqlNumber.Null;

			if (a.IsNull)
				return a;

			var num1 = (SqlNumber)a;
			var num2 = (SqlNumber)b;

			return num1.Divide(num2);
		}

		public override ISqlObject Modulus(ISqlObject a, ISqlObject b) {
			if (!(a is SqlNumber) ||
			    !(b is SqlNumber))
				throw new ArgumentException();

			if (b.IsNull)
				return SqlNumber.Null;

			if (a.IsNull)
				return a;

			var num1 = (SqlNumber) a;
			var num2 = (SqlNumber) b;

			return num1.Modulo(num2);
		}

		public override SqlBoolean IsSmallerThan(ISqlObject a, ISqlObject b) {
			if (!(a is SqlNumber) ||
			    !(b is SqlNumber))
				throw new ArgumentException();

			if (b.IsNull)
				return SqlBoolean.Null;

			if (a.IsNull)
				return SqlBoolean.Null;

			var num1 = (SqlNumber) a;
			var num2 = (SqlNumber) b;

			return num1 < num2;
		}

		public override SqlBoolean IsGreatherThan(ISqlObject a, ISqlObject b) {
			if (!(a is SqlNumber) ||
			    !(b is SqlNumber))
				throw new ArgumentException();

			if (b.IsNull)
				return SqlBoolean.Null;

			if (a.IsNull)
				return SqlBoolean.Null;

			var num1 = (SqlNumber) a;
			var num2 = (SqlNumber) b;

			return num1 > num2;
		}

		public override SqlBoolean IsSmallerOrEqualThan(ISqlObject a, ISqlObject b) {
			if (!(a is SqlNumber) ||
			    !(b is SqlNumber))
				throw new ArgumentException();

			if (b.IsNull)
				return SqlBoolean.Null;

			if (a.IsNull)
				return SqlBoolean.Null;

			var num1 = (SqlNumber) a;
			var num2 = (SqlNumber) b;

			return num1 <= num2;
		}

		public override SqlBoolean IsGreaterOrEqualThan(ISqlObject a, ISqlObject b) {
			if (!(a is SqlNumber) ||
			    !(b is SqlNumber))
				throw new ArgumentException();

			if (b.IsNull)
				return SqlBoolean.Null;

			if (a.IsNull)
				return SqlBoolean.Null;

			var num1 = (SqlNumber) a;
			var num2 = (SqlNumber) b;

			return num1 >= num2;
		}

		public override SqlBoolean IsEqualTo(ISqlObject a, ISqlObject b) {
			if (!(a is SqlNumber) ||
			    !(b is SqlNumber))
				throw new ArgumentException();

			if (b.IsNull)
				return a.IsNull;

			if (a.IsNull)
				return b.IsNull;

			var num1 = (SqlNumber) a;
			var num2 = (SqlNumber) b;

			return num1.Equals(num2);
		}

		public override SqlBoolean IsNotEqualTo(ISqlObject a, ISqlObject b) {
			return !IsEqualTo(a, b);
		}

		public override ISqlObject Negate(ISqlObject value) {
			if (!(value is SqlNumber))
				throw new ArgumentException();

			var num = (SqlNumber) value;

			if (num.IsNull)
				return num;

			return num.Negate();
		}

		public override ISqlObject Reverse(ISqlObject value) {
			if (!(value is SqlNumber))
				throw new ArgumentException();

			var num = (SqlNumber)value;

			if (num.IsNull)
				return num;

			return num.Not();
		}

		public override ISqlObject UnaryPlus(ISqlObject value) {
			if (!(value is SqlNumber))
				throw new ArgumentException();

			var num = (SqlNumber)value;

			if (num.IsNull)
				return num;

			return num.Plus();
		}

		public override ISqlObject XOr(ISqlObject a, ISqlObject b) {
			if (!(a is SqlNumber) ||
			    !(b is SqlNumber))
				throw new ArgumentException();

			if (b.IsNull)
				return SqlNumber.Null;

			if (a.IsNull)
				return a;

			var num1 = (SqlNumber) a;
			var num2 = (SqlNumber) b;

			return num1.XOr(num2);
		}

		public override ISqlObject And(ISqlObject a, ISqlObject b) {
			if (!(a is SqlNumber) ||
			    !(b is SqlNumber))
				throw new ArgumentException();

			if (b.IsNull)
				return SqlNumber.Null;

			if (a.IsNull)
				return a;

			var num1 = (SqlNumber) a;
			var num2 = (SqlNumber) b;

			return num1.And(num2);
		}

		public override ISqlObject Or(ISqlObject a, ISqlObject b) {
			if (!(a is SqlNumber) ||
			    !(b is SqlNumber))
				throw new ArgumentException();

			if (b.IsNull)
				return SqlNumber.Null;

			if (a.IsNull)
				return a;

			var num1 = (SqlNumber) a;
			var num2 = (SqlNumber) b;

			return num1.Or(num2);
		}

		public override void SerializeObject(Stream stream, ISqlObject obj) {
			var writer = new BinaryWriter(stream);

			if (obj is SqlNull) {
				writer.Write((byte)0);
			} else {
				var number = (SqlNumber) obj;
				if (obj.IsNull) {
					writer.Write((byte) 0);
				} else if (number.CanBeInt32) {
					writer.Write((byte) 1);
					writer.Write(number.ToInt32());
				} else if (number.CanBeInt64) {
					writer.Write((byte) 2);
					writer.Write(number.ToInt64());
				} else {
					var bytes = number.ToByteArray();
					writer.Write((byte) 3);
					writer.Write(number.Precision);
					writer.Write(number.Scale);
					writer.Write(bytes.Length);
					writer.Write(bytes);
				}
			}
		}

		public override ISqlObject DeserializeObject(Stream stream) {
			var reader = new BinaryReader(stream);

			var type = reader.ReadByte();
			if (type == 0)
				return SqlNumber.Null;

			if (type == 1) {
				var value = reader.ReadInt32();
				return new SqlNumber(value);
			}
			if (type == 2) {
				var value = reader.ReadInt64();
				return new SqlNumber(value);
			}
			if (type == 3) {
				var precision = reader.ReadInt32();
				var scale = reader.ReadInt32();
				var length = reader.ReadInt32();
				var bytes = reader.ReadBytes(length);
				return new SqlNumber(bytes, scale, precision);
			}

			throw new FormatException();
		}

		internal override int ColumnSizeOf(ISqlObject obj) {
			if (obj is SqlNull)
				return 1;

			if (!(obj is SqlNumber))
				throw new ArgumentException(String.Format("Cannot determine the size of an object of type '{0}'.", obj.GetType()));

			var number = (SqlNumber) obj;

			if (number.IsNull)
				return 1;

			if (number.CanBeInt32)
				return 1 + 4;
			if (number.CanBeInt64)
				return 1+ 8;

			// Type + Scale + Precision + Byte Count
			var length = number.ToByteArray().Length;
			return 1 + 4 + 4 + 4 + length;
		}

		internal static bool IsNumericType(SqlTypeCode typeCode) {
			return typeCode == SqlTypeCode.TinyInt ||
			       typeCode == SqlTypeCode.SmallInt ||
			       typeCode == SqlTypeCode.Integer ||
			       typeCode == SqlTypeCode.BigInt ||
			       typeCode == SqlTypeCode.Real ||
			       typeCode == SqlTypeCode.Float ||
			       typeCode == SqlTypeCode.Double ||
			       typeCode == SqlTypeCode.Decimal ||
			       typeCode == SqlTypeCode.Numeric;
		}
	}
}