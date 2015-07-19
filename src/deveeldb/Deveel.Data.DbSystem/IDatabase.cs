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

using Deveel.Data.Diagnostics;
using Deveel.Data.Protocol;
using Deveel.Data.Security;
using Deveel.Data.Sql;
using Deveel.Data.Transactions;

namespace Deveel.Data.DbSystem {
	public interface IDatabase : ITransactionContext, IEventSource, IDisposable {
		IDatabaseContext Context { get; }

		Version Version { get; }

		ActiveSessionList ActiveSessions { get; }

		bool Exists { get; }

		bool IsOpen { get; }

		ITable SingleRowTable { get; }


		void Create(string adminName, string adminPassword);

		void Open();

		void Close();

		IUserSession CreateSession(SessionInfo sessionInfo);

		IUserSession OpenSession(SessionInfo sessionInfo);
	}
}
