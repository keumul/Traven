using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Traven.Logic.ViewModel.classes;
using Traven.Logic.ViewModel.interfaces;
using Traven.Repository;
using Traven.UOW;
using Traven.View;

namespace Traven.Logic.Model
{
    public class Media
    {
        public int Id { get; set; }
        public string Path { get; set; }
    }
}
