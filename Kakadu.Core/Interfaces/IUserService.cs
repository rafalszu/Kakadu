using System;
using Kakadu.Core.Models;

namespace Kakadu.Core.Interfaces
{
    public interface IUserService
    {
        UserModel Authenticate(string username, string password);

        UserModel Get(Guid id);

        UserModel Create(UserModel model);

        UserModel Update(UserModel model);

        bool SetPassword(Guid id, string password);

        void SeedDefaultUserIfEmpty();
    }
}