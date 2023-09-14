using Microsoft.AspNetCore.Mvc;
using Blog.Services;
using Blog.ViewModels;
using Blog.Data;
using Blog.Extensions;
using Blog.Models;
using Blog.ViewModels.Accounts;
using SecureIdentity.Password;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Runtime.Intrinsics;
using System.Text.RegularExpressions;

namespace Blog.Controllers
{

    [ApiController]
    public class AccountController : ControllerBase
    {
        [HttpPost("v1/accounts/login")]
        public async Task<IActionResult> Post(
            [FromBody] RegisterViewModel model,
            [FromServices] EmailService emailService,
            [FromServices] BlogDataContext context)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ResultViewModel<string>(ModelState.GetErrors()));

            var user = new User
            {
                Name = model.Name,
                Email = model.Email,
                Slug = model.Email.Replace("@", "-").Replace(".", "-")
            };

            var password = PasswordGenerator.Generate(25); // Vai gerar uma senha
            user.PasswordHash = PasswordHasher.Hash(password); // vai gerar um hash dessa senha

            try
            {
                await context.Users.AddAsync(user);
                await context.SaveChangesAsync();

                emailService.Send(
                user.Name,
               user.Email,
                       subject: "Bem vindo ao blog!",
                       body: $"Sua senha é {password}");
                return Ok(new ResultViewModel<dynamic>(new
                {
                    user = user.Email,
                    password
                }));
            }
            catch (DbUpdateException)
            {
                return StatusCode(400, new ResultViewModel<string>("05X99 - Este E-mail já está cadastrado"));
            }
            catch
            {
                return StatusCode(500, new ResultViewModel<string>("05X04 - Falha interna no Servidor"));
            }
        }


        //fazer autenticação 
        // recuperar usuario no banco e comparar a senha dele
        public async Task<IActionResult> Login(
         [FromBody] LoginViewModel model,
         [FromServices] BlogDataContext context,
         [FromServices] TokenService tokenService) // este item depende do token service 
        {
            if (!ModelState.IsValid)
                return BadRequest(new ResultViewModel<string>(ModelState.GetErrors()));

            var user = await context
              .Users
              .AsNoTracking()
              .Include(x => x.Roles)
              .FirstOrDefaultAsync(x => x.Email == model.Email);

            if (user == null)

                return StatusCode(401, new ResultViewModel<string>("Usuário ou senha inaválido"));


            if (!PasswordHasher.Verify(user.PasswordHash, model.Password))
                return StatusCode(401, new ResultViewModel<string>("Usuário ou senha inaválido"));


            try
            {
                var token = tokenService.GenerateToken(user);
                return Ok(new ResultViewModel<string>(token, null));

            }
            catch
            {
                return StatusCode(500, new ResultViewModel<string>("05x04 - Falha interna no Servidor"));
            }

        }

        [Authorize]
        [HttpPost("v1/accounts/upload-image")]
        public async Task<IActionResult> UploadImage(
          [FromBody] UploadImageViewModel model,
          [FromServices] BlogDataContext context)
        {
            var fileName = $"{Guid.NewGuid().ToString()}.jpg";
            var data = new Regex(@"^data:image\/[a-z]+;base64,")
                      .Replace(model.Base64Image, "");
            var bytes byte[] = Convert.FromBase64String(data);

            try
            {
                await System.IO.File.WriteAllBytesAsync($"wwwroot/images/{fileName}", bytes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResultViewModel<string>("05X04 - Falha interna no servidor"));
            }

            var user = await context
                 .Users
                 .FirstOrDefaultAsync(x => x.Email == User.Identity.Name);

            if (user == null)
                return NotFound(new ResultViewModel<Category>("Usuário não encontrado"));

            user.Image = $"https://localhost:0000/images/{fileName}";

            try
            {
                context.Users.Update(user);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResultViewModel<string>("05X04 - Falha interna no Servidor"));
            }

            return Ok(new ResultViewModel<string>("Imagem alterada com sucesso!", null));

        }

    }
}