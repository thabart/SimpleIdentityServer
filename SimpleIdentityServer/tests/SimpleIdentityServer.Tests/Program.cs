using SimpleIdentityServer.Core.Common.Extensions;
using System;

namespace SimpleIdentityServer.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            // string idToken = "eyJhbGciOiJSUzI1NiIsImtpZCI6ImEzck1VZ01Gdjl0UGNsTGE2eUYzekFrZnF1RSIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJodHRwczovL3NpbXBsZWlkZW50aXR5c2VydmVyLmF6dXJld2Vic2l0ZXMubmV0L3Rva2VuIiwiYXVkIjpbIk15QmxvZyIsIk15QmxvZ0NsaWVudFNlY3JldFBvc3QiLCI3Yjk5YmY5Yi0yNzE4LTQzZjMtOWVlOC0zMzQzZjllODdmMmEiLCJodHRwczovL3NpbXBsZWlkZW50aXR5c2VydmVyLmF6dXJld2Vic2l0ZXMubmV0L3Rva2VuIl0sImV4cCI6MTQ1NTg5MzE3MCwiaWF0IjoxNDUyODkzMTcwLCJhY3IiOiJvcGVuaWQucGFwZS5hdXRoX2xldmVsLm5zLnBhc3N3b3JkPTEiLCJhbXIiOlsicGFzc3dvcmQiXSwiYXpwIjoiN2I5OWJmOWItMjcxOC00M2YzLTllZTgtMzM0M2Y5ZTg3ZjJhIiwic3ViIjoiYWRtaW5pc3RyYXRvckBob3RtYWlsLmJlIiwiY19oYXNoIjoibEVaMUQ3d1Y1UGNIeHlpX0pHaEJZZyJ9.gPWkdQw_cy4GlQgUmhvRps4Ng4WMm-_T_A4N-NarURWbBTvyAmUPfDI2CqFE--R2qEwGVmzPyAYr9LoB8apel0I7uuUPF-jkPpZCv_U_Id_Ppslme4Py1ZDYkE3a9bK_SDdJeMoRINOIuBK_kj3BkcC9oPWM2pErptBtEenokSI";
            var idToken = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCIsImtpZCI6IlJTQS1kZWZhdWx0LXNpZyJ9.eyJzdWIiOiI0Y2RkMmE1YWRiM2QxOTdmZDExYzE3ODcyYmEwNmUyN2E0NDE0ZGZmNTAyOGYyMTZkYTU1OWMyMTI2Yjg0ZjM5IiwiYXV0aF90aW1lIjoxNDQ5NzY3MzQ1LCJhdF9oYXNoIjoia0tzSTk5T0tYbDFrRUh1UGN5Z1hXZyIsIm5vbmNlIjoiOGJ6T3pGR0t6dFpFIiwiaWF0IjoxNDQ5NzY3NTc2LCJleHAiOjE0NDk3Njc4NzYsImF1ZCI6IjBmYmRiMTYzLTI0ZGEtNGRlMy05MDRmLTViZmM3ZTJjNTc3ZiIsImlzcyI6Imh0dHBzOi8vZ3VhcmRlZC1jbGlmZnMtODYzNS5oZXJva3VhcHAuY29tL29wIn0.jxRLdUCF4qoux8mmF4vJXoimdFZHG2RnK5kSt8zAUj46nUlB4iB4LTekst9Fyt52F5HKoy3bnU4GGeYS1iTunQzCUSx0Ts14BSt6roKB_2-EOyDL9R-uIZlYbJzU9EzpC4rDaSKRXI1wC-uT7PwVVyBu6MR_zdpkFRZ7h2mgZye6Rr-lB6cz7F6GZ8FydVDwBjMU5BePCYgq21bhC66PuE4WycrA7trS__GTdXVNzukSlwLvRH0uDXJpUFK4k_ZtcIUdAIcMNBKUhU_6dwgVRwqR7p6LfB5uwfIowAiunNsGZNSpYQvVf9hFkUJisPjtvMM9DdQayWndif30YR7Q4A";
            var idTokenSplitted = idToken.Split('.');
            var header = idTokenSplitted[0].Base64Decode();
            var body = idTokenSplitted[1].Base64Decode();
            var signature = idTokenSplitted[2].Base64Decode();
            Console.ReadLine();
        }
    }
}
