import { useState } from "react";
import { useNavigate } from "react-router-dom";
import "../../index.css";
import "../../NewWorkoutPage.css";

// ── Types ────────────────────────────────────────────
interface SetEntry {
    localId: number;
    exerciseId: number;
    weight: number;
    reps: number;
    restBetweenSetInSec: number;
    completed: boolean;
}

interface ExerciseEntry {
    localId: number;
    name: string;
    sets: SetEntry[];
}

interface WorkoutPayload {
    dateOfWorkout: string;
    name: string;
}

// Hard-coded exercise list (matches ExerciseID values in the DB)
const EXERCISES = [
    { id: 1, name: "Bench Press" },
    { id: 2, name: "Squat" },
    { id: 3, name: "Deadlift" },
    { id: 4, name: "Overhead Press" },
    { id: 5, name: "Barbell Row" },
    { id: 6, name: "Pull-up" },
    { id: 7, name: "Dumbbell Curl" },
    { id: 8, name: "Tricep Pushdown" },
    { id: 9, name: "Leg Press" },
    { id: 10, name: "Lat Pulldown" },
];


let _nextId = 1;
const nextId = () => _nextId++;

const mapExerciseNameToId = new Map(EXERCISES.map((exercise) => [exercise.name, exercise.id]));

const getValidationError = (
    currentUserId: string | null,
    workoutName: string,
    exercises: ExerciseEntry[]
): string | null => {
    if (!currentUserId) {
        return "User ID not found. Please log in again.";
    }
    if (!workoutName.trim()) {
        return "Please enter a workout name.";
    }
    if (exercises.length === 0) {
        return "Please add at least one exercise.";
    }

    const allSets = exercises.flatMap((exercise) => exercise.sets);
    if (allSets.length === 0) {
        return "Please add at least one set.";
    }
    if (allSets.some((set) => set.reps <= 0 || set.weight <= 0)) {
        return "Every set must have at least 1 rep and a weight greater than 0 kg.";
    }

    return null;
};

const createWorkout = async (currentUserId: string, payload: WorkoutPayload): Promise<number> => {
    const createRes = await fetch(
        `/api/workout/APIWorkout/CreateWorkout?UserId=${currentUserId}`,
        {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(payload),
        }
    );

    if (!createRes.ok) {
        const message = await createRes.text();
        throw new Error(message || "Failed to create workout.");
    }

    const workoutId = parseInt(await createRes.text(), 10);
    if (isNaN(workoutId) || workoutId <= 0) {
        throw new Error("Server returned an invalid workout ID.");
    }

    return workoutId;
};

const saveSets = async (workoutId: number, allSets: SetEntry[]) => {
    for (const set of allSets) {
        const setPayload = {
            ExerciseID: set.exerciseId,
            Weight: Math.round(set.weight),
            Reps: set.reps,
            RestBetweenSetInSec: set.restBetweenSetInSec,
        };

        const setRes = await fetch(
            `/api/workout/APIWorkout/AddSetToWorkout?workoutId=${workoutId}`,
            {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(setPayload),
            }
        );

        if (!setRes.ok) {
            const message = await setRes.text();
            throw new Error(`Set save failed: ${message}`);
        }
    }
};

