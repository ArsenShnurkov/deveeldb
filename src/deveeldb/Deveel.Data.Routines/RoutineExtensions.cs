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
using Deveel.Data.Security;
using Deveel.Data.Sql;
using Deveel.Data.Sql.Expressions;

namespace Deveel.Data.Routines {
	public static class RoutineExtensions {
		public static ExecuteResult Execute(this IRoutine routine) {
			return Execute(routine, new SqlExpression[0]);
		}

		public static ExecuteResult Execute(this IRoutine routine, SqlExpression[] args) {
			return Execute(routine, args, null);
		}

		public static ExecuteResult Execute(this IRoutine routine, SqlExpression[] args, IQueryContext context) {
			return Execute(routine, args, context, null);
		}

		public static ExecuteResult Execute(this IRoutine routine, SqlExpression[] args, IQueryContext context, IVariableResolver resolver) {
			return Execute(routine, args, context, resolver, null);
		}

		public static ExecuteResult Execute(this IRoutine routine, IQueryContext context) {
			return Execute(routine, context, null);
		}

		public static ExecuteResult Execute(this IRoutine routine, IQueryContext context, IVariableResolver resolver) {
			return Execute(routine, context, resolver, null);
		}

		public static ExecuteResult Execute(this IRoutine routine, IQueryContext context, IVariableResolver resolver, IGroupResolver group) {
			return Execute(routine, new SqlExpression[0], context, resolver, group);
		}

		public static ExecuteResult Execute(this IRoutine routine, SqlExpression[] args, IQueryContext context, IVariableResolver resolver, IGroupResolver group) {
			var request = new Invoke(routine.FullName, args);

			if (context != null &&
			    !context.UserCanExecuteFunction(request))
				throw new InvalidOperationException();

			var executeContext = new ExecuteContext(request, routine, resolver, group, context);
			return routine.Execute(executeContext);
		}

		public static ExecuteResult Execute(this IRoutine routine, DataObject[] args) {
			var exps = new SqlExpression[0];
			if (args != null && args.Length > 0) {
				exps = new SqlExpression[args.Length];

				for (int i = 0; i < args.Length; i++) {
					exps[i] = SqlExpression.Constant(args[i]);
				}
			}

			return routine.Execute(exps);
		}
	}
}