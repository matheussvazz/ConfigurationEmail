using System.Text;
using Blog;
using Blog.Data;
using Blog.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
ConfigureAuthentication(builder);
ConfigureMvc(builder);
ConfigureServices(builder);


var app = builder.Build();
LoadConfiguration(app);

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();


void LoadConfiguration(WebApplication app)
{
   Configuration.JwtKey = app.Configuration.GetValue<string>("JwtKey");
   Configuration.ApiKeyName = app.Configuration.GetValue<string>("ApiKeyName");
   Configuration.ApiKey = app.Configuration.GetValue<string>("Apikey");

   var smtp = new Configuration.SmtpConfiguration();
   app.Configuration.GetSection("Smtp").Bind(smtp);
   Configuration.Smtp = smtp;
}

void ConfigureAuthentication(WebApplicationBuilder builder)

{
   var key = Encoding.ASCII.GetBytes(Configuration.JwtKey);
   builder.Services.AddAuthentication(x =>
   {
      x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
      x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
   }).AddJwtBearer(x =>
   {
      x.TokenValidationParameters = new TokenValidationParameters
      {
         ValidateIssuerSigningKey = true,
         IssuerSigningKey = new SymmetricSecurityKey(key),
         ValidateIssuer = false,
         ValidateAudience = false
      };
   }
   );

}

void ConfigureMvc(WebApplicationBuilder builder)
{
   builder   // Configura o comportamento da API
   .Services
   .AddControllers()
   .ConfigureApiBehaviorOptions(options =>
   {
      options.SuppressModelStateInvalidFilter = true;
   });
}

void ConfigureServices(WebApplicationBuilder builder)
{
   builder.Services.AddDbContext<BlogDataContext>();
   builder.Services.AddTransient<TokenService>();
}