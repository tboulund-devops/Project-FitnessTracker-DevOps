import { useState, type FormEvent } from "react";
import "./../../LoginPage.css";
import { useNavigate } from "react-router-dom";


function LoginPage() {
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();
  
  

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setError("");
    setSuccess("");
    setLoading(true);

    try {
      const response = await fetch("/api/APILogin/Login_CheckCredentials", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ username, password }),
      });

      if (response.ok) {
        const UserID = await response.text();
        setSuccess(UserID + " Redirecting to home…");
        localStorage.setItem("userID",UserID)
        navigate("/home");
      } else {
        const message = await response.text();
        setError(message || "Login failed");
      }
    } catch {
      setError("Could not reach the server. Please try again later.");
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="login-container">
      <form className="login-form" onSubmit={handleSubmit}>
        <h1>Login</h1>

        {error && <p className="login-error">{error}</p>}
        {success && <p className="login-success">{success}</p>}

        <label htmlFor="username">Username</label>
        <input
          id="username"
          type="text"
          value={username}
          onChange={(e) => setUsername(e.target.value)}
          placeholder="Enter your username"
          autoComplete="username"
          required
        />

        <label htmlFor="password">Password</label>
        <input
          id="password"
          type="password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          placeholder="Enter your password"
          autoComplete="current-password"
          required
        />

        <button type="submit" disabled={loading}>
          {loading ? "Logging in…" : "Log in"}
        </button>
          <button type="button" disabled={loading} onClick={() => navigate("/register")}>
              {loading ? "Navigation to RegistrationPage" : "Register"}
          </button>
      </form>
    </div>
  );
}

export default LoginPage;
