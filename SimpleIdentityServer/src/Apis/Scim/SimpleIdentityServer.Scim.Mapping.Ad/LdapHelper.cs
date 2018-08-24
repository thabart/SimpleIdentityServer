using System;
using System.DirectoryServices.Protocols;
using System.Net;

namespace SimpleIdentityServer.Scim.Mapping.Ad
{
    internal class LdapHelper : IDisposable
    {
        private LdapConnection _connection;

        public bool Connect(string server, int port, string userName, string password, string domain = null)
        {
            var ldapDirectoryIdentifier = new LdapDirectoryIdentifier(server, port);
            var networkCredential = new NetworkCredential(userName, password, domain);
            try
            {
                _connection = new LdapConnection(ldapDirectoryIdentifier);
                _connection.AuthType = AuthType.Basic;
                _connection.SessionOptions.ProtocolVersion = 3;
                _connection.Bind(networkCredential);
                return true;
            }
            catch(Exception)
            {
                _connection = null;
                return false;
            }
        }

        public SearchResponse Search(string distinguishedName, string filter)
        {
            if(_connection == null)
            {
                // TODO : THROW AN EXCEPTION.
                return null;
            }

            var searchRequest = new SearchRequest(distinguishedName, filter, SearchScope.Subtree, null);
            return _connection.SendRequest(searchRequest) as SearchResponse;
        }

        public void Dispose()
        {
            if(_connection != null)
            {
                _connection.Dispose();
            }
        }
    }
}
