-- Allow users to log multiple sets for the same workout
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'uq_workout_date_name') THEN
ALTER TABLE tblWorkout DROP CONSTRAINT uq_workout_date_name;
END IF;
END $$;