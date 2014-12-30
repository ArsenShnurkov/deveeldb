﻿// 
//  Copyright 2010-2014 Deveel
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
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

namespace Deveel.Data.Sql.Statements {
	[TestFixture]
	public class SelectStatementTests {
		[Test]
		public void ParseWithFromClause() {
			const string sql = "SELECT col1 AS a FROM table";

			IEnumerable<Statement> statements = null;
			Assert.DoesNotThrow(() => statements = Statement.Parse(sql));
			Assert.IsNotNull(statements);

			var statement = statements.FirstOrDefault();

			Assert.IsNotNull(statement);
			Assert.IsInstanceOf<SelectStatement>(statement);

			var selectStatement = (SelectStatement) statement;

		}
	}
}