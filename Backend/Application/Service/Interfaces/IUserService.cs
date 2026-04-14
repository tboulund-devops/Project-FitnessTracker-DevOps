using Backend.Domain;

namespace Backend.Application.Service.Interfaces;

public interface IUserService
{
    public Task<User?> GetUserByUsername(string username);
    public Task<UserInformationDTO?> GetUserInformation(int userID);
    
    public Task<User> GetProfileInfo(int userID);
    
    public Task<bool> UpdateUserEmail(int userID, string newEmail);
}