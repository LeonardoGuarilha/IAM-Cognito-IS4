using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace CongitoWebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            
            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                })
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
                {
                    options.ClientId = "kr7fp1ratvn8cjvb73urd5dif";
                    options.ClientSecret = "gbdujs0f0gb4jsh1gmjklh95jop7rohfp1pau4udi319b4a8o8q";
                    options.Authority = "https://cognito-idp.us-west-2.amazonaws.com/us-west-2_ApBKD3JYn";
                    options.Scope.Add("openid");
                    options.Scope.Add("profile");
                    options.Scope.Add("email");
                    options.ResponseType = "code";
                    //options.Scope.Add("http://leave.letsdocoding.com/leaves.cancel");
                    //options.Scope.Add("http://leave.letsdocoding.com/leaves.apply");
                    options.SaveTokens = true;

                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        NameClaimType = "cognito:user"
                    };

                    options.Events = new OpenIdConnectEvents()
                    {
                        OnRedirectToIdentityProviderForSignOut = context =>
                        {
                            var logoutUri = "https://authclientapi.auth.us-west-2.amazoncognito.com/logout?client_id=kr7fp1ratvn8cjvb73urd5dif";

                            logoutUri += $"&logout_uri={context.Request.Scheme}://{context.Request.Host}";

                            //var postLogoutUri = context.Properties.RedirectUri;
                            //if (!string.IsNullOrEmpty(postLogoutUri))
                            //{
                            //    if (postLogoutUri.StartsWith("/"))
                            //    {
                            //        // transform to absolute
                            //        var request = context.Request;
                            //        postLogoutUri = request.Scheme + "://" + request.Host + request.PathBase + postLogoutUri;
                            //    }
                            //    logoutUri += $"&returnTo={ Uri.EscapeDataString(postLogoutUri)}";
                            //}

                            context.Response.Redirect(logoutUri);
                            context.HandleResponse();

                            return Task.CompletedTask;
                        }
                    };
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}