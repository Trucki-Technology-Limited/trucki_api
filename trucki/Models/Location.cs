namespace trucki.Models
{
    public class Location : BaseClass
    {
        public string Name { get; set; }

        public int NumberOfTons { get; set; }

        public string FromRoute { get; set; }

        public string ToRoute { get; set; }

        public float Price { get; set; }

        public string Address { get; set; }

    }
}
