﻿using System;
using System.Collections.Generic;
using System.Linq;

using Deveel.Data;
using Deveel.Data.Sql.Tables;
using Deveel.Data.Types;

using NUnit.Framework;

namespace Deveel.Data.Sql.Statements {
	[TestFixture]
	public class CreateViewStatementTests : ContextBasedTest {

		protected override IUserSession CreateAdminSession(IDatabase database) {
			using (var session = base.CreateAdminSession(database)) {
				using (var query = session.CreateQuery()) {
					var tableInfo = new TableInfo(ObjectName.Parse("APP.test_table"));
					tableInfo.AddColumn("a", PrimitiveTypes.Integer());
					tableInfo.AddColumn("b", PrimitiveTypes.String(), false);

					query.CreateTable(tableInfo, false, false);
					query.Commit();
				}
			}

			return base.CreateAdminSession(database);
		}

		[Test]
		public void ParseSimpleCreateView() {
			const string sql = "CREATE VIEW text_view1 AS SELECT * FROM test_table WHERE a = 1";

			var statements = SqlStatement.Parse(sql);
			Assert.IsNotNull(statements);

			var statementList = statements.ToList();
			Assert.IsNotEmpty(statementList);
			Assert.AreEqual(1, statementList.Count);
			Assert.IsInstanceOf<CreateViewStatement>(statementList[0]);

			var createView = (CreateViewStatement) statementList[0];
			Assert.IsNotNull(createView.SourceQuery);
			Assert.IsTrue(createView.IsFromQuery);

			Assert.IsNotNull(createView.ViewName);
		}


		[Test]
		public void ParseCreateViewWithColumns() {
			const string sql = "CREATE VIEW text_view1 (a, b, c) AS SELECT * FROM test_table WHERE a = 1";

			IEnumerable<SqlStatement> statements = null;
			Assert.DoesNotThrow(() => statements = SqlStatement.Parse(sql));
			Assert.IsNotNull(statements);

			var statementList = statements.ToList();
			Assert.IsNotEmpty(statementList);
			Assert.AreEqual(1, statementList.Count);
			Assert.IsInstanceOf<CreateViewStatement>(statementList[0]);

			var createView = (CreateViewStatement)statementList[0];
		}

		[Test]
		public void ParseCreateViewWithOrReplace() {
			const string sql = "CREATE OR REPLACE VIEW text_view1 AS SELECT * FROM test_table WHERE a = 1";

			IEnumerable<SqlStatement> statements = null;
			Assert.DoesNotThrow(() => statements = SqlStatement.Parse(sql));
			Assert.IsNotNull(statements);

			var statementList = statements.ToList();
			Assert.IsNotEmpty(statementList);
			Assert.AreEqual(1, statementList.Count);
			Assert.IsInstanceOf<CreateViewStatement>(statementList[0]);

			var createView = (CreateViewStatement)statementList[0];
		}

		[Test]
		public void ExecuteSimpleCreateView() {
			const string sql = "CREATE VIEW text_view1 AS SELECT * FROM test_table WHERE a = 1";

			IEnumerable<SqlStatement> statements = null;
			Assert.DoesNotThrow(() => statements = SqlStatement.Parse(sql));
			Assert.IsNotNull(statements);

			var list = statements.ToList();

			Assert.AreEqual(1, list.Count);

			var statement = list[0];

			Assert.IsNotNull(statement);
			Assert.IsInstanceOf<CreateViewStatement>(statement);

			ITable result = null;
			Assert.DoesNotThrow(() => result = statement.Execute(Query));
			Assert.IsNotNull(result);
			Assert.AreEqual(1, result.RowCount);
		}
	}
}