function NewWorkout() {
    const navigate = useNavigate();

    const [workoutName, setWorkoutName] = useState("");
    const [exercises, setExercises] = useState<ExerciseEntry[]>([]);
    const [selectedExerciseId, setSelectedExerciseId] = useState<string>("");
    const [startTime] = useState<Date>(new Date());
    const [saving, setSaving] = useState(false);
    const [error, setError] = useState("");
    const [success, setSuccess] = useState("");

    // ── Exercise helpers ──────────────────────────────
    const addExercise = () => {
        if (!selectedExerciseId) return;
        const ex = EXERCISES.find((e) => e.id === Number(selectedExerciseId));
        if (!ex) return;
        setExercises((prev) => [
            ...prev,
            { localId: nextId(), name: ex.name, sets: [] },
        ]);
        setSelectedExerciseId("");
    };

    const removeExercise = (localId: number) =>
        setExercises((prev) => prev.filter((e) => e.localId !== localId));

    // ── Set helpers ───────────────────────────────────
    const addSet = (exerciseLocalId: number) => {
        setExercises((prev) =>
            prev.map((ex) => {
                if (ex.localId !== exerciseLocalId) return ex;
                const last = ex.sets[ex.sets.length - 1];
                const newSet: SetEntry = {
                    localId: nextId(),
                    exerciseId: mapExerciseNameToId.get(ex.name) ?? 1,
                    weight: last?.weight ?? 0,
                    reps: last?.reps ?? 10,
                    restBetweenSetInSec: last?.restBetweenSetInSec ?? 90,
                    completed: false,
                };
                return { ...ex, sets: [...ex.sets, newSet] };
            })
        );
    };

    const updateSet = (
        exLocalId: number,
        setLocalId: number,
        field: keyof Pick<SetEntry, "weight" | "reps" | "restBetweenSetInSec">,
        value: number
    ) => {
        setExercises((prev) =>
            prev.map((ex) =>
                ex.localId !== exLocalId
                    ? ex
                    : {
                          ...ex,
                          sets: ex.sets.map((s) =>
                              s.localId === setLocalId ? { ...s, [field]: value } : s
                          ),
                      }
            )
        );
    };

    const removeSet = (exLocalId: number, setLocalId: number) => {
        setExercises((prev) =>
            prev.map((ex) =>
                ex.localId !== exLocalId
                    ? ex
                    : { ...ex, sets: ex.sets.filter((s) => s.localId !== setLocalId) }
            )
        );
    };
    
    // ── Save ─────────────────────────────────────────
    const saveWorkout = async () => {
        setError("");
        setSuccess("");

        const currentUserId = localStorage.getItem("userID");
        const allSets = exercises.flatMap((e) => e.sets);
        const validationError = getValidationError(currentUserId, workoutName, exercises);
        if (validationError) {
            setError(validationError);
            return;
        }
        if (!currentUserId) {
            return;
        }

        setSaving(true);
        try {
            const workoutPayload: WorkoutPayload = {
                dateOfWorkout: startTime.toISOString(),
                name: workoutName.trim(),
            };
            const workoutId = await createWorkout(currentUserId, workoutPayload);
            await saveSets(workoutId, allSets);

            setSuccess("Workout saved! Redirecting...");
            setTimeout(() => navigate("/old-workouts"), 1200);
        } catch (err) {
            const message = err instanceof Error ? err.message : "Could not reach the server. Please try again later.";
            setError(message);
        } finally {
            setSaving(false);
        }
    };

    // ── Render ────────────────────────────────────────
    return (
        <div className="nw-container">
            <div>
                <h1>Log Workout</h1>
                <p>Track your sets, reps and weight</p>
            </div>
            {/* Workout name */}
            <div className="nw-card">
                <h2>Workout Details</h2>
                <div className="nw-field">
                    <label htmlFor="workout-name">Workout Name</label>
                    <input
                        id="workout-name"
                        type="text"
                        placeholder="e.g. Upper Body Day, Leg Day…"
                        value={workoutName}
                        onChange={(e) => setWorkoutName(e.target.value)}
                    />
                </div>
            </div>

            {/* Add exercise */}
            <div className="nw-card">
                <h2>Add Exercise</h2>
                <div className="nw-exercise-select-row">
                    <div className="nw-field">
                        <label htmlFor="exercise-select">Exercise</label>
                        <select
                            id="exercise-select"
                            value={selectedExerciseId}
                            onChange={(e) => setSelectedExerciseId(e.target.value)}
                        >
                            <option value="">— choose an exercise —</option>
                            {EXERCISES.map((ex) => (
                                <option key={ex.id} value={ex.id}>
                                    {ex.name}
                                </option>
                            ))}
                        </select>
                    </div>
                    <button
                        className="nw-btn-primary"
                        style={{ flexShrink: 0, width: "auto", padding: "0.6rem 1.2rem" }}
                        onClick={addExercise}
                        disabled={!selectedExerciseId}
                    >
                        + Add
                    </button>
                </div>
            </div>

            {/* Exercise list */}
            {exercises.map((ex, exIdx) => (
                <div key={ex.localId} className="nw-exercise-card">
                    <div className="nw-exercise-header">
                        <div>
                            <p className="nw-exercise-title">{ex.name}</p>
                            <p className="nw-exercise-sub">Exercise {exIdx + 1}</p>
                        </div>
                        <button className="nw-btn-remove" onClick={() => removeExercise(ex.localId)}>
                            ✕
                        </button>
                    </div>

                    <div className="nw-sets">
                        {ex.sets.map((set, sIdx) => (
                            <div key={set.localId} className="nw-set-row">
                                <span className="nw-set-index">#{sIdx + 1}</span>

                                <div className="nw-set-fields">
                                    <div className="nw-set-field">
                                        <label>Reps</label>
                                        <input
                                            type="number"
                                            min={0}
                                            value={set.reps}
                                            disabled={set.completed}
                                            onChange={(e) =>
                                                updateSet(ex.localId, set.localId, "reps", parseInt(e.target.value) || 0)
                                            }
                                        />
                                    </div>
                                    <div className="nw-set-field">
                                        <label>Weight (kg)</label>
                                        <input
                                            type="number"
                                            min={0}
                                            value={set.weight}
                                            disabled={set.completed}
                                            onChange={(e) =>
                                                updateSet(ex.localId, set.localId, "weight", parseInt(e.target.value) || 0)
                                            }
                                        />
                                    </div>
                                    <div className="nw-set-field">
                                        <label>Rest (sec)</label>
                                        <input
                                            type="number"
                                            min={0}
                                            value={set.restBetweenSetInSec}
                                            disabled={set.completed}
                                            onChange={(e) =>
                                                updateSet(ex.localId, set.localId, "restBetweenSetInSec", parseInt(e.target.value) || 0)
                                            }
                                        />
                                    </div>
                                </div>
                                
                                <button
                                    className="nw-btn-remove-set"
                                    onClick={() => removeSet(ex.localId, set.localId)}
                                    title="Remove set"
                                >
                                    ✕
                                </button>
                            </div>
                        ))}
                    </div>

                    <button className="nw-btn-add-set" onClick={() => addSet(ex.localId)}>
                        + Add Set
                    </button>
                </div>
            ))}

            {/* Feedback */}
            {error && <p className="nw-error">{error}</p>}
            {success && <p className="nw-success">{success}</p>}

            {/* Save / cancel */}
            {exercises.length > 0 && (
                <div className="nw-actions">
                    <button className="nw-btn-primary" onClick={saveWorkout} disabled={saving}>
                        {saving ? "Saving…" : "✓ Complete Workout"}
                    </button>
                    <button className="nw-btn-secondary" onClick={() => navigate("/home")}>
                        Cancel
                    </button>
                </div>
            )}
        </div>
    );
}

export default NewWorkout;

