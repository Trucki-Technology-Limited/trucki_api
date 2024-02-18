namespace trucki.Entities
{
    public class Routes : BaseClass
    {
        public string Name { get; set; }

        public string FromRoute { get; set; }

        public string ToRoute { get; set; }

        public float Price { get; set; }

        public bool IsActive { get; set; }
        
        public Business? User { get; set; }

    }
}
