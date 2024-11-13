namespace workout_progress.Models
{
    public class Exercises_NameandLifts
    {
        public List<string> Name { get; set; }
        public List<string> Lifts { get; set; }

        public Exercises_NameandLifts()
        {
            Name = new List<string>();
            Lifts = new List<string>();
        }
    }

    public class Exercises_NameLiftsAndStandards
    {
        public Exercises_NameandLifts ExercisesFound { get; set; }
        public List<List<List<string>>> MaleData { get; set; }

        public Exercises_NameLiftsAndStandards()
        {
            ExercisesFound = new Exercises_NameandLifts();
            MaleData = new List<List<List<string>>>();
        }
    }
}
