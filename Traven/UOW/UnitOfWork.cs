using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Traven.Logic.Model;
using Traven.Logic.ViewModel.classes;
using Traven.Logic.ViewModel.interfaces;
using Traven.Repository;
using Traven.UOW;
using Traven.View;

namespace Traven.UOW
{
    public class UnitOfWork : IDisposable
    {
        private Context db;
        private Repository<User> userRepository;
        private Repository<UserAuth> userAuthRepository;
        private Repository<MindMap> mindmapRepository;
        private Repository<Node> nodeRepository;
        private Repository<Settings> settingsRepository;
        private Repository<Media> mediaRepository;
        
        public Repository<User> UserRepository
        {
            get
            {
                if (userRepository == null)
                    userRepository = new Repository<User>(db);
                return userRepository;
            }
        }

        public Repository<UserAuth> UserAuthRepository
        {
            get
            {
                if (userAuthRepository == null)
                    userAuthRepository = new Repository<UserAuth>(db);
                return userAuthRepository;
            }
        }

        public Repository<MindMap> MindMapRepository
        {
            get
            {
                if (mindmapRepository == null)
                    mindmapRepository = new Repository<MindMap>(db);
                return mindmapRepository;
            }
        }

        public Repository<Node> NodeRepository
        {
            get
            {
                if (nodeRepository == null)
                    nodeRepository = new Repository<Node>(db);
                return nodeRepository;
            }
        }

        public Repository<Settings> SettingsRepository
        {
            get
            {
                if (settingsRepository == null)
                    settingsRepository = new Repository<Settings>(db);
                return settingsRepository;
            }
        }

        public Repository<Media> MediaRepository
        {
            get
            {
                if (mediaRepository == null)
                    mediaRepository = new Repository<Media>(db);
                return mediaRepository;
            }
        }

        public UnitOfWork()
        {
            db = new Context();
        }
        public void Save()
        {
            db.SaveChanges();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
