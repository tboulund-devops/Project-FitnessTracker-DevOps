import {useEffect, useState} from "react";
import type ProfileStats from "../../Domain/ProfileStats";
import "../../index.css";
function HomePage() {
    const [stats, setStats] = useState<ProfileStats | null>(null);
    const [error, setError] = useState("");
    const [loading, setLoading] = useState(true);

    // TODO: replace with actual logged-in user ID (e.g. from auth context)
    const userId = 1;

    useEffect(() => {
        async function fetchProfile() {
            try {
                const response = await fetch(`/api/user/APIUser/GetProfileOverview/${userId}`);
                if (response.ok) {
                    const data: ProfileStats = await response.json();
                    setStats(data);
                } else {
                    const message = await response.text();
                    setError(message || "Failed to load profile");
                }
            } catch {
                setError("Could not reach the server. Please try again later.");
            } finally {
                setLoading(false);
            }
        }

        fetchProfile();
    }, [userId]);

    function formatDate(iso: string): string {
        return new Date(iso).toLocaleDateString(undefined, {
            year: "numeric",
            month: "long",
            day: "numeric",
        });
    }

    if (loading) {
        return (
            <div className="home-container">
                <div className="profile-card">
                    <h1>Homepage</h1>
                    <p className="profile-loading">Loading profile…</p>
                </div>
            </div>
        );
    }

    if (error) {
        return (
            <div className="profile-container">
                <div className="profile-card">
                    <h1>Homepage</h1>
                    <p className="profile-error">{error}</p>
                </div>
            </div>
        );
    }

    if (!stats) return null;

    return (
        <div className="profile-container">
            <div className="profile-card">
                <h1>Profile Overview</h1>

                <div className="profile-stats">
                    <div className="stat-row">
                        <span className="stat-label">Name</span>
                        <span className="stat-value">{stats.name}</span>
                    </div>

                    <div className="stat-row">
                        <span className="stat-label">Member Since</span>
                        <span className="stat-value">{formatDate(stats.timeOfRegistry)}</span>
                    </div>

                    <div className="stat-row">
                        <span className="stat-label">Total Workouts</span>
                        <span className="stat-value highlight">{stats.totalAmountOfWorkouts}</span>
                    </div>

                    <div className="stat-row">
                        <span className="stat-label">Total Time Trained</span>
                        <span className="stat-value">{stats.totalAmountOfTimeTrained}</span>
                    </div>

                    <div className="stat-row">
                        <span className="stat-label">Current Streak</span>
                        <span className="stat-value highlight">
                            <span className="streak-detail">
                                {stats.currentStreakDays}
                                <span className="streak-unit">days</span>
                                ({stats.currentStreakWeeks}
                                <span className="streak-unit">weeks</span>)
                            </span>
                        </span>
                    </div>

                    <div className="stat-row">
                        <span className="stat-label">Best Streak</span>
                        <span className="stat-value">
                            <span className="streak-detail">
                                {stats.bestStreakDays}
                                <span className="streak-unit">days</span>
                                ({stats.bestStreakWeeks}
                                <span className="streak-unit">weeks</span>)
                            </span>
                        </span>
                    </div>

                    <div className="stat-row">
                        <span className="stat-label">Favorite Exercise</span>
                        <span className="stat-value highlight">{stats.favoriteExercise}</span>
                    </div>
                </div>
            </div>
        </div>
    );
}
export default HomePage;