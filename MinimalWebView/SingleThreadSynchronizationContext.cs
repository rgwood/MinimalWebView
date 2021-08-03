using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace MinimalWebView
{
    // based on this very good Stephen Toub article: https://devblogs.microsoft.com/pfxteam/await-synchronizationcontext-and-console-apps/
    internal sealed class SingleThreadSynchronizationContext : SynchronizationContext
    {
        private readonly BlockingCollection<KeyValuePair<SendOrPostCallback, object>> m_queue = new BlockingCollection<KeyValuePair<SendOrPostCallback, object>>();
        private HWND hwnd;

        public SingleThreadSynchronizationContext(HWND hwnd) : base()
        {
            this.hwnd = hwnd;
        }

        public override void Post(SendOrPostCallback d, object state)
        {
            m_queue.Add(new KeyValuePair<SendOrPostCallback, object>(d, state));
            PInvoke.SendMessage(hwnd, Program.WM_SYNCHRONIZATIONCONTEXT_WORK_AVAILABLE, 0, 0);
        }

        public void RunAvailableWorkOnCurrentThread()
        {
            KeyValuePair<SendOrPostCallback, object> workItem;

            while (m_queue.TryTake(out workItem))
                workItem.Key(workItem.Value);
        }

        public void Complete() { m_queue.CompleteAdding(); }
    }
}
