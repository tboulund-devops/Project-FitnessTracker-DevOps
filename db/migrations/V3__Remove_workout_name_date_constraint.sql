-- Allow users to log multiple workouts with the same name on the same day
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'uq_set_definition') THEN
        ALTER TABLE tblSet DROP CONSTRAINT uq_set_definition;
    END IF;
END $$;

