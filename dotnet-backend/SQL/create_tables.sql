-- Create Users table
CREATE TABLE IF NOT EXISTS Users (
    userID SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL, 
    email VARCHAR(255) UNIQUE NOT NULL,
    refresh_token VARCHAR(255),
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
CREATE INDEX IF NOT EXISTS idx_user_exercise ON exercises_record (userID);

CREATE TABLE IF NOT EXISTS Male_Standard_Data (
    name VARCHAR(255) NOT NULL,
    level VARCHAR(50) NOT NULL CHECK (level IN ('Beginner', 'Novice', 'Intermediate', 'Advanced', 'Elite')),
    "110" VARCHAR(50),
    "120" VARCHAR(50),
    "130" VARCHAR(50),
    "140" VARCHAR(50),
    "150" VARCHAR(50),
    "160" VARCHAR(50),
    "170" VARCHAR(50),
    "180" VARCHAR(50),
    "190" VARCHAR(50),
    "200" VARCHAR(50),
    "210" VARCHAR(50),
    "220" VARCHAR(50),
    "230" VARCHAR(50),
    "240" VARCHAR(50),
    "250" VARCHAR(50),
    "260" VARCHAR(50),
    "270" VARCHAR(50),
    "280" VARCHAR(50),
    "290" VARCHAR(50),
    "300" VARCHAR(50),
    "310" VARCHAR(50)
);

            