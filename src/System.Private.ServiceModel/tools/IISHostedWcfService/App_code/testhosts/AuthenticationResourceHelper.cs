﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


using System;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace WcfService
{
    public static class AuthenticationResourceHelper
    {
        public static void ConfigureServiceHostUseDigestAuth(ServiceHost serviceHost)
        {
            var authManager = new ResourceDigestServiceAuthorizationManager();
            serviceHost.Description.Behaviors.Add(authManager);
        }

        public static void ConfigureServiceHostUseBasicAuth(ServiceHost serviceHost)
        {
            var authManager = new ResourceBasicServiceAuthorizationManager();
            serviceHost.Description.Behaviors.Add(authManager);
        }

        private class ResourceBasicServiceAuthorizationManager : BasicServiceAuthorizationManager
        {
            private const string BasicUsernameHeaderName = "BasicUsername";
            private const string BasicPasswordHeaderName = "BasicPassword";

            public ResourceBasicServiceAuthorizationManager() : base("NoRealm") { }

            public override bool GetPassword(ref Message message, string username, out string password)
            {
                if (!message.Properties.ContainsKey(HttpRequestMessageProperty.Name))
                {
                    password = null;
                    return false;
                }

                var requestProperty = (HttpRequestMessageProperty)message.Properties[HttpRequestMessageProperty.Name];
                string sentUsername = requestProperty.Headers.Get(BasicUsernameHeaderName);
                if (username.Equals(sentUsername))
                {
                    password = requestProperty.Headers.Get(BasicPasswordHeaderName);
                    return true;
                }

                password = null;
                return false;
            }
        }

        private class ResourceDigestServiceAuthorizationManager : DigestServiceAuthorizationManager
        {
            private const string DigestUsernameHeaderName = "DigestUsername";
            private const string DigestPasswordHeaderName = "DigestPassword";
            private const string DigestRealmHeaderName = "DigestRealm";

            public ResourceDigestServiceAuthorizationManager() : base(string.Empty) { }

            public override bool GetPassword(ref Message message, string username, out string password)
            {
                if (!message.Properties.ContainsKey(HttpRequestMessageProperty.Name))
                {
                    password = null;
                    return false;
                }

                var requestProperty = (HttpRequestMessageProperty) message.Properties[HttpRequestMessageProperty.Name];
                string sentUsername = requestProperty.Headers.Get(DigestUsernameHeaderName);
                if (username.Equals(sentUsername))
                {
                    password = requestProperty.Headers.Get(DigestPasswordHeaderName);
                    return true;
                }

                password = null;
                return false;
            }

            public override string GetRealm(ref Message message)
            {
                if (!message.Properties.ContainsKey(HttpRequestMessageProperty.Name))
                {
                    return base.GetRealm(ref message);
                }

                var requestProperty = (HttpRequestMessageProperty)message.Properties[HttpRequestMessageProperty.Name];
                return requestProperty.Headers.Get(DigestRealmHeaderName);
            }
        }
    }
}
