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

namespace Deveel.Data.Sql {
	public class SqlErrorException : ErrorException {
		public SqlErrorException(int errorCode) 
			: base(EventClasses.SqlModel, errorCode) {
		}

		public SqlErrorException(int errorCode, string message) 
			: base(EventClasses.SqlModel, errorCode, message) {
		}

		public SqlErrorException(int errorCode, string message, Exception innerException) 
			: base(EventClasses.SqlModel, errorCode, message, innerException) {
		}
	}
}
