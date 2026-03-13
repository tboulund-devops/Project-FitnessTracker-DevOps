import "../../index.css";
import {useEffect, useState} from "react";
import type {Workout} from "../../Domain/Workout";
function OldWorkouts() {
    const [workouts, setWorkouts] = useState<Workout[] | null>(null);
    const [error, setError] = useState("");
    const [loading, setLoading] = useState(true);
  
    //TODO replace with actual logged-in user ID (e.g. from auth context)
    const userId = 1;

    useEffect(() => {
        async function fetchWorkouts() {
            try {
                const response = await fetch(`/api/workout/APIWorkout/GetWorkoutsByUserID/${userId}`);
                if (response.ok) {
                    const backendData = await response.json();

                    // Transform backend data to match frontend Workout interface
                    const transformedWorkouts: Workout[] = backendData.map((backendWorkout: any) => {
                        // Group sets by exercise
                        const exerciseMap = new Map();

                        // If there are sets, group them by ExerciseID
                        if (backendWorkout.sets && backendWorkout.sets.length > 0) {
                            backendWorkout.sets.forEach((set: any) => {
                                const exerciseId = set.exerciseID;
                                const exerciseName = set.exerciseName;

                                if (!exerciseMap.has(exerciseId)) {
                                    exerciseMap.set(exerciseId, {
                                        name: exerciseName||`Exercise ${exerciseId}`, 
                                        sets: []
                                    });
                                }

                                // Add the set to the exercise
                                exerciseMap.get(exerciseId).sets.push({
                                    weight: set.weight,
                                    repetitions: set.reps
                                });
                            });
                        }

                        // Convert the map to an array of exercises
                        const exercises = Array.from(exerciseMap.values());

                        // Return the transformed workout
                        return {
                            id: backendWorkout.workoutID,
                            date: backendWorkout.dateOfWorkout,
                            exercises: exercises
                        };
                    });

                    setWorkouts(transformedWorkouts);
                } else {
                    const message = await response.text();
                    setError(message || "Failed to load workouts");
                }
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