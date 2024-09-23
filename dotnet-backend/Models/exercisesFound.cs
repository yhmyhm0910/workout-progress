namespace workout_progress.Models
{
public class Exercises
{
    public List<string> Name { get; set; }
    public List<string> Lifts { get; set; }
    //public List<string[]>

    public Exercises()
    {
        Name = new List<string>();
        Lifts = new List<string>();
    }
}
}
