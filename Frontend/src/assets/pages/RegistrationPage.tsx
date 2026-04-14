import { useState, type FormEvent } from "react";
import "./../../LoginPage.css";
import { useNavigate } from "react-router-dom";


function RegistrationPage() {
    const [username, setUsername] = useState("");
    const [password, setPassword] = useState("");
    const [name, setName] = useState("");
    const [email, setEmail] = useState("");
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
            const response = await fetch("/api/APILogin/Register", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ username, password, name, email }),
            });

            const message = await response.text();
            if (response.ok) {
                setSuccess(message || "Registration successful. Redirecting to login…");
                navigate("/");
            } else {
                setError(message || "Registration failed");
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
                <h1>Registration</h1>

                {error && <p className="login-error">{error}</p>}
                {success && <p className="login-success">{success}</p>}

                <label htmlFor="username">Add Username</label>
                <input
                    id="username"
                    type="text"
                    value={username}
                    onChange={(e) => setUsername(e.target.value)}
                    placeholder="Enter your username"
                    autoComplete="username"
                    required
                />

                <label htmlFor="password">Add Password</label>
                <input
                    id="password"
                    type="password"
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                    placeholder="Enter your password"
                    autoComplete="current-password"
                    required
                />

                <label htmlFor="name">Add Name</label>
                <input
                    id="name"
                    type="text"
                    value={name}
                    onChange={(e) => setName(e.target.value)}
                    placeholder="Enter your full name"
                    autoComplete="name"
                    required
                />

                <label htmlFor="email">Add Email</label>
                <input
                    id="email"
                    type="email"
                    value={email}
                    onChange={(e) => setEmail(e.target.value)}
                    placeholder="Enter your email"
                    autoComplete="email"
                    required
                />
                <button type="submit" disabled={loading}>
                    {loading ? "Adding Credentials" : "Register"}
                </button>
                <button type="button" disabled={loading} onClick={() => navigate("/")}>
                    {loading ? "Navigation to LoginPage" : "Return to Login"}
                </button>
            </form>
        </div>
    );
}

export default RegistrationPage;
