using Backend.Domain;

namespace Backend.Application.Service.Interfaces;

public interface IUserService
{
    public Task<User?> GetUserByUsername(string username);
    public Task<UserInformationDTO?> GetUserInformation(int userID);
}