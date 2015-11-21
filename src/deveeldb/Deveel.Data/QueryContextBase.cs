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
using System.Collections;
#if !PCL
using System.Security.Cryptography;
#endif

using Deveel.Data.Caching;
using Deveel.Data.Services;
using Deveel.Data.Sql;
using Deveel.Data.Sql.Cursors;
using Deveel.Data.Sql.Objects;
using Deveel.Data.Sql.Variables;
using Deveel.Data.Types;

namespace Deveel.Data {
	abstract class QueryContextBase : Context, IQueryContext, IVariableScope {
#if PCL
		private Random secureRandom;
#else
		private RNGCryptoServiceProvider secureRandom;
#endif
		private ICache tableCache;
		private bool disposed;

		protected QueryContextBase(IUserSession session)
			: base(session.SessionContext) {
			if (session == null)
				throw new ArgumentNullException("session");

			this.RegisterInstance<IQueryContext>(this);
#if PCL
			secureRandom = new Random();
#else
			secureRandom = new RNGCryptoServiceProvider();
#endif
			Session = session;
			tableCache = new MemoryCache();
			VariableManager = new VariableManager(this);
			CursorManager = new CursorManager(this);
		}

		IQueryContext IQueryContext.ParentContext {
			get { return ParentQueryContext; }
		}
		public virtual IQueryContext ParentQueryContext {
			get { return null; }
		}

	    public ISessionContext SessionContext {
	        get { return Session.SessionContext; }
	    }

		public VariableManager VariableManager { get; private set; }

		public CursorManager CursorManager { get; private set; }

		public IUserSession Session {get; private set; }

		public virtual string CurrentSchema {
			get { return Session.CurrentSchema; }
		}

		public virtual ICache TableCache {
			get { return tableCache; }
		}

		protected override string ContextName {
			get { return ContextNames.Query;  }
		}

		private void AssertNotDisposed() {
			if (disposed)
				throw new ObjectDisposedException("QueryContext", "The query context was disposed.");
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

		protected override void Dispose(bool disposing) {
			if (!disposed) {
				if (disposing) {
					if (VariableManager != null)
						VariableManager.Dispose();
					if (CursorManager != null)
						CursorManager.Dispose();
				}

				tableCache = null;
				VariableManager = null;
				CursorManager = null;
				secureRandom = null;
				Session = null;

				disposed = true;
			}

			base.Dispose(true);
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

		//object IResolveScope.OnBeforeResolve(Type serviceType, string name) {
		//	throw new NotImplementedException();
		//}

		//void IResolveScope.OnAfterResolve(Type serviceType, string name, object obj) {
		//	throw new NotImplementedException();
		//}

		//IEnumerable IResolveScope.OnBeforeResolveAll(Type serviceType) {
		//	throw new NotImplementedException();
		//}

		//void IResolveScope.OnAfterResolveAll(Type serviceType, IEnumerable list) {
		//	throw new NotImplementedException();
		//}
	}
}