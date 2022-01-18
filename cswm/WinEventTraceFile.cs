using cswm.Win32;
using PInvoke;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace cswm
{
    public class WinEventTraceFile : IDisposable
    {
        private WinEventListener winEventListener;
        private Encoding encoding = new UTF8Encoding(true);
        private FileStream logStream;

        public WinEventTraceFile(WinEventListener winEventListener)
        {
            this.winEventListener = winEventListener;
            this.winEventListener.EventRaised += LogWinEvent;

            var filename = $"WinEventTrace-{DateTime.Now:yyyyMMdd_HHmmss}.log";
            logStream = File.OpenWrite(filename);
            var header = @$"{{""Timestamp"":""{DateTime.Now:u}"",""Events"":[";
            WriteLog(header);
        }

        public void Dispose()
        {
            var footer = @"]}";
            logStream.Seek(logStream.Position - 1, SeekOrigin.Begin); // -1 to overwrite last comma in list
            WriteLog(footer);

            if (winEventListener != null)
            {
                winEventListener.EventRaised -= LogWinEvent;
                winEventListener = null;
            }
            if (logStream != null)
            {
                logStream.Dispose();
                logStream = null;
            }
        }

        private void LogWinEvent(WinEventListener.WinEvent winEvent)
        {
            var serializableEvent = new TraceEvent(winEvent);
            JsonSerializer.Serialize(logStream, serializableEvent);
            WriteLog(",");
        }

        private void WriteLog(string message)
        {
            var data = encoding.GetBytes(message);
            logStream.Write(data, 0, data.Length);
        }

        private record TraceEvent
        {
            public long hWinEventHook { get; init; }
            public User32.WindowsEventHookType eventType { get; init; }
            public long hWnd { get; init; }
            public int idObject { get; init; }
            public int idChild { get; init; }
            public int dwEventThread { get; init; }
            public uint dwmsEventTime { get; init; }
            public long gwlStyle { get; init; }
            public int pId { get; init; }
            public int threadId { get; init; }
            public int left { get; init; }
            public int top { get; init; }
            public int right { get; init; }
            public int bottom { get; init; }
            public string processName { get; init; }
            public bool onCurrentVirtualDesktop { get; init; }

            public TraceEvent(WinEventListener.WinEvent winEvent)
            {
                hWinEventHook = winEvent.hWinEventHook.ToInt64();
                eventType = winEvent.eventType;
                hWnd = winEvent.hWnd.ToInt64();
                idObject = winEvent.idObject;
                idChild = winEvent.idChild;
                dwEventThread = winEvent.dwEventThread;
                dwmsEventTime = winEvent.dwmsEventTime;

                var window = new Window(winEvent.hWnd);
                gwlStyle = window.Styles;
                pId = window.ProcessId;
                threadId = window.ThreadId;
                left = window.Size.left;
                top = window.Size.top;
                right = window.Size.right;
                bottom = window.Size.bottom;

                if (window.ProcessId != 0)
                {
                    var details = new WindowDetails(window);
                    processName = details.Process.ProcessName;
                    onCurrentVirtualDesktop = details.OnCurrentVirtualDesktop;
                }
            }
        }

        private class TraceLog
        {
            public DateTime Timestamp { get; init; }
            public TraceEvent[] Events { get; init; }
        }

    }
}
