namespace workout_progress.Models
{
public class Exercises_NameandLifts
{
    public List<string> Name { get; set; }
    public List<string> Lifts { get; set; }
    //public List<string[]>

    public Exercises_NameandLifts()
    {
        Name = new List<string>();
        Lifts = new List<string>();
    }
}
}
