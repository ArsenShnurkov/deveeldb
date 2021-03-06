﻿using System;
using System.Data;
using System.IO;

using NUnit.Framework;

namespace Deveel.Data.Client {
	[TestFixture]
	public sealed class ConnectionTest {
		[SetUp]
		public void TestSetUp() {
			var testName = TestContext.CurrentContext.Test.Name;
			if (testName == "ConnectToLocal") {
				var dbPath = Path.Combine(Environment.CurrentDirectory, "testdb");
				if (Directory.Exists(dbPath))
					Directory.Delete(dbPath, true);
			}
		}

		[Test]
		public void ConnectToLocal() {
			// since we're connecting locally, we can specify the option
			// 'CreateOrBoot' to tell the system to startup a database or
			// to create it, if not existing...
			// Usernmae and password have to be the administrator's credentials
			// for both the operations..
			const string connString = "Host=Local;User=SA;Password=123456;CreateOrBoot=true;Database=testdb";

			var connection = new DeveelDbConnection(connString);
			Assert.DoesNotThrow(connection.Open);
			Assert.IsTrue(connection.State == ConnectionState.Open);
			Assert.DoesNotThrow(connection.Close);
			Assert.IsTrue(connection.State == ConnectionState.Closed);
		}

		//[Test]
		//public void ConnectToRemote() {
		//	// we connect to 127.0.0.1 which is the IPv4 local loopbak
		//	// but not specifying the "Local" address to the connection
		//	// string triggers the RemoteDatabaseInterface, which is what
		//	// we want to test...

		//	const string connString = "Host=127.0.0.1;User=SA;Password=123456;Database=testdb";

		//	var connection = new DeveelDbConnection(connString);
		//	Assert.DoesNotThrow(connection.Open);
		//	Assert.IsTrue(connection.State == ConnectionState.Broken);
		//	Assert.DoesNotThrow(connection.Close);
		//	Assert.IsTrue(connection.State == ConnectionState.Closed);
		//}
	}
}