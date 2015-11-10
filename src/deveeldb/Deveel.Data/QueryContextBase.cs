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
#if !PCL
using System.Security.Cryptography;
#endif

using Deveel.Data.Caching;
using Deveel.Data.Security;
using Deveel.Data.Sql;
using Deveel.Data.Sql.Cursors;
using Deveel.Data.Sql.Objects;
using Deveel.Data.Sql.Variables;
using Deveel.Data.Types;

namespace Deveel.Data {
	abstract class QueryContextBase : IQueryContext, IVariableScope {
#if PCL
		private Random secureRandom;
#else
		private RNGCryptoServiceProvider secureRandom;
#endif
		private ICache tableCache;
		private bool disposed;

		protected QueryContextBase(IUserSession session) {
			if (session == null)
				throw new ArgumentNullException("session");

#if PCL
			secureRandom = new Random();
#else
			secureRandom = new RNGCryptoServiceProvider();
#endif
			Session = session;
			tableCache = new MemoryCache();
			VariableManager = new VariableManager(this);
			CursorManager = new CursorManager(this);

			InitUserManager();
		}

		~QueryContextBase() {
			Dispose(false);
		}


		public IDatabaseContext DatabaseContext {
			get { return Session.Database.DatabaseContext; }
		}

		public VariableManager VariableManager { get; private set; }

		public CursorManager CursorManager { get; private set; }

		private bool OwnsUserManager { get; set; }

		public IUserManager UserManager { get; private set; }

		public IUserSession Session {get; private set; }

		public virtual string CurrentSchema {
			get { return Session.CurrentSchema; }
		}

		public virtual ICache TableCache {
			get { return tableCache; }
		}

		public virtual IQueryContext ParentContext {
			get { return null; }
		}

		private void AssertNotDisposed() {
			if (disposed)
				throw new ObjectDisposedException("QueryContext", "The query context was disposed.");
		}

		private void InitUserManager() {
			var userManager = DatabaseContext.SystemContext.ResolveService<IUserManager>();
			if (userManager == null) {
				userManager = new UserManager(this);
				OwnsUserManager = true;
			}

			UserManager = userManager;
		}

		public virtual SqlNumber NextRandom(int bitSize) {
			AssertNotDisposed();

#if PCL
			var num = secureRandom.NextDouble();
#else
			var bytes = new byte[8];
			secureRandom.GetBytes(bytes);
			var num = BitConverter.ToInt64(bytes, 0);
#endif
			return new SqlNumber(num);
		}

		public void Dispose() {
			Dispose(true);			
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing) {
			if (!disposed) {
				if (disposing) {
					if (VariableManager != null)
						VariableManager.Dispose();
					if (CursorManager != null)
						CursorManager.Dispose();

					if (OwnsUserManager &&
						UserManager != null)
						UserManager.Dispose();
				}

				tableCache = null;
				VariableManager = null;
				CursorManager = null;
				secureRandom = null;
				Session = null;

				disposed = true;
			}
		}

		DataObject IVariableResolver.Resolve(ObjectName variable) {
			AssertNotDisposed();

			throw new NotImplementedException();
		}

		SqlType IVariableResolver.ReturnType(ObjectName variableName) {
			AssertNotDisposed();

			throw new NotImplementedException();
		}

		void IVariableScope.OnVariableDefined(Variable variable) {
			OnVariableDefined(variable);
		}

		void IVariableScope.OnVariableDropped(Variable variable) {
			OnVariableDropped(variable);
		}

		Variable IVariableScope.OnVariableGet(string name) {
			return OnVariableGet(name);
		}

		protected virtual Variable OnVariableGet(string name) {
			return null;
		}

		protected virtual void OnVariableDropped(Variable variable) {
		}

		protected virtual void OnVariableDefined(Variable variable) {
		}
	}
}