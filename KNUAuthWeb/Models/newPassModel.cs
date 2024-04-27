using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace KNUAuthWeb.Models
{
    public class newPasswordModel
    {
        [Required(ErrorMessage = "Поле не повинно бути порожнім!")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Поле не повинно бути порожнім!")]
        public string newPassword { get; set; }
        [Required(ErrorMessage = "Поле не повинно бути порожнім!")]
        public string newPasswordCheck { get; set; }
    }
}
