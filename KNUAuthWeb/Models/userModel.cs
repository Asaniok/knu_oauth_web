using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace KNUAuthWeb.Models
{
    public class User
    {
        [Required(ErrorMessage = "Поле не повинно бути порожнім!")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Поле не повинно бути порожнім!")]
        public string Password { get; set; }
        [Required(ErrorMessage = "Поле не повинно бути порожнім!")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Поле не повинно бути порожнім!")]
        public string middlename { get; set; }
        [Required(ErrorMessage = "Поле не повинно бути порожнім!")]
        public string Surname { get; set; }

        [Required(ErrorMessage = "Поле не повинно бути порожнім!")]
        public string RestoreEmail { get; set; }
    }
}
