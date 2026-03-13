﻿
-- Adds unique constraints required for idempotent test data insertion.
    
-- Constraint on tblUserCredentials (fldUsername)
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'uq_credentials_username') THEN
ALTER TABLE tblUserCredentials ADD CONSTRAINT uq_credentials_username UNIQUE (fldUsername);
END IF;
END $$;

-- Constraint on tblUser (fldCredentialsID)
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'uq_user_credentials') THEN
ALTER TABLE tblUser ADD CONSTRAINT uq_user_credentials UNIQUE (fldCredentialsID);
END IF;
END $$;


-- Constraint on tblWorkout (fldDateOfWorkout, fldName)
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'uq_workout_date_name') THEN
ALTER TABLE tblWorkout ADD CONSTRAINT uq_workout_date_name UNIQUE (fldDateOfWorkout, fldName);
END IF;
END $$;

-- Constraint on tblUserWorkout (fldUserID, fldWorkoutID)
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'uq_user_workout') THEN
ALTER TABLE tblUserWorkout ADD CONSTRAINT uq_user_workout UNIQUE (fldUserID, fldWorkoutID);
END IF;
END $$;

-- Constraint on tblExercise (fldName)
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'uq_exercise_name') THEN
ALTER TABLE tblExercise ADD CONSTRAINT uq_exercise_name UNIQUE (fldName);
END IF;
END $$;

-- Constraint on tblSet (fldExerciseID, fldWeight, fldReps, fldRestBetweenSet)
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'uq_set_definition') THEN
ALTER TABLE tblSet ADD CONSTRAINT uq_set_definition UNIQUE (fldExerciseID, fldWeight, fldReps, fldRestBetweenSet);
END IF;
END $$;

-- Constraint on tblWorkoutSet (fldSetID, fldWorkoutID)
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'uq_workout_set') THEN
ALTER TABLE tblWorkoutSet ADD CONSTRAINT uq_workout_set UNIQUE (fldSetID, fldWorkoutID);
END IF;
END $$;