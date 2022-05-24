using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Traven.Logic.Model
{
    public class Settings
    {
        public int Id { get; set; }
        public string Color { get; set; }
        public bool Fill { get; set; }
        public int Font { get; set; }
        public Media Icon { get; set; }

    }
}
