namespace trucki.Entities
{
    public class Routes : BaseClass
    {
        public string Name { get; set; }

        public string FromRoute { get; set; }

        public string ToRoute { get; set; }
        public string Ntons { get; set; }

        public float Price { get; set; }

        public bool IsActive { get; set; }
        public float Gtv { get; set; }
        
        public Business? Business { get; set; }
    }
}
