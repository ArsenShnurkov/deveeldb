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

using Deveel.Data.Sql;

namespace Deveel.Data.Types {
	public sealed class UserType : DataType, IDbObject {
		public UserType(UserTypeInfo typeInfo) 
			: base(typeInfo.TypeName.FullName, SqlTypeCode.Type) {
			if (typeInfo == null)
				throw new ArgumentNullException("typeInfo");

			TypeInfo = typeInfo;
		}

		public UserTypeInfo TypeInfo { get; private set; }

		public ObjectName FullName {
			get { return TypeInfo.TypeName; }
		}

		public override bool IsStorable {
			get { return true; }
		}

		DbObjectType IDbObject.ObjectType {
			get { return DbObjectType.Type; }
		}

		public override bool IsComparable(DataType type) {
			// For the moment not possible to compare
			return false;
		}

		public override bool CanCastTo(DataType destType) {
			return false;
		}

		public override bool IsIndexable {
			get { return false; }
		}
	}
}
