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
using Deveel.Data.Sql.Expressions;
using Deveel.Data.Sql.Statements;
using Deveel.Data.Types;

namespace Deveel.Data.Sql.Parser {
	class StatementBuilder : SqlNodeVisitor {
		private readonly IQueryContext context;
		private readonly List<SqlStatement> statements;

		public StatementBuilder() 
			: this(null) {
		}

		public StatementBuilder(IQueryContext context) {
			this.context = context;
			statements = new List<SqlStatement>();
		}

		private SqlExpression Expression(IExpressionNode node) {
			return ExpressionBuilder.Build(node);
		}

		public override void Visit(ISqlNode node) {
			if (node is CreateTableNode)
				VisitCreateTable((CreateTableNode) node);
			if (node is CreateViewNode)
				VisitCreateView((CreateViewNode) node);
			if (node is CreateTriggerNode)
				VisitCreateTrigger((CreateTriggerNode) node);

			if (node is AlterTableNode)
				VisitAlterTable((AlterTableNode) node);

			if (node is SelectStatementNode)
				VisitSelect((SelectStatementNode) node);

			if (node is UpdateStatementNode)
				VisitUpdate((UpdateStatementNode)node);
			if (node is InsertStatementNode)
				VisitInsert((InsertStatementNode) node);

			if (node is SequenceOfStatementsNode)
				VisitSequenceOfStatements((SequenceOfStatementsNode) node);
		}

		private void VisitSequenceOfStatements(SequenceOfStatementsNode node) {
			foreach (var statementNode in node.Statements) {
				Visit(statementNode);
			}
		}

		public override void VisitSelect(SelectStatementNode node) {
			var queryExpression = (SqlQueryExpression) Expression(node.QueryExpression);
			if (node.QueryExpression.IntoClause != null) {
				var refExp = Expression(node.QueryExpression.IntoClause);
				statements.Add(new SelectIntoStatement(queryExpression, refExp));
			} else {
				var orderBy = OrderBy(node.OrderBy);
				var statement = new SelectStatement(queryExpression, orderBy);
				statements.Add(statement);
			}
		}

		private IEnumerable<SortColumn> OrderBy(IEnumerable<OrderByNode> nodes) {
			if (nodes == null)
				return null;

			return nodes.Select(node => new SortColumn(Expression(node.Expression), node.Ascending));
		} 

		public override void VisitCreateTrigger(CreateTriggerNode node) {
			
		}

		public override void VisitCreateView(CreateViewNode node) {
			var queryExpression = (SqlQueryExpression)Expression(node.QueryExpression);
			var statement = new CreateViewStatement(node.ViewName.Name, node.ColumnNames, queryExpression);
			statements.Add(statement);
		}

		public override void VisitCreateTable(CreateTableNode node) {
			CreateTable.Build(context, node, statements);
		}

		public IEnumerable<SqlStatement> Build(ISqlNode rootNode, SqlQuery query) {
			Visit(rootNode);
			return statements.ToArray();
		}

		public override void VisitAlterTable(AlterTableNode node) {
			AlterTable.Build(context, node, statements);
		}

		public IEnumerable<SqlStatement> Build(ISqlNode rootNode, string query) {
			return Build(rootNode, new SqlQuery(query));
		}

		public override void VisitSimpleUpdate(SimpleUpdateNode node) {
			var whereExpression = Expression(node.WhereExpression);
			var assignments = UpdateAssignments(node.Columns);
			statements.Add(new UpdateStatement(node.TableName, whereExpression, assignments));
		}

		private IEnumerable<SqlColumnAssignment> UpdateAssignments(IEnumerable<UpdateColumnNode> columns) {
			if (columns == null)
				return null;

			return columns.Select(column => new SqlColumnAssignment(column.ColumnName, Expression(column.Expression)));
		}

		public override void VisitQueryUpdate(QueryUpdateNode node) {
			base.VisitQueryUpdate(node);
		}

		protected override void VisitValuesInsert(ValuesInsertNode valuesInsert) {
			var values = valuesInsert.Values.Select(x => x.Values.Select(Expression).ToArray());
			statements.Add(new InsertValuesStatement(valuesInsert.TableName, valuesInsert.ColumnNames, values));
		}

