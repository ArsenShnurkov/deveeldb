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

namespace Deveel.Data.DbSystem {
	/// <summary>
	/// Implementation of a Table that references a DataTable as its
	/// parent.
	/// </summary>
	/// <remarks>
	/// This is a one-to-one relationship unlike <see cref="VirtualTable"/>
	/// which is a one-to-many relationship.
	/// <para>
	/// The entire purpose of this class is as a filter. We can use it to rename
	/// a <see cref="DataTable"/> to any domain we feel like. 
	/// This allows us to generate unique column names.
	/// </para>
	/// <para>
	/// For example, say we need to join the same table. We can use this method
	/// to ensure that the newly joined table won't have duplicate column names.
	/// </para>
	/// </remarks>
	public sealed class ReferenceTable : FilterTable, IRootTable {
		/// <summary>
		/// This represents the new name of the table.
		/// </summary>
		private readonly TableName table_name;

		/// <summary>
		/// The modified DataTableInfo object for this reference.
		/// </summary>
		private readonly DataTableInfo modifiedTableInfo;


		internal ReferenceTable(Table table, TableName tname)
			: base(table) {
			table_name = tname;

			// Create a modified table info based on the parent info.
			modifiedTableInfo = table.TableInfo.Clone(tname);
			modifiedTableInfo.IsReadOnly = true;
		}

		/// <summary>
		/// Constructs the <see cref="ReferenceTable"/> given the parent 
		/// table, and a new <see cref="TableInfo"/> that describes the 
		/// columns in this table.
		/// </summary>
		/// <param name="table"></param>
		/// <param name="info"></param>
		/// <remarks>
		/// This is used if we want to redefine the column names.
		/// <para>
		/// Note that the given DataTableInfo must contain the same number of 
		/// columns as the parent table, and the columns must be the same type.
		/// </para>
		/// </remarks>
		internal ReferenceTable(Table table, DataTableInfo info)
			: base(table) {
			table_name = info.TableName;

			modifiedTableInfo = info;
		}

		/// <summary>
		/// Gets the declared name of the table.
		/// </summary>
		public TableName TableName {
			get { return table_name; }
		}

		/// <inheritdoc/>
		public override DataTableInfo TableInfo {
			get { return modifiedTableInfo; }
		}

		/// <inheritdoc/>
		public override int FindFieldName(VariableName v) {
			TableName tableName = v.TableName;
			if (tableName != null && tableName.Equals(TableName)) {
				return TableInfo.FastFindColumnName(v.Name);
			}
			return -1;
		}

		/// <inheritdoc/>
		public override VariableName GetResolvedVariable(int column) {
			return new VariableName(TableName,
								TableInfo[column].Name);
		}

		/// <inheritdoc/>
		public bool TypeEquals(IRootTable table) {
			return (this == table);
		}
	}
}