﻿using System;
using System.Collections.Generic;

namespace Deveel.Data.Security {
	public sealed class UserIdentification {
		public UserIdentification(string method) {
			if (String.IsNullOrEmpty(method))
				throw new ArgumentNullException("method");

			Method = method;
			Arguments = new Dictionary<string, object>();
		}

		static UserIdentification() {
			PlainText = new UserIdentification("plain");
		}

		public string Method { get; private set; }

		public IDictionary<string, object> Arguments { get; private set; }

		public static UserIdentification PlainText { get; private set; }

		public static UserIdentification Pkcs12(string salt) {
			return new UserIdentification("hash") {
				Arguments = {
					["salt"] = salt,
					["mechanism"] = "pkcs12"
				}
			};
		}
	}
}