		private static SqlTableColumn BuildColumnInfo(IQueryContext context, string tableName, TableColumnNode column, IList<ConstraintInfo> constraints) {
			var objTableName = ObjectName.Parse(tableName);
			var dataTypeBuilder = new DataTypeBuilder();
			var dataType = dataTypeBuilder.Build(context.TypeResolver(), column.DataType);

			var columnInfo = new SqlTableColumn(column.ColumnName.Text, dataType);

			if (column.Default != null)
				columnInfo.DefaultExpression = ExpressionBuilder.Build(column.Default);

			if (column.IsIdentity) {
				columnInfo.DefaultExpression = SqlExpression.FunctionCall("UNIQUEKEY",
					new[] { SqlExpression.Constant(tableName) });
			}

			foreach (var constraint in column.Constraints) {
				if (String.Equals(ConstraintTypeNames.Check, constraint.ConstraintType, StringComparison.OrdinalIgnoreCase)) {
					var exp = ExpressionBuilder.Build(constraint.CheckExpression);
					constraints.Add(ConstraintInfo.Check(objTableName, exp, column.ColumnName.Text));
				} else if (String.Equals(ConstraintTypeNames.ForeignKey, constraint.ConstraintType, StringComparison.OrdinalIgnoreCase)) {
					var fTable = ObjectName.Parse(constraint.ReferencedTable.Name);
					var fColumn = constraint.ReferencedColumn.Text;
					var fkey = ConstraintInfo.ForeignKey(objTableName, column.ColumnName.Text, fTable, fColumn);
					if (!String.IsNullOrEmpty(constraint.OnDeleteAction))
						fkey.OnDelete = GetForeignKeyAction(constraint.OnDeleteAction);
					if (!String.IsNullOrEmpty(constraint.OnUpdateAction))
						fkey.OnUpdate = GetForeignKeyAction(constraint.OnUpdateAction);

					constraints.Add(fkey);
				} else if (String.Equals(ConstraintTypeNames.PrimaryKey, constraint.ConstraintType, StringComparison.OrdinalIgnoreCase)) {
					constraints.Add(ConstraintInfo.PrimaryKey(objTableName, column.ColumnName.Text));
				} else if (String.Equals(ConstraintTypeNames.UniqueKey, constraint.ConstraintType, StringComparison.OrdinalIgnoreCase)) {
					constraints.Add(ConstraintInfo.Unique(objTableName, column.ColumnName.Text));
				} else if (String.Equals(ConstraintTypeNames.NotNull, constraint.ConstraintType, StringComparison.OrdinalIgnoreCase)) {
					columnInfo.IsNotNull = true;
				} else if (String.Equals(ConstraintTypeNames.Null, constraint.ConstraintType, StringComparison.OrdinalIgnoreCase)) {
					columnInfo.IsNotNull = false;
				}
			}

			return columnInfo;
		}

		private static ConstraintInfo BuildConstraint(IQueryContext context, string tableName, TableConstraintNode constraint) {
			var objTableName = ObjectName.Parse(tableName);
			if (String.Equals(ConstraintTypeNames.Check, constraint.ConstraintType, StringComparison.OrdinalIgnoreCase)) {
				var exp = ExpressionBuilder.Build(constraint.CheckExpression);
				return ConstraintInfo.Check(constraint.ConstraintName, objTableName, exp, constraint.Columns.ToArray());
			}
			if (String.Equals(ConstraintTypeNames.PrimaryKey, constraint.ConstraintType, StringComparison.OrdinalIgnoreCase))
				return ConstraintInfo.PrimaryKey(constraint.ConstraintName, objTableName, constraint.Columns.ToArray());
			if (String.Equals(ConstraintTypeNames.UniqueKey, constraint.ConstraintType, StringComparison.OrdinalIgnoreCase))
				return ConstraintInfo.Unique(constraint.ConstraintName, objTableName, constraint.Columns.ToArray());
			if (String.Equals(ConstraintTypeNames.ForeignKey, constraint.ConstraintType, StringComparison.OrdinalIgnoreCase)) {
				var fTable = ObjectName.Parse(constraint.ReferencedTableName.Name);
				var fColumns = constraint.ReferencedColumns;
				var fkey = ConstraintInfo.ForeignKey(constraint.ConstraintName, objTableName, constraint.Columns.ToArray(), fTable,
					fColumns.ToArray());
				if (!String.IsNullOrEmpty(constraint.OnDeleteAction))
					fkey.OnDelete = GetForeignKeyAction(constraint.OnDeleteAction);
				if (!String.IsNullOrEmpty(constraint.OnUpdateAction))
					fkey.OnUpdate = GetForeignKeyAction(constraint.OnUpdateAction);

				return fkey;
			}

			throw new NotSupportedException();
		}

		private static ForeignKeyAction GetForeignKeyAction(string actionName) {
			if (String.Equals("NO ACTION", actionName, StringComparison.OrdinalIgnoreCase) ||
				String.Equals("NOACTION", actionName, StringComparison.OrdinalIgnoreCase))
				return ForeignKeyAction.NoAction;
			if (String.Equals("CASCADE", actionName, StringComparison.OrdinalIgnoreCase))
				return ForeignKeyAction.Cascade;
			if (String.Equals("SET DEFAULT", actionName, StringComparison.OrdinalIgnoreCase) ||
				String.Equals("SETDEFAULT", actionName, StringComparison.OrdinalIgnoreCase))
				return ForeignKeyAction.SetDefault;
			if (String.Equals("SET NULL", actionName, StringComparison.OrdinalIgnoreCase) ||
				String.Equals("SETNULL", actionName, StringComparison.OrdinalIgnoreCase))
				return ForeignKeyAction.SetNull;

			throw new NotSupportedException();
		}

		#region CreateTable

