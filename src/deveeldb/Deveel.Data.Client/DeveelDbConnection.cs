﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

using Deveel.Data.Protocol;
using Deveel.Data.Sql;
using Deveel.Data.Types;

namespace Deveel.Data.Client {
	// TODO:
	public sealed class DeveelDbConnection : DbConnection {
		private DeveelDbConnectionStringBuilder connectionString;

		private ConnectionState state;
		private ConnectionState oldState;
		private readonly object stateLock = new object();

		private string serverVersion;
		private string dataSource;

		private Dictionary<string, DeveelDbDataReader> openReaders; 

		internal DeveelDbConnection(IClientConnector connector) {
			Client = new ConnectionClient(connector);
			RowCache = new LocalRowCache(this);
		}

		private ConnectionClient Client { get; set; }

		internal LocalRowCache RowCache { get; private set; }

		public new DeveelDbTransaction BeginTransaction(IsolationLevel isolationLevel) {
			if (Transaction != null)
				throw new DeveelDbException("A transaction is already open on this connection.");

			if (isolationLevel == IsolationLevel.Unspecified)
				isolationLevel = IsolationLevel.Serializable;

			if (isolationLevel != IsolationLevel.Serializable)
				throw new NotSupportedException(String.Format("Isolation Level '{0}' is not supported yet.", isolationLevel));

			var commitId = BeginServerTransaction(isolationLevel);
			return new DeveelDbTransaction(this, isolationLevel, commitId);
		}

		protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel) {
			return BeginTransaction(isolationLevel);
		}

		public override void Close() {
			lock (this) {
				try {
					if (State == ConnectionState.Closed)
						return;

					if (openReaders != null) {
						foreach (var reader in openReaders.Values) {
							reader.Close();
						}

						openReaders.Clear();
						openReaders = null;
					}


					if (Transaction != null) {
						Transaction.Rollback();
						Transaction = null;
					}

					Client.Disconnect();
				} catch (Exception ex) {
					throw new DeveelDbException("An error occurred while closing the connection.", ex);
				} finally {
					ChangeState(ConnectionState.Closed);
				}
			}
		}

		public override void ChangeDatabase(string databaseName) {
			throw new NotImplementedException();
		}

		public override void Open() {
			lock (this) {
				if (State == ConnectionState.Open)
					return;

				if (State != ConnectionState.Closed)
					return;

				try {
					ChangeState(ConnectionState.Connecting);

					Client.Connect();
					Client.Authenticate();

					serverVersion = Client.ServerVersion;
				} catch (Exception ex) {
					ChangeState(ConnectionState.Broken);

					// TODO: throw a specialized exception
					throw;
				}

				ChangeState(ConnectionState.Open);
			}
		}

		public override string ConnectionString {
			get { return Settings.ToString(); }
			set { Settings = new DeveelDbConnectionStringBuilder(value); }
		}

		internal DeveelDbConnectionStringBuilder Settings {
			get { return connectionString; }
			set {
				AssertClosed();
				connectionString = value;
				// TODO: Re-init the connection
			}
		}

		public override string Database {
			get { return Settings.Database; }
		}

		public override ConnectionState State {
			get {
				lock (stateLock) {
					return state;
				}
			}
		}

		public override string DataSource {
			get {
				if (String.IsNullOrEmpty(dataSource)) {
					if (String.IsNullOrEmpty(connectionString.DataSource) &&
						!String.IsNullOrEmpty(connectionString.Host)) {
						dataSource = String.Format("{0}:{1}", connectionString.Host, connectionString.Port);
					} else {
						dataSource = connectionString.DataSource;
					}
				}

				return dataSource;
			}
		}

		public override string ServerVersion {
			get { return serverVersion; }
		}

		public override int ConnectionTimeout {
			get { return Settings.QueryTimeout; }
		}

		internal DeveelDbTransaction Transaction { get; set; }

		protected override DbCommand CreateDbCommand() {
			return CreateCommand();
		}

		public new DeveelDbCommand CreateCommand() {
			return new DeveelDbCommand(this);
		}

		private void AssertClosed() {
			if (state != ConnectionState.Closed)
				throw new InvalidOperationException("The connection is not closed.");
		}

		private void AssertFetching() {
			if (state != ConnectionState.Fetching)
				throw new InvalidOperationException("The connection is not fetching data.");
		}

		internal void ChangeState(ConnectionState newState) {
			lock (stateLock) {
				if (state != newState)
					OnStateChange(new StateChangeEventArgs(state, newState));

				oldState = state;
				state = newState;
			}
		}

		internal void EndState() {
			ChangeState(oldState);
		}

		private int BeginServerTransaction(IsolationLevel isolationLevel) {
			lock (this) {
				try {
					return Client.BeginTransaction(isolationLevel);
				} catch (Exception ex) {
					throw new DeveelDbException("Could not begin a transaction.", ex);
				}
			}
		}

		internal void CommitTransaction(int commitId) {
			lock (this) {
				try {
					Client.CommitTransaction(commitId);
				} catch (Exception ex) {
					throw new DeveelDbException(String.Format("Could not COMMIT transaction '{0}' on the server.", commitId), ex);
				}	
			}
		}

		internal void RollbackTransaction(int commitId) {
			lock (this) {
				try {
					Client.RollbackTransaction(commitId);
				} catch (Exception ex) {
					throw new DeveelDbException(String.Format("Could not ROLLBACK transaction '{0}' on the server.", commitId), ex);
				}				
			}
		}

		internal IQueryResponse[] ExecuteQuery(int commitId, SqlQuery query) {
			try {
				return Client.ExecuteQuery(commitId, query);
			} catch (Exception ex) {
				throw new DeveelDbException("An error occurred while executing a query.", ex);
			}
		}

		internal QueryResultPart RequestResultPart(int resultId, int rowIndex, int rowCount) {
			try {
				return Client.GetResultPart(resultId, rowIndex, rowCount);
			} catch (Exception ex) {
				throw new DeveelDbException(String.Format("Could not retrieve part of the result '{0}' from the server.", resultId), ex);
			}
		}

		internal void DisposeResult(int resultId) {
			try {
				Client.DisposeResult(resultId);
			} catch (Exception ex) {
				throw new DeveelDbException(String.Format("The remote result '{0}' could not be disposed.", resultId), ex);
			}
		}

		protected override void Dispose(bool disposing) {
			if (disposing) {
				Close();

				if (Client != null)
					Client.Dispose();
			}

			Client = null;

			base.Dispose(disposing);
		}
	}
}
