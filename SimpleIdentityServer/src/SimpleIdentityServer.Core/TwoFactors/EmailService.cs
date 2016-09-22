#region copyright
// Copyright 2015 Habart Thierry
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using MailKit.Net.Smtp;
using MimeKit;
using SimpleIdentityServer.Core.Models;
using System;
using System.Net.Security;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Core.TwoFactors
{
    public class EmailService : ITwoFactorAuthenticationService
    {
        public EmailService()
        {

        }

        public int Code
        {
            get
            {
                return (int)TwoFactorAuthentications.Email;
            }
        }

        public async Task SendAsync(string code, ResourceOwner user)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Habart Thierry", "habarthierry@hotmail.fr"));
            message.To.Add(new MailboxAddress("thabart", "habarthierry@hotmail.fr"));
            message.Subject = $"Confirmation code";
            message.Body = new TextPart("plain")
            {
                Text = $"The confirmation code is {code}"
            };

            using (var client = new SmtpClient())
            {
                client.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => {
                    return true;
                };
                await client.ConnectAsync("smtp.live.com", 587, false);
                client.AuthenticationMechanisms.Remove("XOAUTH2");
                await client.AuthenticateAsync("habarthierry@hotmail.fr", "SORbonne1989");
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }
    }
}
