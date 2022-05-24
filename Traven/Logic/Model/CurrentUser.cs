using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Traven.Logic.Model
{
    public static class CurrentUser
    {
        public static int Id { get; set; }
        public static int MapId { get; set; } = -1;

        public static string Name { get; set; }
        public static string Surname { get; set; }
        public static string Avatar { get; set; }
    }
}
