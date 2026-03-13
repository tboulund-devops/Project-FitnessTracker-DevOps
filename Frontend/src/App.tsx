import { BrowserRouter, Routes, Route, useLocation } from 'react-router-dom';
import NavBar from './assets/Components/NavBar';
import HomePage from './assets/pages/HomePage';
import Login from './assets/pages/LoginPage';
import Profile from './assets/pages/ProfilePage';
import NewWorkout from './assets/pages/NewWorkout';
import OldWorkouts from './assets/pages/OldWorkouts';

function AppContent() {
    const location = useLocation();

    // Don't show NavBar on login page
    const showNavBar = location.pathname !== '/';

    return (
        <>
            {showNavBar && <NavBar />}
            <Routes>
                <Route path="/" element={<Login />} />
                <Route path="/home" element={<HomePage />} />
                <Route path="/profile" element={<Profile />} />
                <Route path="/new-workout" element={<NewWorkout />} />
                <Route path="/old-workouts" element={<OldWorkouts />} />
            </Routes>
        </>
    );
}

function App() {
    return (
        <BrowserRouter>
            <AppContent />
        </BrowserRouter>
    );
}

export default App;