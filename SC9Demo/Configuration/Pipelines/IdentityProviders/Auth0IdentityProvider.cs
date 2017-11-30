using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.Owin.Authentication.Configuration;
using Sitecore.Owin.Authentication.Pipelines.IdentityProviders;
using Sitecore.Diagnostics;
using Owin;
using Sitecore.Owin.Authentication.Services;
using System.Security.Claims;

namespace SC9Demo.Configuration.Pipelines.IdentityProviders
{
    public class Auth0IdentityProvider : Sitecore.Owin.Authentication.Pipelines.IdentityProviders.IdentityProvidersProcessor
    {
        public Auth0IdentityProvider(FederatedAuthenticationConfiguration federatedAuthenticationConfiguration) : base(federatedAuthenticationConfiguration)
        {
        }

        protected override string IdentityProviderName
        {
            get
            {
                return "Auth0";
            }
        }

        protected override void ProcessCore(IdentityProvidersArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            IdentityProvider identityProvider = this.GetIdentityProvider();
            string authenticationType = this.GetAuthenticationType();

            var provider = new Auth0.Owin.Auth0AuthenticationProvider
            {
                OnAuthenticated = (context) =>
                {

                    // transform all claims
                    ClaimsIdentity identity = context.Identity;
                    foreach (Transformation current in identityProvider.Transformations)
                    {
                        current.Transform(identity, new TransformationContext(FederatedAuthenticationConfiguration, identityProvider));
                    }
                    return System.Threading.Tasks.Task.FromResult(0);
                },

                OnReturnEndpoint = (context) =>
                {
                    // xsrf validation
                    if (context.Request.Query["state"] != null && context.Request.Query["state"].Contains("xsrf="))
                    {
                        var state = HttpUtility.ParseQueryString(context.Request.Query["state"]);
                        //todo: do something with it.
                    }

                    return System.Threading.Tasks.Task.FromResult(0);
                }
            };

            // not needed yet.
            //Auth0AuthenticationOptions options = new Auth0AuthenticationOptions();


            // need to change these into settngs
            args.App.UseAuth0Authentication(
                clientId: "bn8HQpg8DMIiVPjfUfVa2uob8qvvgaRp",
                clientSecret: "smTdil4tkMq0DHyUK_XYa1Jx_CjavuCnFMzPRGx36wUjDUB_cC_JWveUUFYmQi8q",
                domain: "sc9demo.eu.auth0.com",
                provider: provider);
        }
    }
}