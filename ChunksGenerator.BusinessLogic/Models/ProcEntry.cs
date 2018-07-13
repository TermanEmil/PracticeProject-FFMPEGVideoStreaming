using System;
using System.Diagnostics;

namespace ChunksGenerator.BusinessLogic.Models
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

			if (!Proc.HasExited)
                Proc.Kill();
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
