import LoginPage from './assets/pages/LoginPage'
import { Routes, Route} from 'react-router-dom'
import HomePage from './assets/pages/HomePage'


function App() {
    return (
        <div className="app-container">
            <main className="main-content">
                <div className="content-container">
                    <Routes>
                        <Route path="/" element={<LoginPage />} />
                        <Route path="/homepage" element={<HomePage/>} />
                    </Routes>
                </div>
            </main>
        </div>)
}

export default App
