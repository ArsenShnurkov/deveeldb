﻿using System;

using Deveel.Data.Sql.Schemas;

using NUnit.Framework;

namespace Deveel.Data.Sql.Statements {
	[TestFixture]
	public sealed class CreateSchemaStatementTests : ContextBasedTest {
		[Test]
		public void CreateNewSchema() {
			const string schemaName = "Sch1";
			var statement = new CreateSchemaStatement(schemaName);

			statement.Execute(Query);

			var exists = Query.SchemaExists(schemaName);

			Assert.IsTrue(exists);
		}
	}
}