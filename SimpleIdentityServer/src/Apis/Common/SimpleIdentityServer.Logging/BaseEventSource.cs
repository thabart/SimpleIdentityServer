using Microsoft.Extensions.Logging;
using System;

namespace SimpleIdentityServer.Logging
{
    public interface IEventSource
    {
        void Info(string message);
        void Failure(string message);
        void Failure(Exception exception);
    }

    public class BaseEventSource : IEventSource
    {
        protected readonly ILogger _logger;
        protected const string MessagePattern = "{Id} : {Task}, {Message} : {Operation}";

        public BaseEventSource(ILogger logger)
        {
            _logger = logger;
        }

        #region Information

        public void Info(string message)
        {
            var evt = new Event
            {
                Id = 8000,
                Task = EventTasks.Information,
                Message = message
            };

            LogInformation(evt);
        }

        #endregion

        #region Failures

        public void Failure(string message)
        {
            var evt = new Event
            {
                Id = 9000,
                Task = EventTasks.Failure,
                Message = $"Something goes wrong, code : {message}"
            };

            LogError(evt);
        }

        public void Failure(Exception exception)
        {
            var evt = new Event
            {
                Id = 9001,
                Task = EventTasks.Failure,
                Message = "an error occured"
            };

            LogError(evt, new EventId(28), exception);
        }

        #endregion

        #region Protected methods

        protected void LogInformation(Event evt)
        {
            _logger.LogInformation(MessagePattern, evt.Id, evt.Task, evt.Message, evt.Operation);
        }

        protected void LogError(Event evt)
        {
            _logger.LogError(MessagePattern, evt.Id, evt.Task, evt.Message, evt.Operation);
        }

        protected void LogError(Event evt, EventId evtId, Exception ex)
        {
            _logger.LogError(evtId, ex, MessagePattern, evt.Id, evt.Task, evt.Message, evt.Operation);
        }

        #endregion
    }
}
