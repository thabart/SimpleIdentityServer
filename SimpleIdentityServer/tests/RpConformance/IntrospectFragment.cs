using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace RpConformance
{
    public class IntrospectFragment
    {
        private readonly RequestDelegate _next;
        private readonly IApplicationBuilder _app;

        public IntrospectFragment(RequestDelegate next, IApplicationBuilder app)
        {
            if (next == null)
            {
                throw new ArgumentNullException(nameof(next));
            }

            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            _next = next;
            _app = app;
        }

        public async Task Invoke(HttpContext context)
        {
            var request = context.Request;
            await _next.Invoke(context);
        }
    }
}
