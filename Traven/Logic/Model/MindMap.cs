using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Traven.Logic.Model
{
    public class MindMap
    {
        public int Id { get; set; }
        //public bool Access { get; set; }
        public Media Media { get; set; }
        public Node Node { get; set; }
        public User Creator { get; set; }
    }
}
