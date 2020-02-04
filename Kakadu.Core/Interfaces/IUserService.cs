using System;
using Kakadu.Core.Models;

namespace Kakadu.Core.Interfaces
{
    public interface IUserService
    {
        UserModel Authenticate(string username, string password);

        UserModel Get(Guid Id);

        UserModel Create(UserModel model);

        UserModel Update(UserModel model);

        bool SetPassword(Guid Id, string password);
    }
}