using Backend.Application.Service.Interfaces;
using Backend.Domain;
using Backend.External.Repos;
using Backend.Gateway;

namespace Backend.Service;

public class LoginService : ILoginService
{
    private readonly ILoginRepo _repo;
    private readonly IUserRepo _userRepo;

    public LoginService(ILoginRepo repo, IUserRepo userRepo)
    {
        _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        _userRepo = userRepo ?? throw new ArgumentNullException(nameof(userRepo));
    }

    public int CheckCredentials(LoginRequest request)
    {
        // Validate input
        if (request == null || string.IsNullOrWhiteSpace(request.Username) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return -1; // Indicate invalid credentials
        }

        var credentialsFromDatabase = _repo.getCredentials(request.Username);

        // Check if we got valid credentials back BEFORE trying to access them
        if (credentialsFromDatabase == null || credentialsFromDatabase.Count < 2)
        {
            return -1; //wrong returned from database - failed login
        }

        
        if (credentialsFromDatabase[0] == request.Username &&
            credentialsFromDatabase[1] == request.Password)
        {
            return _repo.getUserID(request.Username);
        }

        return -1; // Indicate failed login
    }
    
    public bool RegisterLoginCredentials(RegisterUserRequest request)
    {
        // Validate input
        if (request == null || string.IsNullOrWhiteSpace(request.Username) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return false; // Indicate invalid credentials
        }

        // Check if username already exists
        var existingCredentials = _repo.getCredentials(request.Username);
        if (existingCredentials != null && existingCredentials.Count >= 2)
        {
            return false; // Username already exists
        }

        // Add new credentials to the database and use the generated id for tblUser insert.
        int credentialsID = _repo.addCredentials(request.Username, request.Password);
        if (credentialsID <= 0)
        {
            return false;
        }

        bool addedUserInfo = _userRepo.AddUserInformation(credentialsID, request.Name, request.Email, request.TotalWorkoutTime);
        return addedUserInfo;
    }

}