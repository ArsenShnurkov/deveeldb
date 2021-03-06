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

using Deveel.Data.DbSystem;
using Deveel.Data.Diagnostics;

namespace Deveel.Data.Store {
	public class DataStorageException : ErrorException {
		public DataStorageException(int errorCode) 
			: this(errorCode, null) {
		}

		public DataStorageException(int errorCode, string message) 
			: this(errorCode, message, null) {
		}

		public DataStorageException() 
			: this(null) {
		}

		public DataStorageException(string message) 
			: this(message, null) {
		}

		public DataStorageException(string message, Exception innerException) 
			: this(StorageErrorCodes.Unknown, message, innerException) {
		}

		public DataStorageException(int errorCode, string message, Exception innerException) 
			: base(EventClasses.Storage, errorCode, message, innerException) {
		}
	}
}