﻿// 
//  Copyright 2012 Deveel
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

using System;

using Deveel.Data.Sql;
using Deveel.Data.Types;

using NUnit.Framework;

namespace Deveel.Data {
	[TestFixture]
	public sealed class TTypeTest {
		[Test]
		public void DecodeNumeric() {
			// TODO: Maybe we should convert this form to INT or NUMERIC
			const string s = "NUMERIC(INTEGER)";
			TType type = TType.DecodeString(s);
			Assert.AreEqual(DbType.Numeric, type.DbType);
			Assert.AreEqual(SqlType.Integer, type.SqlType);
		}
	}
}