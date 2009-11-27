﻿using System;
using System.Windows.Forms;

namespace Deveel.Data.Commands {
	[Command("ResetConnection", "&Reset Connection")]
	[CommandImage("Deveel.Data.Images.database_refresh.png")]
	public sealed class ResetConnectionCommand : Command {
		public override void Execute() {
			try {
				HostWindow.SetPointerState(Cursors.WaitCursor);
				Settings.ResetConnection();
				HostWindow.SetStatus(null, "Connection reset");
			} finally {
				HostWindow.SetPointerState(Cursors.Default);
			}
		}
	}
}