﻿using System;
using System.Data;

using NUnit.Framework;

namespace Deveel.Data.Client {
	[TestFixture]
	public class CommandTest {
		[Test]
		public void CreateCommandWithMarkerParameters() {
			const string connString = "Host=Heap;UserID=SA;Password=123456;Database=testdb;Parameter Style=Marker;BootOrCreate=true";
			var connection = new DeveelDbConnection(connString);
			Assert.DoesNotThrow(connection.Open);

			var command = connection.CreateCommand();

			command.CommandText = "CREATE TABLE Person (Id Identity, Name VARCHAR NOT NULL, LastName VarChar)";
			var result = command.ExecuteNonQuery();

			Assert.AreEqual(0, result);

			command.CommandText = "SELECT * FROM Person WHERE Name = ?";
			Assert.DoesNotThrow(() => command.Parameters.Add("antonelllo"));

			object value = null;
			Assert.DoesNotThrow(() => value = command.ExecuteScalar());
			Assert.IsNull(value);

			command.CommandText = "DROP TABLE Person";
			command.ExecuteNonQuery();
		}

		[TestCase(":Name", "antonello")]
		[TestCase("Name", "antonello")]
		public void CreateCommandWithNamedParameters(string paramName, string paramValue) {
			const string connString = "Host=Heap;UserID=SA;Password=123456;Database=testdb;Parameter Style=Named;BootOrCreate=true";
			var connection = new DeveelDbConnection(connString);
			Assert.DoesNotThrow(connection.Open);

			var command = connection.CreateCommand();

			command.CommandText = "CREATE TABLE Person (Id Identity, Name VARCHAR NOT NULL, LastName VarChar)";
			var result = command.ExecuteNonQuery();

			command.CommandText = String.Format("SELECT * FROM Person WHERE Name = {0}", paramName);
			Assert.DoesNotThrow(() => command.Parameters.Add(paramName, paramValue));

			DeveelDbDataReader reader = null;
			Assert.DoesNotThrow(() => reader = command.ExecuteReader());
			Assert.IsNotNull(reader);
			reader.Close();

			command.CommandText = "DROP TABLE Person";
			command.ExecuteNonQuery();
		}

		[Test]
		public void CreateCommandWithMixedParameters() {
			// by default "Parameter Style" configuration is set to "Marker" in connection strings
			const string connString = "Host=Heap;UserID=SA;Password=123456;Database=testdb;BootOrCreate=true";
			var connection = new DeveelDbConnection(connString);
			Assert.DoesNotThrow(connection.Open);

			var command = connection.CreateCommand();
			command.CommandText = "SELECT * FROM Person WHERE Name = ?";
			Assert.DoesNotThrow(() => command.Parameters.Add("antonelllo"));
			Assert.DoesNotThrow(() => command.Parameters.Add("Name", "antonello"));
			Assert.Throws<InvalidOperationException>(() => command.ExecuteReader());
		}

		[Test]
		public void CreateCommandOnClosedConnection() {
			const string connString = "Host=Heap;UserID=SA;Password=123456;Database=testdb;BootOrCreate=true";
			var connection = new DeveelDbConnection(connString);
			DeveelDbCommand command = null;
			Assert.DoesNotThrow(() => command = connection.CreateCommand());
			Assert.IsNotNull(command);
			command.CommandText = "SELECT * FROM Person WHERE Name = 'antonello'";
		}

		[Test]
		public void ExecuteCommandOnClosedConnection() {
			const string connString = "Host=Heap;UserID=SA;Password=123456;Database=testdb;BootOrCreate=true";
			var connection = new DeveelDbConnection(connString);
			DeveelDbCommand command = null;
			Assert.DoesNotThrow(() => command = connection.CreateCommand());
			Assert.IsNotNull(command);

			command.CommandText = "SELECT * FROM Person WHERE Name = 'antonello'";

			Assert.IsTrue(connection.State == ConnectionState.Closed);
			Assert.Throws<DeveelDbException>(() => command.ExecuteNonQuery());
		}

		[Test]
		public void ExecuteScalarOnsingleColumn() {
			const string connString = "Host=Heap;UserID=SA;Password=123456;Database=testdb;BootOrCreate=true";
			var connection = new DeveelDbConnection(connString);
			connection.Open();

			// TODO: Open the connection, create a transaction, declare some variables

			DeveelDbCommand command = null;
			Assert.DoesNotThrow(() => command = connection.CreateCommand());
			Assert.IsNotNull(command);
			command.CommandText = "SELECT Age FROM Person WHERE Name = 'antonello'";
		}

		[Test]
		public void ExecuteScalarOnMultipleColumns() {
			Assert.Inconclusive();
		}


		[Test]
		public void ExecuteMultipleStatements() {
			const string connString = "Host=Heap;UserID=SA;Password=123456;Database=testdb;BootOrCreate=true";
			var connection = new DeveelDbConnection(connString);

			Assert.DoesNotThrow(connection.Open);
			Assert.IsTrue(connection.State == ConnectionState.Open);

			DeveelDbCommand command = connection.CreateCommand();
			command.CommandText = "DECLARE firstName STRING NOT NULL; SET firstName = 'antonello'; SELECT :firstName;";
			var reader = command.ExecuteReader();
			Assert.IsTrue(reader.NextResult());
			Assert.IsTrue(reader.NextResult());
			Assert.IsTrue(reader.Read());
			Assert.IsFalse(reader.IsDBNull(0));
			Assert.AreEqual("antonello", reader.GetString(0));
			connection.Close();
		}
	}
}