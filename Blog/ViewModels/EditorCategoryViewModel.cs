using System.ComponentModel.DataAnnotations;

namespace Blog.ViewModels
{
    public class CreateCategoryViewModel
    {
        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(40, MinimumLength = 3, ErrorMessage = "Este campo deve conter no minimo 3 caracteres")]
        public string Name { get; set; }
        [Required(ErrorMessage = "O slug é obrigatório")]
        public string Slug { get; set; }
    }
}