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
using System.Text;

using Deveel.Data.Types;

namespace Deveel.Data.Routines {
	public sealed class RoutineParameter {
		public RoutineParameter(string name, DataType type, ParameterAttributes attributes) 
			: this(name, type, ParameterDirection.Input, attributes) {
		}

		public RoutineParameter(string name, DataType type) 
			: this(name, type, ParameterDirection.Input) {
		}

		public RoutineParameter(string name, DataType type, ParameterDirection direction) 
			: this(name, type, direction, ParameterAttributes.None) {
		}

		public RoutineParameter(string name, DataType type, ParameterDirection direction, ParameterAttributes attributes) {
			Attributes = attributes;
			Direction = direction;
			Type = type;
			Name = name;
		}

		public string Name { get; private set; }

		public ParameterDirection Direction { get; private set; }

		public DataType Type { get; private set; }

		public ParameterAttributes Attributes { get; private set; }

		public int Offset { get; internal set; }

		public bool IsNullable {
			get { return (Attributes & ParameterAttributes.Nullable) != 0; }
		}

		public bool IsUnbounded {
			get { return (Attributes & ParameterAttributes.Unbounded) != 0; }
		}

		public bool IsOutput {
			get { return (Direction & ParameterDirection.Output) != 0; }
		}

		public bool IsInput {
			get { return (Direction & ParameterDirection.Input) != 0; }
		}

		public override string ToString() {
			var sb = new StringBuilder();
			if (IsInput && !IsOutput) {
				sb.Append("IN");
			} else if (IsOutput && !IsInput) {
				sb.Append("OUT");
			} else if (IsOutput && IsInput) {
				sb.Append("IN OUT");
			}

			sb.Append(Name);
			sb.Append(" ");
			sb.Append(Type);

			if (!IsNullable)
				sb.Append("NOT NULL");

			return sb.ToString();
		}
	}
}