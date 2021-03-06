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

using Deveel.Data.DbSystem;

namespace Deveel.Data.Sql.Query {
	class FetchViewNode : IQueryPlanNode {
		public FetchViewNode(ObjectName viewName, ObjectName aliasName) {
			ViewName = viewName;
			AliasName = aliasName;
		}

		public ObjectName ViewName { get; private set; }

		public ObjectName AliasName { get; private set; }

		private IQueryPlanNode CreateChildNode(IQueryContext context) {
			return context.GetViewQueryPlan(ViewName);
		}

		public ITable Evaluate(IQueryContext context) {
			IQueryPlanNode node = CreateChildNode(context);
			var t = node.Evaluate(context);

			return AliasName != null ? new ReferenceTable(t, AliasName) : t;
		}
	}
}
