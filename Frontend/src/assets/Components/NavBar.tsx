import './NavBar.css';

function NavBar() {
    return (
        <nav className="navbar">
            <ul className="navbar-links">
                <li><a href="/home">Homepage</a></li>
                <li><a href="/profile">Profile</a></li>
                <li><a href="/new-workout">New Workout</a></li>
                <li><a href="/old-workouts">Old Workouts</a></li>
                <li><a href="/settings">Settings</a></li>
            </ul>
        </nav>
    )
}


 export default NavBar;