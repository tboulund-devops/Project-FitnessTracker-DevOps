import './NavBar.css';
import { useNavigate } from 'react-router-dom';

function NavBar() {
    const navigate = useNavigate();

    const handleLogout = () => {
        localStorage.removeItem('userID');
        navigate('/');
    };

    return (
        <nav className="navbar">
            <ul className="navbar-links">
                <li><a href="/home">Homepage</a></li>
                <li><a href="/profile">Profile</a></li>
                <li><a href="/new-workout">New Workout</a></li>
                <li><a href="/old-workouts">Old Workouts</a></li>
                <li><a href="/settings">Settings</a></li>
                <li className="logout-item"><button type="button" onClick={handleLogout}>Logout</button></li>
            </ul>
        </nav>
    )
}


export default NavBar;