		static class CreateTable {
			public static void Build(IQueryContext context, CreateTableNode node, ICollection<SqlStatement> statements) {
				string idColumn = null;

				var tableName = node.TableName;
				var constraints = new List<ConstraintInfo>();
				var columns = new List<SqlTableColumn>();

				foreach (var column in node.Columns) {
					if (column.IsIdentity) {
						if (!String.IsNullOrEmpty(idColumn))
							throw new InvalidOperationException(String.Format("Table {0} defines already {1} as identity column.",
								node.TableName, idColumn));

						if (column.Default != null)
							throw new InvalidOperationException(String.Format("The identity column {0} cannot have a DEFAULT constraint.",
								idColumn));

						idColumn = column.ColumnName.Text;
					}

					var columnInfo = BuildColumnInfo(context, tableName.Name, column, constraints);

					columns.Add(columnInfo);
				}

				foreach (var constraint in node.Constraints) {
					var constraintInfo = BuildConstraint(context, tableName.Name, constraint);
					statements.Add(new AlterTableStatement(tableName.Name, new AddConstraintAction(constraintInfo)));
				}

				//TODO: Optimization: merge same constraints

				statements.Add(MakeCreateTable(tableName.Name, columns, node.IfNotExists, node.Temporary));

				foreach (var constraint in constraints) {
					statements.Add(MakeAlterTableAddConstraint(tableName.Name, constraint));
				}
			}

			private static SqlStatement MakeAlterTableAddConstraint(string tableName, ConstraintInfo constraint) {
				var action = new AddConstraintAction(constraint);

				return new AlterTableStatement(tableName, action);
			}

			private static SqlStatement MakeCreateTable(string tableName, IEnumerable<SqlTableColumn> columns, bool ifNotExists, bool temporary) {
				var tree = new CreateTableStatement(tableName, columns.ToList());
				tree.IfNotExists = ifNotExists;
				tree.Temporary = temporary;
				return tree;
			}
		}

		#endregion

		#region AlterTable

		static class AlterTable {
			public static void Build(IQueryContext context, AlterTableNode node, ICollection<SqlStatement> statements) {
				if (node.CreateTable != null) {
					CreateTable.Build(context, node.CreateTable, statements);
					foreach (var statement in statements) {
						if (statement is CreateTableStatement)
							((CreateTableStatement) statement).IfNotExists = true;
					}
				} else if (node.Actions != null) {
					foreach (var action in node.Actions) {
						BuildAction(context, node.TableName, action, statements);
					}
				}
			}

			private static void BuildAction(IQueryContext context, string tableName, IAlterActionNode action, ICollection<SqlStatement> statements) {
				if (action is AddColumnNode) {
					var column = ((AddColumnNode) action).Column;
					var constraints = new List<ConstraintInfo>();
					var columnInfo = BuildColumnInfo(context, tableName, column, constraints);

					statements.Add(new AlterTableStatement(tableName, new AddColumnAction(columnInfo)));

					foreach (var constraint in constraints) {
						statements.Add(new AlterTableStatement(tableName, new AddConstraintAction(constraint)));
					}
				} else if (action is AddConstraintNode) {
					var constraint = ((AddConstraintNode) action).Constraint;

					var constraintInfo = BuildConstraint(context, tableName, constraint);
					statements.Add(new AlterTableStatement(tableName, new AddConstraintAction(constraintInfo)));
				} else if (action is DropColumnNode) {
					var columnName = ((DropColumnNode) action).ColumnName;
					statements.Add(new AlterTableStatement(tableName, new DropColumnAction(columnName)));
				} else if (action is DropConstraintNode) {
					var constraintName = ((DropConstraintNode) action).ConstraintName;
					statements.Add(new AlterTableStatement(tableName, new DropConstraintAction(constraintName)));
				} else if (action is SetDefaultNode) {
					var actionNode = ((SetDefaultNode) action);
					var columnName =actionNode.ColumnName;
					var expression = ExpressionBuilder.Build(actionNode.Expression);
					statements.Add(new AlterTableStatement(tableName, new SetDefaultAction(columnName, expression)));
				} else if (action is DropDefaultNode) {
					var columnName = ((DropDefaultNode) action).ColumnName;
					statements.Add(new AlterTableStatement(tableName, new DropDefaultAction(columnName)));
				} else if (action is AlterColumnNode) {
					var column = ((AlterColumnNode) action).Column;
					var constraints = new List<ConstraintInfo>();
					var columnInfo = BuildColumnInfo(context, tableName, column, constraints);

					// CHECK: Here we do a drop and add column: is there a better way on the back-end?
					statements.Add(new AlterTableStatement(tableName, new DropColumnAction(columnInfo.ColumnName)));

					statements.Add(new AlterTableStatement(tableName, new AddColumnAction(columnInfo)));

					foreach (var constraint in constraints) {
						statements.Add(new AlterTableStatement(tableName, new AddConstraintAction(constraint)));
					}
				}
			}
		}

		#endregion
	}
}
