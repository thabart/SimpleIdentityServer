using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SimpleIdServer.Concurrency.Extensions;
using SimpleIdServer.Storage;

namespace SimpleIdServer.Concurrency
{
    public interface IRepresentationManager
    {
        Task AddOrUpdateRepresentationAsync(Controller controller, string representationId, bool addHeader = true);
        Task AddOrUpdateRepresentationAsync(Controller controller, string representationId, string etag, bool addHeader = true);
        Task UpdateHeader(Controller controller, string representationId);
        Task<bool> CheckRepresentationExistsAsync(Controller controller, string representationId);
        Task<bool> CheckRepresentationHasChangedAsync(Controller controller, string representationId);
        Task RemoveRepresentations();
    }

    internal class RepresentationManager : IRepresentationManager
    {
        private readonly IConcurrencyManager _concurrencyManager;

        public RepresentationManager(IConcurrencyManager concurrencyManager)
        {
            _concurrencyManager = concurrencyManager;
        }

        public async Task AddOrUpdateRepresentationAsync(
            Controller controller,
            string representationId,
            bool addHeader = true)
        {
            await AddOrUpdateRepresentationAsync(controller, representationId, Guid.NewGuid().ToString(), addHeader);
        }

        public async Task AddOrUpdateRepresentationAsync(
            Controller controller,
            string representationId,
            string etag,
            bool addHeader = true)
        {
            if (controller == null)
            {
                throw new ArgumentNullException(nameof(controller));
            }

            if (string.IsNullOrWhiteSpace(representationId))
            {
                throw new ArgumentNullException(nameof(representationId));
            }

            if (string.IsNullOrWhiteSpace(etag))
            {
                throw new ArgumentNullException(nameof(etag));
            }

            var concurrentObject = await _concurrencyManager.TryUpdateRepresentationAsync(representationId, etag);
            if (addHeader)
            {
                SetHeaders(controller, concurrentObject);
            }
        }

        public async Task UpdateHeader(
            Controller controller,
            string representationId)
        {
            if (controller == null)
            {
                throw new ArgumentNullException(nameof(controller));
            }

            if (string.IsNullOrWhiteSpace(representationId))
            {
                throw new ArgumentNullException(nameof(representationId));
            }

            var concurrentObject = await _concurrencyManager.TryGetRepresentationAsync(representationId);
            if (concurrentObject == null)
            {
                return;
            }

            SetHeaders(controller, concurrentObject);
        }

        public async Task<bool> CheckRepresentationExistsAsync(
            Controller controller,
            string representationId)
        {
            if (controller == null)
            {
                throw new ArgumentNullException(nameof(controller));
            }

            if (string.IsNullOrWhiteSpace(representationId))
            {
                throw new ArgumentNullException(nameof(representationId));
            }

            var concatenatedEtags = controller.GetIfMatch();
            var unmodifiedSince = controller.GetUnmodifiedSince();
            var checkDateCallback = new Func<DateTime, ConcurrentObject, bool>((d, c) =>
            {
                return c.DateTime <= d;
            });
            var checkEtagCorrectCallback = new Func<ConcurrentObject, List<EntityTagHeaderValue>, bool>((c, etags) =>
            {
                return etags.Any(e =>
                {
                    if (e.IsWeak)
                    {                        
                        // Weak etag
                        if (c.Etag.Contains(e.Tag.ToString()))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (e.Tag == c.Etag)
                        {
                            return true;
                        }
                    }

                    return false;
                });
            });
            return await ContinueExecution(concatenatedEtags, unmodifiedSince, representationId, checkDateCallback, checkEtagCorrectCallback);
        }

        public async Task<bool> CheckRepresentationHasChangedAsync(
            Controller controller,
            string representationId)
        {
            if (controller == null)
            {
                throw new ArgumentNullException(nameof(controller));
            }

            if (string.IsNullOrWhiteSpace(representationId))
            {
                throw new ArgumentNullException(nameof(representationId));
            }

            // Check the http request contains the header "If-None-Match"
            var concatenatedEtags = controller.GetIfNoneMatch();
            var modifiedSince = controller.GetModifiedSince();
            var checkDateCallback = new Func<DateTime, ConcurrentObject, bool>((d, c) =>
            {
                return c.DateTime > d;
            });
            var checkEtagCorrectCallback = new Func<ConcurrentObject, List<EntityTagHeaderValue>, bool>((c, etags) =>
            {
                return etags.All(etag =>
                {
                    if (etag.IsWeak)
                    {
                        // Weak etag
                        if (c.Etag.Contains(etag.Tag.ToString()))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (etag.Tag == c.Etag)
                        {
                            return false;
                        }
                    }

                    return true;
                });
            });
            return await ContinueExecution(concatenatedEtags, modifiedSince, representationId, checkDateCallback, checkEtagCorrectCallback);
        }

        public async Task RemoveRepresentations()
        {
           await _concurrencyManager.RemoveAllAsync();
        }

        private async Task<bool> ContinueExecution(
            string concatenatedEtags,
            string modifiedDate,
            string representationId,
            Func<DateTime, ConcurrentObject, bool> checkDateCallback,
            Func<ConcurrentObject, List<EntityTagHeaderValue>, bool> checkEtagCorrectCallback)
        {
            if (string.IsNullOrWhiteSpace(concatenatedEtags) &&
                string.IsNullOrWhiteSpace(modifiedDate))
            {
                return true;
            }

            // Check a representation exists
            var lastRepresentation = await _concurrencyManager.TryGetRepresentationAsync(representationId);
            if (lastRepresentation == null)
            {
                return true;
            }

            if (string.IsNullOrWhiteSpace(concatenatedEtags) || concatenatedEtags == "*")
            {
                // Process the date
                DateTime dateTime;
                if (!DateTime.TryParse(modifiedDate, out dateTime))
                {
                    return true;
                }

                return !checkDateCallback(dateTime, lastRepresentation);
            }
            else
            {
                // Check etags are correct
                var etagsStr = concatenatedEtags.Split(',');
                var etags = new List<EntityTagHeaderValue>();
                foreach (var etagStr in etagsStr)
                {
                    EntityTagHeaderValue et = null;
                    if (EntityTagHeaderValue.TryParse(etagStr, out et))
                    {
                        etags.Add(et);
                    }
                }

                if (checkEtagCorrectCallback(lastRepresentation, etags))
                {
                    return false;
                }
            }

            return true;
        }

        private void SetHeaders(Controller controller, ConcurrentObject concurrentObject)
        {
            controller.SetEtag(concurrentObject.Etag);
            controller.SetLastModifiedDate(concurrentObject.DateTime.ToUniversalTime().ToString("R"));
        }
    }
}
