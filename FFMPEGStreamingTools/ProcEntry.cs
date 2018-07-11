using System;
using System.Diagnostics;

namespace FFMPEGStreamingTools
{
    public class ProcEntry
    {
		public Process Proc { get; set; }
		public string Name { get; set; }
		public int Hash { get; set; }
		internal bool closeRequested = false;

		public void CloseProcess()
		{
			if (Proc == null)
				return;

			closeRequested = true;

			Proc.Kill();
			Proc.Dispose();
		}

		public void Restart(Action chunkingAction)
		{
			closeRequested = false;

			if (Proc == null || Proc.HasExited)
				chunkingAction.Invoke();
			else
				Proc.Kill();
		}
    }
}
