import LoginPage from './assets/pages/LoginPage'
import { Routes, Route} from 'react-router-dom'
import HomePage from './assets/pages/HomePage'
import ProfilePage from './assets/pages/ProfilePage'


function App() {
    return (
        <div className="app-container">
            <main className="main-content">
                <div className="content-container">
                    <Routes>
                        <Route path="/" element={<LoginPage />} />
                        <Route path="/homepage" element={<HomePage/>} />
                        <Route path="/profile" element={<ProfilePage/>} />
                    </Routes>
                </div>
            </main>
        </div>)
}

export default App
