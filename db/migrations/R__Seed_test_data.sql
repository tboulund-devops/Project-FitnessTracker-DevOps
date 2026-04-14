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

-- Insert a test workout only if one with the same demo date+name does not already exist
INSERT INTO tblWorkout (fldDateOfWorkout, fldName)
SELECT CURRENT_DATE - INTERVAL '2 days', 'Morning Chest Day'
WHERE NOT EXISTS (
    SELECT 1
    FROM tblWorkout
    WHERE fldDateOfWorkout = CURRENT_DATE - INTERVAL '2 days'
      AND fldName = 'Morning Chest Day'
);

-- Link the user to one deterministic workout record for the demo data
INSERT INTO tblUserWorkout (fldUserID, fldWorkoutID)
SELECT u.fldUserID, w.fldWorkoutID
FROM tblUser u
CROSS JOIN LATERAL (
    SELECT fldWorkoutID
    FROM tblWorkout
    WHERE fldName = 'Morning Chest Day'
      AND fldDateOfWorkout = CURRENT_DATE - INTERVAL '2 days'
    ORDER BY fldWorkoutID
    LIMIT 1
) w
WHERE u.fldName = 'John Doe'
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

-- Link the set to one deterministic workout record for the demo data
INSERT INTO tblWorkoutSet (fldSetID, fldWorkoutID)
SELECT s.fldSetID, w.fldWorkoutID
FROM tblSet s
CROSS JOIN LATERAL (
    SELECT fldWorkoutID
    FROM tblWorkout
    WHERE fldName = 'Morning Chest Day'
      AND fldDateOfWorkout = CURRENT_DATE - INTERVAL '2 days'
    ORDER BY fldWorkoutID
    LIMIT 1
) w
WHERE s.fldWeight = 185
  AND s.fldReps = 10
  AND s.fldRestBetweenSet = 90
ON CONFLICT (fldSetID, fldWorkoutID) DO NOTHING;