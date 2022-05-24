using System;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Traven.Logic.ViewModel.classes;
using Traven.Logic.ViewModel.interfaces;
using Traven.Logic.Model;
using Traven.Repository;
using Traven.UOW;
using Traven.View;

namespace Traven
{
    public class Context : DbContext
    {
        public Context() : base("DefaultConnection") { }

        public DbSet<Node> Node { get; set; }
        public DbSet<Media> Media { get; set; }
        public DbSet<MindMap> Mindmap { get; set; }
        public DbSet<User> User { get; set; }
        public DbSet<UserAuth> UserAuth { get; set; }
        public DbSet<Settings> Settings { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //TODO делать млдель БД 

            base.OnModelCreating(modelBuilder);
        }
    }
}
