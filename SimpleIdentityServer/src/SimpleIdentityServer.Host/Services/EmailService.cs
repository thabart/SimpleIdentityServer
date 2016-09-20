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
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Host.Services
{
    public interface IEmailService
    {

    }

    public class EmailOptions
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
    }

    internal class EmailService : IEmailService
    {
        private readonly EmailOptions _emailOptions;

        public EmailService(EmailOptions emailOptions)
        {
            if (emailOptions == null)
            {
                throw new ArgumentNullException(nameof(emailOptions));
            }

            _emailOptions = emailOptions;
        }

        public async Task Send(string code)
        {
            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(_emailOptions.Host, _emailOptions.Port, false);
                client.AuthenticationMechanisms.Remove("XOAUTH2");
                await client.AuthenticateAsync(_emailOptions.Login, _emailOptions.Password);

                client.Disconnect(true);
            }
        }
    }
}
