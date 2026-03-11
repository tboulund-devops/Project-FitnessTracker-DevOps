import './NavBar.css';

function NavBar() {
    return (
        <nav className="navbar">
            <div className="navbar-logo">MyApp</div>
            <ul className="navbar-links">
                <li><a href="/">Home</a></li>
                <li><a href="/profile">Profile</a></li>
                <li><a href="/new-workout">NewWorkout</a></li>
                <li><a href="/old-workouts">OldWorkouts</a></li>
                <li><a href="/settings">Settings</a> </li>
            </ul>
        </nav>
    )
}


 export default NavBar;