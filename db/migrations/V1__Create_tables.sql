
-- Initial schema creation for workout application
-- Includes unique constraints to support idempotent test data insertion

-- Table: tblUserCredentials (no dependencies)
CREATE TABLE IF NOT EXISTS tblUserCredentials (
    fldCredentialsID SERIAL PRIMARY KEY,
    fldUsername VARCHAR(100) NOT NULL,
    fldPassword VARCHAR(100) NOT NULL,
    CONSTRAINT uq_credentials_username UNIQUE (fldUsername)
    );

-- Table: tblUser (depends on tblUserCredentials)
CREATE TABLE IF NOT EXISTS tblUser (
    fldUserID SERIAL PRIMARY KEY,
    fldCredentialsID INT NOT NULL,
    fldName VARCHAR(100) NOT NULL,
    fldEmail VARCHAR(100) NOT NULL,
    fldTotalWorkoutTime INT,
    fldTimeOfRegistration TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (fldCredentialsID) REFERENCES tblUserCredentials(fldCredentialsID),
    CONSTRAINT uq_user_credentials UNIQUE (fldCredentialsID)   -- one user per credentials
    );

-- Table: tblWorkout (no dependencies)
CREATE TABLE IF NOT EXISTS tblWorkout (
    fldWorkoutID SERIAL PRIMARY KEY,
    fldDateOfWorkout DATE NOT NULL,
    fldName VARCHAR(100) NOT NULL,
    CONSTRAINT uq_workout_date_name UNIQUE (fldDateOfWorkout, fldName)
    );

-- Table: tblUserWorkout (depends on tblUser and tblWorkout)
CREATE TABLE IF NOT EXISTS tblUserWorkout (
    fldUserWorkoutID SERIAL PRIMARY KEY,
    fldUserID INT NOT NULL,
    fldWorkoutID INT NOT NULL,
    FOREIGN KEY (fldUserID) REFERENCES tblUser(fldUserID),
    FOREIGN KEY (fldWorkoutID) REFERENCES tblWorkout(fldWorkoutID),
    CONSTRAINT uq_user_workout UNIQUE (fldUserID, fldWorkoutID)
    );

-- Table: tblExercise (no dependencies)
CREATE TABLE IF NOT EXISTS tblExercise (
    fldExerciseID SERIAL PRIMARY KEY,
    fldName VARCHAR(100) NOT NULL,
    fldDescription VARCHAR(100) NOT NULL,
    CONSTRAINT uq_exercise_name UNIQUE (fldName)
    );

-- Table: tblSet (depends on tblExercise)
CREATE TABLE IF NOT EXISTS tblSet (
    fldSetID SERIAL PRIMARY KEY,
    fldExerciseID INT NOT NULL,
    fldWeight INT NOT NULL,
    fldReps INT NOT NULL,
    fldRestBetweenSet INT NOT NULL,
    FOREIGN KEY (fldExerciseID) REFERENCES tblExercise(fldExerciseID),
    CONSTRAINT uq_set_definition UNIQUE (fldExerciseID, fldWeight, fldReps, fldRestBetweenSet)
    );

-- Table: tblWorkoutSet (depends on tblSet and tblWorkout)
CREATE TABLE IF NOT EXISTS tblWorkoutSet (
    fldWorkoutSetID SERIAL PRIMARY KEY,
    fldSetID INT NOT NULL,
    fldWorkoutID INT NOT NULL,
    FOREIGN KEY (fldSetID) REFERENCES tblSet(fldSetID),
    FOREIGN KEY (fldWorkoutID) REFERENCES tblWorkout(fldWorkoutID),
    CONSTRAINT uq_workout_set UNIQUE (fldSetID, fldWorkoutID)
    );