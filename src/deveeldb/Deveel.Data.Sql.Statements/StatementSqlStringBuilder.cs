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
using System.Text;

namespace Deveel.Data.Sql.Statements {
	class StatementSqlStringBuilder {
		private StringBuilder sqlBuilder;

		public StatementSqlStringBuilder() {
			sqlBuilder = new StringBuilder();
		}

		public void Visit(SqlStatement statement) {
			throw new NotImplementedException();
		}

		public override string ToString() {
			return sqlBuilder.ToString();
		}
	}
}
