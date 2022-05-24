using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Traven.Logic.Model
{
    public class Node
    {
        public int Id { get; set; }
        public float X { set; get; }
        public float Y { set; get; }
        public float Wight { set; get; }
        public float Height { set; get; }
        public string Text { set; get; }
        public Settings Settings { set; get; }
        public int? FatherId { set; get; }
        [ForeignKey("FatherId")]
        public Node FatherNode { set; get; }
    }
}
