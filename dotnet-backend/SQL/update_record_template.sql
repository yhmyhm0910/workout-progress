UPDATE exercises_record
SET 
    exercise_name = COALESCE(@ExerciseName, exercise_name),
    one_rep_max = COALESCE(@OneRepMax, one_rep_max),
    unit = COALESCE(@Unit, unit),
    date_of_exercise = COALESCE(@DateOfExercise, date_of_exercise)
WHERE recordID = @RecordID;
