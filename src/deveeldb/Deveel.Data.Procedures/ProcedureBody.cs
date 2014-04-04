// 
//  Copyright 2010  Deveel
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
using System.Collections;
using System.Collections.Generic;

using Deveel.Data.Sql;

namespace Deveel.Data.Procedures {
	public sealed class ProcedureBody {
		internal ProcedureBody(StoredProcedure procedure) {
			this.procedure = procedure;
			statements = new List<Statement>();
		}

		private readonly StoredProcedure procedure;
		private readonly List<Statement> statements;

		internal void Evaluate(IVariableResolver resolver, ProcedureQueryContext context) {
			throw new NotImplementedException();
		}
	}
}