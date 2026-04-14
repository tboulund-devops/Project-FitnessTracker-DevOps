import type { ProfileInfo } from "../../Domain/ProfileInfo";
import "../../index.css";
import {useEffect, useState} from "react";

function ProfilePage() {
    
    const [profileInfo, setProfileInfo] = useState<ProfileInfo | null>(null);
    const [error, setError] = useState("");
    const [loading, setLoading] = useState(true);
    const [emailDraft, setEmailDraft] = useState("");
    const [saving, setSaving] = useState(false);
    const [isChangeEmailEnabled, setIsChangeEmailEnabled] = useState(false);

    const userId = localStorage.getItem("userID") ?? localStorage.getItem("userId");
    const numericUserId = userId ? parseInt(userId, 10) : null;


    useEffect(() => {
        if (!userId) {
            setError("Please log in");
            setLoading(false);
            return;
        }

        async function fetchProfile() {
            try {
                const response = await fetch(`/api/user/APIUser/GetProfileInfo/${userId}`);
                if (response.ok) {
                    const data = await response.json();
                    setProfileInfo(data);
                    setEmailDraft(data.email ?? "");
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
    
    
    // Fetches the status of a feature toggle by its key - Frontend
    async function fetchFeatureToggles(featureKey: string): Promise<boolean> {
        try {
            const response = await fetch(`/api/toggle/APIToggles/feature-toggles/${encodeURIComponent(featureKey)}`);
            if (!response.ok) {
                throw new Error('Failed to fetch feature toggles');
            }
            const data = await response.json();
            return Boolean(data);
        } catch (error) {
            console.error('Error fetching feature toggles:', error);
            return false;
        }
    }
    async function updateEmail(newEmail: string) {
        if (!userId) {
            setError("Please log in");
            return;
        }
        setSaving(true);
        try {
            const response = await fetch(`/api/user/APIUser/UpdateEmail?userID=${numericUserId}&newEmail=${encodeURIComponent(newEmail)}`, {
                method: "PUT",
            });
            if (response.ok) {
                const refreshed = await fetch(`/api/user/APIUser/GetProfileInfo/${userId}`);
                if (refreshed.ok) {
                    const data = await refreshed.json();
                    setProfileInfo(data);
                    setEmailDraft(data.email ?? "");
                }
            } else {
                const message = await response.text();
                setError(message || "Failed to update email");
            }
        } catch {
            setError("Could not reach the server. Please try again later.");
        } finally {
            setSaving(false);
        }
    }
    
    // Load the feature toggle status on component mount
    useEffect(() => {
        async function loadToggle() {
            const enabled = await fetchFeatureToggles("ChangeEmail");
            setIsChangeEmailEnabled(enabled);
        }
        loadToggle();
    }, []);

    if (loading) {
        return (
            <div className="home-container">
                <div className="profile-card">
                    <h1>Profile Page</h1>
                    <p className="profile-loading">Loading profile…</p>
                </div>
            </div>
        );
    }

    if (error) {
        return (
            <div className="profile-container">
                <div className="profile-card">
                    <h1>Profile Page</h1>
                    <p className="profile-error">{error}</p>
                </div>
            </div>
        );
    }


    if (!profileInfo) return null;
    
    return (
        <div className="profile-page">
            <h1>Profile Page</h1>
            <div className="profile-stats">
                <p><strong>Username:</strong> {profileInfo.username}</p>
                <div className="email-section">
                <label>
                    <strong>Email:</strong>
                    <input
                        type="email"
                        value={emailDraft}
                        onChange={(e) => setEmailDraft(e.target.value)}
                        disabled={!isChangeEmailEnabled || saving}
                    />
                </label>
                    {isChangeEmailEnabled && (
                        <button
                            onClick={() => updateEmail(emailDraft)}
                            disabled={saving || !emailDraft || emailDraft === profileInfo.email}
                            className="savebutton"
                        >
                            {saving ? "Saving..." : "Save Email"}
                        </button>
                    )}
                </div>
                <p><strong>Member Since:</strong> {new Date(profileInfo.timeOfRegistration).toLocaleDateString()}</p>
            </div>
        </div>
    );
   
}

export default ProfilePage;
