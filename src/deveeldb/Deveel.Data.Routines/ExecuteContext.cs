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
using System.Collections.Generic;
using System.Linq;

using Deveel.Data.DbSystem;
using Deveel.Data.Sql;
using Deveel.Data.Sql.Expressions;

namespace Deveel.Data.Routines {
	/// <summary>
	/// Encapsulates the 
	/// </summary>
	public sealed class ExecuteContext : IDisposable {
		private DataObject[] evaluatedArgs;
		private Dictionary<string, DataObject> output; 

		internal ExecuteContext(Invoke invoke, IRoutine routine, IVariableResolver resolver, IGroupResolver group, IQueryContext queryContext) {
			if (invoke == null)
				throw new ArgumentNullException("invoke");
			if (routine == null)
				throw new ArgumentNullException("routine");

			QueryContext = queryContext;
			GroupResolver = group;
			VariableResolver = resolver;
			Invoke = invoke;
			Routine = routine;
		}

		~ExecuteContext() {
			Dispose(false);
		}

		/// <summary>
		/// Gets the information about the invoke of the routine.
		/// </summary>
		/// <seealso cref="Routines.Invoke"/>
		public Invoke Invoke { get; private set; }

		/// <summary>
		/// Gets the instance of the <see cref="IRoutine"/> being executed.
		/// </summary>
		/// <seealso cref="IRoutine"/>
		public IRoutine Routine { get; private set; }

		/// <summary>
		/// Gets the type of the routine being executed.
		/// </summary>
		/// <seealso cref="RoutineType"/>
		/// <seealso cref="IRoutine.Type"/>
		public RoutineType RoutineType {
			get { return Routine.Type; }
		}

		/// <summary>
		/// Gets the array of expressions forming the arguments of the execution.
		/// </summary>
		/// <seealso cref="SqlExpression"/>
		/// <seealso cref="Routines.Invoke.Arguments"/>
		/// <seealso cref="Invoke"/>
		public SqlExpression[] Arguments {
			get { return Invoke.Arguments; }
		}

		public IVariableResolver VariableResolver { get; private set; }

		public IGroupResolver GroupResolver { get; private set; }

		public IQueryContext QueryContext { get; private set; }

		public DataObject[] EvaluatedArguments {
			get {
				if (evaluatedArgs == null) {
					evaluatedArgs = new DataObject[Arguments.Length];
					for (int i = 0; i < Arguments.Length; i++) {
						evaluatedArgs[i] = Arguments[i].EvaluateToConstant(QueryContext, VariableResolver);
					}
				}

				return evaluatedArgs;
			}
		}

		/// <summary>
		/// Gets a count of the arguments passed to the routine.
		/// </summary>
		/// <seealso cref="Arguments"/>
		public int ArgumentCount {
			get { return Arguments == null ? 0 : Arguments.Length; }
		}

		public bool TryGetArgument(string argName, out SqlExpression value) {
			var parameter = Routine.RoutineInfo.Parameters.FirstOrDefault(x => x.Name.Equals(argName, StringComparison.Ordinal));
			if (parameter == null) {
				value = null;
				return false;
			}

			var offset = parameter.Offset;
			if (parameter.IsUnbounded) {
				// TODO: Copy all arguments starting at the given offset and make an Array
				var tuple = SqlExpression.Tuple(new SqlExpression[0]);
				value = tuple;
			} else {
				value = Invoke.Arguments[offset];
			}

			return true;
		}

		public bool TryGetArgument(string argName, out DataObject value) {
			SqlExpression exp;
			if (!TryGetArgument(argName, out exp)) {
				value = DataObject.Null();
				return false;
			}

			value = exp.EvaluateToConstant(QueryContext, VariableResolver);
			return true;
		}

		public bool HasArgument(string argName) {
			throw new NotImplementedException();
		}

		public ExecuteResult Result(DataObject value) {
			if (Routine.Type != RoutineType.Function)
				throw new InvalidOperationException("The routine is not a function.");

			return new ExecuteResult(this, value);
		}

		public ExecuteResult Result() {
			if (Routine.Type != RoutineType.Procedure)
				throw new InvalidOperationException("The routine is not a procedure: a return value is required.");

			return new ExecuteResult(this);
		}

		private RoutineParameter GetParameter(string name) {
			return Routine.RoutineInfo.Parameters.FirstOrDefault(x => x.Name.Equals(name, StringComparison.Ordinal));
		}

		public void SetOutput(string argName, DataObject value) {
			var parameter = GetParameter(argName);
			if (parameter == null)
				throw new InvalidOperationException(String.Format("Routine {0} has none parameter named '{1}'.", Routine.FullName, argName));
			if (!parameter.IsOutput)
				throw new InvalidOperationException();

			if (!parameter.IsNullable &&
				value.IsNull)
				throw new ArgumentException();

			if (!parameter.IsUnbounded)
				throw new ArgumentException();

			if (!parameter.Type.IsComparable(value.Type))
				throw new ArgumentException();

			if (output == null)
				output = new Dictionary<string, DataObject>();

			output[argName] = value;
		}

		private void Dispose(bool disposing) {
			Routine = null;
			Invoke = null;
			evaluatedArgs = null;
			output = null;
		}

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}