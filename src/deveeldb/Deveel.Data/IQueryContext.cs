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

using Deveel.Data.Functions;

namespace Deveel.Data {
	/// <summary>
	/// Facts about a particular query including the root table sources, user name
	/// of the controlling context, sequence state, etc.
	/// </summary>
	public interface IQueryContext {
		/// <summary>
		/// Returns a TransactionSystem object that is used to determine information
		/// about the transactional system.
		/// </summary>
		TransactionSystem System { get; }

		/// <summary>
		/// Returns the user name of the connection.
		/// </summary>
		string UserName { get; }

		/// <summary>
		/// Returns a <see cref="FunctionLookup"/> object used to convert 
		/// <see cref="FunctionDef"/> objects to <see cref="IFunction"/> objects 
		/// when evaluating an expression.
		/// </summary>
		IFunctionLookup FunctionLookup { get; }

		// ---------- Sequences ----------

		/// <summary>
		/// Increments the sequence generator and returns the next unique key.
		/// </summary>
		/// <param name="generator_name"></param>
		/// <returns></returns>
		long NextSequenceValue(String generator_name);

		/// <summary>
		/// Returns the current sequence value returned for the given sequence
		/// generator within the connection defined by this context.
		/// </summary>
		/// <param name="generator_name"></param>
		/// <returns></returns>
		/// <exception cref="StatementException">
		/// If a value was not returned for this connection.
		/// </exception>
		long CurrentSequenceValue(String generator_name);

		/// <summary>
		/// Sets the current sequence value for the given sequence generator.
		/// </summary>
		/// <param name="generator_name"></param>
		/// <param name="value"></param>
		void SetSequenceValue(String generator_name, long value);

		// ---------- Caching ----------

		/// <summary>
		/// Marks a table in a query plan.
		/// </summary>
		/// <param name="mark_name"></param>
		/// <param name="table"></param>
		/// <seealso cref="GetMarkedTable"/>
		void AddMarkedTable(String mark_name, Table table);

		/// <summary>
		/// Returns a table that was marked in a query plan or null if no 
		/// mark was found.
		/// </summary>
		/// <param name="mark_name"></param>
		/// <returns></returns>
		/// <seealso cref="AddMarkedTable"/>
		Table GetMarkedTable(String mark_name);

		/// <summary>
		/// Put a <see cref="Table"/> into the cache.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="table"></param>
		/// <seealso cref="GetCachedNode"/>
		void PutCachedNode(long id, Table table);

		/// <summary>
		/// Returns a cached table or null if it isn't cached.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		/// <seealso cref="PutCachedNode"/>
		Table GetCachedNode(long id);

		/// <summary>
		/// Clears the cache of any cached tables.
		/// </summary>
		/// <seealso cref="PutCachedNode"/>
		/// <seealso cref="GetCachedNode"/>
		void ClearCache();

		// -------------- Variables ----------------

		/// <summary>
		/// Declares a variable, identified by the given name,
		/// within the current query context.
		/// </summary>
		/// <param name="name">The name of the variable to declare.</param>
		/// <param name="type">The type of the variable.</param>
		/// <param name="constant">A flag indicating whether the variable
		/// is <c>constant</c> (default value is required and cannot be 
		/// changed).</param>
		/// <param name="notNull">A flag indicating whether the variable
		/// can be set to <c>null</c>.</param>
		/// <returns>
		/// Returns a reference to the <see cref="Variable"/> object created.
		/// </returns>
		Variable DeclareVariable(string name, TType type, bool constant, bool notNull);

		/// <summary>
		/// Gets a declared variable with the given name.
		/// </summary>
		/// <param name="name">The name of the variable
		/// to return.</param>
		/// <returns>
		/// Returns a declared <see cref="Variable"/> having
		/// the given name or <c>null</c> if no variables were 
		/// found for the given name.
		/// </returns>
		Variable GetVariable(string name);

		/// <summary>
		/// Sets the value of a variable identified by the
		/// given name.
		/// </summary>
		/// <param name="name">The name of the variable to set.</param>
		/// <param name="value">The value to set for the variable.</param>
		void SetVariable(string name, Expression value);

		// -------------- Cursors ----------------

		Cursor DeclareCursor(TableName name, IQueryPlanNode planNode, CursorAttributes attributes);

		Cursor GetCursror(TableName name);

		void OpenCursor(TableName name);

		void CloseCursor(TableName name);
	}
}