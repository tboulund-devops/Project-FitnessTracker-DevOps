-- Allow users to log multiple workouts with the same name on the same day
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'uq_workout_date_name') THEN
        ALTER TABLE tblWorkout DROP CONSTRAINT uq_workout_date_name;
    END IF;
END $$;

