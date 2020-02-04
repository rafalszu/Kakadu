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
                throw new Exception("No user found");

            if(BCrypt.Net.BCrypt.Verify(password, user.Password))
                return user;
            else
                return null;
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

        public UserModel Get(Guid Id)
        {
            if(Id == Guid.Empty)
                throw new ArgumentNullException(nameof(Id));

            var result = _instance.FirstOrDefault<UserModel>(x => x.Id == Id);
            if(result == null)
                throw new Exception("No user found");

            return result;
        }

        public UserModel Update(UserModel model)
        {
            if(model == null)
                throw new ArgumentNullException(nameof(model));

            if(_instance.Update<UserModel>(model))
                return model;

            return null;
        }

        public bool SetPassword(Guid Id, string password)
        {
            if(Id == Guid.Empty)
                throw new ArgumentNullException(nameof(Id));
            if(string.IsNullOrWhiteSpace(password))
                throw new ArgumentNullException(password);

            var model = this.Get(Id);
            if(model == null)
                throw new Exception("No user found");

            model.Password = BCrypt.Net.BCrypt.HashPassword(password);

            return this.Update(model) != null;
        }        
    }
}