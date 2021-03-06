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

namespace Deveel.Data.Security {
	public static class SystemGroupNames {
		/// <summary>
		/// The name of the lock group.
		/// </summary>
		/// <remarks>
		/// If a user belongs to this group the user account is locked and they are not 
		/// allowed to log into the database.
		/// </remarks>
		public const string LockGroup = "#locked";

		/// <summary>
		/// The name of the schema manager group.
		/// </summary>
		/// <remarks>
		/// Users that belong in this group can create and drop schema from the system.
		/// </remarks>
		public const String SchemaManagerGroup = "schema manager";

		/// <summary>
		/// THe name of the secure access group.
		/// </summary>
		/// <remarks>
		/// If a user belongs to this group they are permitted to perform a number of 
		/// priviledged operations such as shutting down the database, and adding and 
		/// removing users.
		/// </remarks>
		public const string SecureGroup = "secure access";

		/// <summary>
		/// The name of the user manager group.
		/// </summary>
		/// <remarks>
		/// Users that belong in this group can create, alter and drop users from the 
		/// system.
		/// </remarks>
		public const String UserManagerGroup = "user manager";
	}
}
