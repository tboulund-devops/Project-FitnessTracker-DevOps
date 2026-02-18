namespace DefaultNamespace;

public interface IAPILoginController
{
    public ActionResult<bool> CheckCredentials(string username, string password);
}