﻿#if NO_TIMER

using Serilog.Debugging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Serilog.Sinks.Elasticsearch.CrossPlatform
{
    class PortableTimer : IDisposable
    {
        enum PortableTimerState
        {
            NotWaiting,
            Waiting,
            Active,
            Disposed
        }

        readonly object _stateLock = new object();
        PortableTimerState _state = PortableTimerState.NotWaiting;

        readonly Action<CancellationToken> _onTick;
        readonly CancellationTokenSource _cancel = new CancellationTokenSource();

        public PortableTimer(Action<CancellationToken> onTick)
        {
            if (onTick == null) throw new ArgumentNullException(nameof(onTick));
            _onTick = onTick;
        }

        public async void Start(TimeSpan interval)
        {
            if (interval < TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(interval));

            lock (_stateLock)
            {
                if (_state == PortableTimerState.Disposed)
                    throw new ObjectDisposedException("PortableTimer");

                // There's a little bit of raciness here, but it's needed to support the
                // current API, which allows the tick handler to reenter and set the next interval.

                if (_state == PortableTimerState.Waiting)
                    throw new InvalidOperationException("The timer is already set.");

                if (_cancel.IsCancellationRequested) return;

                _state = PortableTimerState.Waiting;
            }

            try
            {
                if (interval > TimeSpan.Zero)
                    await Task.Delay(interval, _cancel.Token).ConfigureAwait(false);

                _state = PortableTimerState.Active;

                if (!_cancel.Token.IsCancellationRequested)
                {
                    _onTick(_cancel.Token);
                }
            }
            catch (TaskCanceledException tcx)
            {
                SelfLog.WriteLine("The timer was canceled during invocation: {0}", tcx);
            }
            finally
            {
                lock (_stateLock)
                    _state = PortableTimerState.NotWaiting;
            }
        }

        public void Dispose()
        {
            _cancel.Cancel();

            while (true)
            {
                lock (_stateLock)
                {
                    if (_state == PortableTimerState.Disposed ||
                        _state == PortableTimerState.NotWaiting)
                    {
                        _state = PortableTimerState.Disposed;
                        return;
                    }
                }

// On the very old platforms, we've got no option but to spin here.
#if THREAD
                Thread.Sleep(10);
#endif
            }
        }
    }
}
#endif