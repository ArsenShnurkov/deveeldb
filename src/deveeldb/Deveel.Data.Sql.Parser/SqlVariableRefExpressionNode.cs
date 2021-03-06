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

namespace Deveel.Data.Sql.Parser {
	/// <summary>
	/// References a variable within a SQL execution context.
	/// </summary>
	class SqlVariableRefExpressionNode : SqlNode, IExpressionNode {
		internal SqlVariableRefExpressionNode() {
		}

		/// <summary>
		/// Gets the name of the variable to reference.
		/// </summary>
		public string Variable { get; private set; }

		/// <inheritdoc/>
		protected override ISqlNode OnChildNode(ISqlNode node) {
			if (node is IdentifierNode) {
				Variable = ((IdentifierNode) node).Text;
			}

			return base.OnChildNode(node);
		}
	}
}