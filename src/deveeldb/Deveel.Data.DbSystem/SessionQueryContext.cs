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

namespace Deveel.Data.DbSystem {
	public class SessionQueryContext : QueryContextBase {
		private IUserSession session;

		public SessionQueryContext(IUserSession session) {
			if (session == null)
				throw new ArgumentNullException("session");

			this.session = session;
		}

		public override IUserSession Session {
			get { return session; }
		}

		protected override void Dispose(bool disposing) {
			session = null;
			base.Dispose(disposing);
		}
	}
}
