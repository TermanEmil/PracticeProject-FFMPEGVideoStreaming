using System;
using System.Diagnostics;

namespace FFMPEGStreamingTools
{
    public class ProcEntry
    {
		public Process Proc { get; set; }
		public string Name { get; set; }
		internal bool closeRequested = false;

		public void CloseProcess()
		{
			if (Proc == null)
				return;

			closeRequested = true;

			Proc.CloseMainWindow();
			Proc.Close();

			Proc.Dispose();         
		}
    }
}
