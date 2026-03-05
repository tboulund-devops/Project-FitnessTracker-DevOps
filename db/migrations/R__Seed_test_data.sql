-- R__Seed_test_data.sql
-- Repeatable migration: inserts or updates test data.
-- Uses ON CONFLICT with unique constraints to remain idempotent.

-- Insert test credentials (if not already present)
INSERT INTO tblUserCredentials (fldUsername, fldPassword)
VALUES ('test', 'test')
    ON CONFLICT (fldUsername) DO NOTHING;

-- Insert test user (links to the credentials above)
INSERT INTO tblUser (fldCredentialsID, fldName, fldEmail, fldTotalWorkoutTime)
SELECT fldCredentialsID, 'John Doe', 'john.doe@email.com', 125
FROM tblUserCredentials
WHERE fldUsername = 'test'
    ON CONFLICT (fldCredentialsID) DO NOTHING;   -- unique constraint ensures one user per credentials

-- Insert a test workout with a dynamic date (two days ago from today)
INSERT INTO tblWorkout (fldDateOfWorkout, fldName)
VALUES (CURRENT_DATE - INTERVAL '2 days', 'Morning Chest Day')   
    ON CONFLICT (fldDateOfWorkout, fldName) DO NOTHING;

-- Link the user to the workout (if not already linked)
INSERT INTO tblUserWorkout (fldUserID, fldWorkoutID)
SELECT u.fldUserID, w.fldWorkoutID
FROM tblUser u, tblWorkout w
WHERE u.fldName = 'John Doe' AND w.fldName = 'Morning Chest Day'
  AND w.fldDateOfWorkout = CURRENT_DATE - INTERVAL '2 days'   -- match the exact date
ON CONFLICT (fldUserID, fldWorkoutID) DO NOTHING;

-- Insert test exercise
INSERT INTO tblExercise (fldName, fldDescription)
VALUES ('Barbell Bench Press', 'Compound exercise targeting chest, shoulders, and triceps')
    ON CONFLICT (fldName) DO NOTHING;

-- Insert test set (uses the exercise just inserted)
INSERT INTO tblSet (fldExerciseID, fldWeight, fldReps, fldRestBetweenSet)
SELECT fldExerciseID, 185, 10, 90
FROM tblExercise
WHERE fldName = 'Barbell Bench Press'
    ON CONFLICT (fldExerciseID, fldWeight, fldReps, fldRestBetweenSet) DO NOTHING;

-- Link the set to the workout (must match the exact date)
INSERT INTO tblWorkoutSet (fldSetID, fldWorkoutID)
SELECT s.fldSetID, w.fldWorkoutID
FROM tblSet s, tblWorkout w
WHERE s.fldWeight = 185
  AND s.fldReps = 10
  AND s.fldRestBetweenSet = 90
  AND w.fldName = 'Morning Chest Day'
  AND w.fldDateOfWorkout = CURRENT_DATE - INTERVAL '2 days'   -- ensure correct workout
ON CONFLICT (fldSetID, fldWorkoutID) DO NOTHING;