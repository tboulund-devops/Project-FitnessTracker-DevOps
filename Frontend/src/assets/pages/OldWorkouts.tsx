import "../../index.css";
import {useEffect, useState} from "react";
import type {Workout} from "../../Domain/Workout";

interface BackendSet {
    exerciseID: number;
    exerciseName: string | null;
    weight: number;
    reps: number;
}

interface BackendWorkout {
    workoutID: number;
    dateOfWorkout: string;
    sets?: BackendSet[];
}

function mapExerciseSets(sets?: BackendSet[]) {
    if (!sets || sets.length === 0) {
        return [];
    }

    const exerciseMap = new Map<number, { name: string; sets: { weight: number; repetitions: number }[] }>();
    for (const set of sets) {
        const exerciseId = set.exerciseID;
        if (!exerciseMap.has(exerciseId)) {
            exerciseMap.set(exerciseId, {
                name: set.exerciseName || `Exercise ${exerciseId}`,
                sets: [],
            });
        }

        exerciseMap.get(exerciseId)?.sets.push({
            weight: set.weight,
            repetitions: set.reps,
        });
    }

    return Array.from(exerciseMap.values());
}

function toWorkout(backendWorkout: BackendWorkout): Workout {
    return {
        id: backendWorkout.workoutID,
        date: backendWorkout.dateOfWorkout,
        exercises: mapExerciseSets(backendWorkout.sets),
    };
}

function OldWorkouts() {
    const [workouts, setWorkouts] = useState<Workout[] | null>(null);
    const [error, setError] = useState("");
    const [loading, setLoading] = useState(true);
  
    const userId = localStorage.getItem("userID");

    useEffect(() => {
        async function fetchWorkouts() {
            try {
                const response = await fetch(`/api/workout/APIWorkout/GetWorkoutsByUserID/${userId}`);
                if (!response.ok) {
                    const message = await response.text();
                    setError(message || "Failed to load workouts");
                    return;
                }

                const backendData: BackendWorkout[] = await response.json();
                setWorkouts(backendData.map(toWorkout));
            } catch (error) {
                console.error("Fetch error:", error);
                setError("Could not reach the server. Please try again later.");
            } finally {
                setLoading(false);
            }
        }

        fetchWorkouts();
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
            <h1>Old Workouts</h1>
            <p className="profile-loading">Loading workouts…</p>
          </div>
        </div>
      );
    }
  
    if (error) {
      return (
        <div className="profile-container">
          <div className="profile-card">
            <h1>Old Workouts</h1>
            <p className="profile-error">{error}</p>
          </div>
        </div>
      );
    }
  
    return (
      <div className="home-container">
        <div className="profile-card">
          <h1>Old Workouts</h1>
          {workouts && workouts.length > 0 ? (
            workouts.map((workout, workoutIndex) => (
              <div key={workoutIndex} className="workout-entry">
                <h2>{formatDate(workout.date)}</h2>
                {workout.exercises.map((exercise, exerciseIndex) => (
                  <div key={exerciseIndex}>
                    <p>{exercise.name}</p>
                    {exercise.sets.map((set, setIndex) => (
                      <p key={setIndex}>
                        Set {setIndex + 1}: {set.repetitions} reps at {set.weight} kg
                      </p>
                    ))}
                  </div>
                ))}
              </div>
            ))
          ) : (
            <p>No workouts found.</p>
          )}
        </div>
      </div>
    );
}
export default OldWorkouts;
