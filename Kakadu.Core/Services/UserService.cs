using System;
using Kakadu.Core.Interfaces;
using Kakadu.Core.Models;
using LiteDB;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kakadu.Core.Services
{
    public class UserService : IUserService
    {
        private readonly ILogger<UserService> _logger;
        private readonly LiteRepository _instance;

        public UserService(ILogger<UserService> logger, LiteRepository instance)
        {
            _logger = logger;
            _instance = instance;
        }
        
        public UserModel Authenticate(string username, string password)
        {
            var user = _instance.SingleOrDefault<UserModel>(x => x.Username == username);
            if(user == null)
                return null;

            return BCrypt.Net.BCrypt.Verify(password, user.Password) ? user : null;
        }

        public UserModel Create(UserModel model)
        {
            if(model == null)
                throw new ArgumentNullException(nameof(model));

            if(!string.IsNullOrWhiteSpace(model.Password))
                model.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);

            _instance.Insert<UserModel>(model);
            return model;
        }

        public UserModel Get(Guid id)
        {
            if(id == Guid.Empty)
                throw new ArgumentNullException(nameof(id));

            var result = _instance.FirstOrDefault<UserModel>(x => x.Id == id);
            if(result == null)
                throw new Exception("No user found");

            return result;
        }

        public UserModel Update(UserModel model)
        {
            if(model == null)
                throw new ArgumentNullException(nameof(model));

            return _instance.Update<UserModel>(model) ? model : null;
        }

        public bool SetPassword(Guid id, string password)
        {
            if(id == Guid.Empty)
                throw new ArgumentNullException(nameof(id));
            if(string.IsNullOrWhiteSpace(password))
                throw new ArgumentNullException(password);

            var model = this.Get(id);
            if(model == null)
                throw new Exception("No user found");

            model.Password = BCrypt.Net.BCrypt.HashPassword(password);

            return this.Update(model) != null;
        }

        public void SeedDefaultUserIfEmpty()
        {
            if (_instance.Fetch<UserModel>().Count == 0)
                Create(new UserModel
                {
                    FirstName = "System",
                    Id = Guid.NewGuid(),
                    LastName = "Administrator",
                    Username = "admin",
                    Password = "admin"
                });
        }
    }
}