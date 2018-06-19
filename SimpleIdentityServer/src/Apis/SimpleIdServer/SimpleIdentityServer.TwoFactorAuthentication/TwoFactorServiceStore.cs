using System;
using System.Collections.Generic;

namespace SimpleIdentityServer.TwoFactorAuthentication
{
    public class TwoFactorServiceStore
    {
        private static TwoFactorServiceStore _instance;
        private Dictionary<string, ITwoFactorAuthenticationService> _twoFactorAuthenticationServices;

        private TwoFactorServiceStore()
        {
            _twoFactorAuthenticationServices = new Dictionary<string, ITwoFactorAuthenticationService>();
        }

        public static TwoFactorServiceStore Instance()
        {
            if (_instance == null)
            {
                _instance = new TwoFactorServiceStore();
            }

            return _instance;
        }

        public Dictionary<string, ITwoFactorAuthenticationService> GetAll()
        {
            return _twoFactorAuthenticationServices;
        }

        public bool Add(string serviceType, ITwoFactorAuthenticationService service)
        {
            if (string.IsNullOrWhiteSpace(serviceType))
            {
                throw new ArgumentNullException(nameof(service));
            }

            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }
            
            if (_twoFactorAuthenticationServices.ContainsKey(serviceType))
            {
                return false;
            }

            _twoFactorAuthenticationServices.Add(serviceType, service);
            return true;
        }

        public ITwoFactorAuthenticationService Get(string serviceType)
        {
            if (string.IsNullOrWhiteSpace(serviceType))
            {
                throw new ArgumentNullException(nameof(serviceType));
            }

            if (!_twoFactorAuthenticationServices.ContainsKey(serviceType))
            {
                return null;
            }

            return _twoFactorAuthenticationServices[serviceType];
        }
    }
}
