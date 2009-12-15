//  
//  TIntervalType.cs
//  
//  Author:
//       Antonello Provenzano <antonello@deveel.com>
// 
//  Copyright (c) 2009 Deveel
// 
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
// 
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
// 
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;

namespace Deveel.Data {
	public sealed class TIntervalType : TType {
		public TIntervalType(SqlType type)
			: base(type) {
		}

		public override int Compare(object x, object y) {
			Interval a = (Interval) x;
			Interval b = (Interval) y;

			return a.CompareTo(b);
		}

		public override bool IsComparableType(TType ttype) {
			return (ttype is TIntervalType);
		}

		public override int CalculateApproximateMemoryUse(object ob) {
			return 4 + (6 * 4);
		}

		public override Type GetObjectType() {
			return typeof (Interval);
		}
	}
}