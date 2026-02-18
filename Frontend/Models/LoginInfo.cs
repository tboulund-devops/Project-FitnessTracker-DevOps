using Frontend.ViewModels;

namespace Frontend.Models;

public class LoginInfo:Bindable
{
    private string _username="";
    public string Username
    {
        get { return _username; }
        set
        {
            _username = value;
            propertyIsChanged();
        }
    }
    
    private string _password="";

    public string Password
    {
        get { return _password; }
        set
        {
            _password = value;
            propertyIsChanged();
        }
        
    }
    public LoginInfo() { }
    public LoginInfo(string username, string password)
    {
        Username = username;
        Password = password;
    }

}