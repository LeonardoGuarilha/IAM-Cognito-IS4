using System.Diagnostics;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using SkyCommerce.Data.Configuration;
using SkyCommerce.Data.Context;
using SkyCommerce.Site.Configure;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;

namespace SkyCommerce.Site
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
            services.AddControllersWithViews()
                .AddRazorRuntimeCompilation();
            
            services.AddHttpContextAccessor(); // Tem que ter esse cara aqui também

            JwtSecurityTokenHandler.DefaultMapInboundClaims = false; // Para o aspnet core não associar os schemas xml aos nomes defaults das claims
            
            if(Debugger.IsAttached)
                IdentityModelEventSource.ShowPII = true;
            
            // Integração com o OpenId Connect
            services.AddAuthentication(options =>
                {
                    options.DefaultScheme = "Cookies";
                    options.DefaultChallengeScheme = "oidc";
                })
                .AddCookie("Cookies")
                .AddOpenIdConnect("oidc", options =>
                {
                    options.Authority = "https://localhost:5001"; // URL do meu SSO

                    options.ClientId = "f23f4ee8810b474faec9c0bf1ef7319c";
                    options.ClientSecret = "20bfac9530e444719d71231695565d59";
                    options.ResponseType = "code"; // Qual o flow de autenticação que eu estou usando (code é o Authorization Code)
                    // Scopes são informações do usuário que eu quero retornar do IS4
                    options.Scope.Add("openid");
                    options.Scope.Add("profile");
                    options.Scope.Add("api_frete");
                    //options.Scope.Add("company_info");
                    options.GetClaimsFromUserInfoEndpoint = true;
                    options.SaveTokens = true; // O ASP.NET Core salva o access token que ele pegou no IS4 no cookie do usuário
                    // e ele faz o gerenciamento de access token através da biblioteca o OpenId Connect.
                    
                    // Retorna os dados das claims indicadas
                    options.Events.OnUserInformationReceived = context =>
                    {
                        options.ClaimActions.MapUniqueJsonKey("Cargo", "Cargo");
                        options.ClaimActions.MapUniqueJsonKey("picture", "picture");
                        return Task.CompletedTask;
                    };
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        NameClaimType = "name", // Nome da role que vai conter o nome do usuário
                        RoleClaimType = "role", // Qual claim que vai conter as roles do usuário

                    };
                });

            services.AddHttpClient();
            // Dbcontext config
            services.ConfigureProviderForContext<SkyContext>(DetectDatabase);

            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = new PathString("/conta/entrar");
            });
            services.ConfigureSkyCommerce();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            // Definindo a cultura padr�o: pt-BR
            var supportedCultures = new[] { new CultureInfo("pt-BR") };
            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture(culture: "pt-BR", uiCulture: "pt-BR"),
                SupportedCultures = supportedCultures,
                SupportedUICultures = supportedCultures
            });

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        /// <summary>
        /// it's just a tuple. Returns 2 parameters.
        /// Trying to improve readability at ConfigureServices
        /// </summary>
        private (DatabaseType, string) DetectDatabase => (
            Configuration.GetValue<DatabaseType>("ApplicationSettings:DatabaseType"),
            Configuration.GetConnectionString("DefaultConnection"));
    }
}
