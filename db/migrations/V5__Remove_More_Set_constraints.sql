-- Drop the unique constraint on tblSet that prevents duplicate sets
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'uq_set_definition') THEN
ALTER TABLE tblSet DROP CONSTRAINT uq_set_definition;
RAISE NOTICE 'Dropped constraint uq_set_definition from tblSet';
ELSE
        RAISE NOTICE 'Constraint uq_set_definition does not exist, skipping';
END IF;
END $$;