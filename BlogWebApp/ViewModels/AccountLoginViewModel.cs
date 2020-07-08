using BlogWebApp.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BlogWebApp.ViewModels
{
    public class AccountLoginViewModel
    {

        [StringLength(60, MinimumLength = 3)]
        [BindProperty]
        [Required]
        public string Username { get; set; }


    }
}
