namespace OgeApp.Entyties
{
    public class Topic
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int TaskNumber { get; set; }
        public int DoneTasks { get; set; } = 0;
        public double DoneTaskProcent { get; set; }

    }
}
