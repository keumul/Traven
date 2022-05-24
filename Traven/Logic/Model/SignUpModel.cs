using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Traven.Logic.Model
{
    public class SignUpModel
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Password { get; set; }
        public string RepeatPassword { get; set; }
        public string Mail { get; set; }
        public string Avatar { get; set; }
    }
}
