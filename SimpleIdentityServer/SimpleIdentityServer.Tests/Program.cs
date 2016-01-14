using SimpleIdentityServer.Core.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            string idToken = "eyJhbGciOiJSUzI1NiIsImtpZCI6ImEzck1VZ01Gdjl0UGNsTGE2eUYzekFrZnF1RSIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJodHRwczovL3NpbXBsZWlkZW50aXR5c2VydmVyLmF6dXJld2Vic2l0ZXMubmV0LyIsImF1ZCI6WyJNeUJsb2ciLCJNeUJsb2dDbGllbnRTZWNyZXRQb3N0IiwiaHR0cHM6Ly9zaW1wbGVpZGVudGl0eXNlcnZlci5henVyZXdlYnNpdGVzLm5ldC8iXSwiZXhwIjoxNDU1Nzk4MDY3LCJpYXQiOjE0NTI3OTgwNjcsImFjciI6Im9wZW5pZC5wYXBlLmF1dGhfbGV2ZWwubnMucGFzc3dvcmQ9MSIsImFtciI6WyJwYXNzd29yZCJdLCJhenAiOiI5YTIxMTZjZi0xMjc2LTRiOTItOTQzMS02NGQyNDRjZmViMDEiLCJzdWIiOiJhZG1pbmlzdHJhdG9yQGhvdG1haWwuYmUiLCJjX2hhc2giOiJBcHZaem82OXNNRF93aVQ1dFRoZzlRIn0.bvzQBIn_-y6QnstEt-TK-aWbdKrisB_EydbnePHFJi935Sj_d3eI78oR2dkpWiYP3PSwSLsbwj5SUWnQOOSWx5ZsoWV5aI18z9ix2iaOC5M85GNXJVe0U1s8R0dFeJAh4Rc_HmCbd1RY7B9E3326zWEu9yWafbb4CNhJ_BY9QPw";
            var idTokenSplitted = idToken.Split('.');
            var header = idTokenSplitted[0].Base64Decode();
            var body = idTokenSplitted[1].Base64Decode();
            var signature = idTokenSplitted[2].Base64Decode();
            Console.ReadLine();
        }
    }
}
