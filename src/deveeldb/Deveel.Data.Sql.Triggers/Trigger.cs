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

namespace Deveel.Data.Sql.Triggers {
	/// <summary>
	/// Represents an event fired at a given modification event
	/// (either <c>INSERT</c>, <c>DELETE</c> or <c>UPDATE</c>) at
	/// a given time (<c>BEFORE</c> or <c>AFTER</c>).
	/// </summary>
	/// <remarks>
	/// <para>
	/// Event triggers can be of two different main categories:
	/// <list type="bullet">
	/// <item><b>Callback</b>, called for every modification event of the given
	/// type on any resource and notified only to the user client</item>
	/// <item><b>Procedure</b>, that executes a procedure (either external, internal
	/// or defined as the body of the trigger)</item>
	/// </list>
	/// </para>
	/// </remarks>
	public class Trigger : IDbObject {
		/// <summary>
		/// Constructs a trigger with the given information.
		/// </summary>
		/// <param name="triggerInfo">The object defining the information
		/// of this trigger.</param>
		/// <exception cref="ArgumentNullException">
		/// If the provided <paramref name="triggerInfo"/> is <c>null</c>.
		/// </exception>
		public Trigger(TriggerInfo triggerInfo) {
			if (triggerInfo == null)
				throw new ArgumentNullException("triggerInfo");

			TriggerInfo = triggerInfo;
		}

		/// <summary>
		/// Gets the information describing the dynamics of this trigger.
		/// </summary>
		public TriggerInfo TriggerInfo { get; private set; }

		/// <summary>
		/// Gets the fully qualified name of the trigger, as defined in <see cref="TriggerInfo"/>
		/// </summary>
		/// <seealso cref="TriggerInfo"/>
		/// <seealso cref="Triggers.TriggerInfo.TriggerName"/>
		public ObjectName TriggerName {
			get { return TriggerInfo.TriggerName; }
		}

		/// <summary>
		/// Gets the type of the trigger, as defined in <see cref="TriggerInfo"/>
		/// </summary>
		/// <seealso cref="TriggerInfo"/>
		/// <seealso cref="Triggers.TriggerInfo.TriggerType"/>
		public TriggerType TriggerType {
			get { return TriggerInfo.TriggerType; }
		}

		DbObjectType IDbObject.ObjectType {
			get { return DbObjectType.Trigger; }
		}

		ObjectName IDbObject.FullName {
			get { return TriggerInfo.TriggerName; }
		}

		private bool ShouldFire(TableEventContext context) {
			if ((TriggerInfo.EventType & context.EventType) == 0)
				return false;

			var tableName = context.Table.TableInfo.TableName;
			return TriggerInfo.TableName == null ||
			       TriggerInfo.TableName.Equals(tableName, true);
		}

		public void Fire(TableEventContext context) {
			if (context == null)
				throw new ArgumentNullException("context");

			if (ShouldFire(context)) {
				
			}
		}
	}
}
