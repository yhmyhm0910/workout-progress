-- Create Users table
CREATE TABLE IF NOT EXISTS Users (
    userID SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL, 
    email VARCHAR(255) UNIQUE NOT NULL,
    created_at TIMESTAMPTZ DEFAULT NOW()
);

-- Create exercises_record table
CREATE TABLE IF NOT EXISTS exercises_record (
    recordID SERIAL PRIMARY KEY,
    userID INT NOT NULL,
    exercise_name VARCHAR(100) NOT NULL,
    one_rep_max FLOAT NOT NULL,
    unit VARCHAR(3) NOT NULL CHECK (unit IN ('lbs', 'kgs')), 
    date_of_exercise DATE NOT NULL, 
    created_at TIMESTAMPTZ DEFAULT NOW(), 

    -- Foreign key constraint to reference Users table
    CONSTRAINT fk_user
    FOREIGN KEY (userID) 
    REFERENCES Users (userID)
    ON DELETE CASCADE -- If a user is deleted, their exercise records are deleted too
);

-- Create index on userID in exercises_record for performance
CREATE INDEX idx_user_exercise ON exercises_record (userID);

            