namespace trucki.DTOs
{
    public class GenericResponse<T>
    {
        public string ResponseCode { get; set; }
        public string ResponseMessage { get; set; }
        public bool IsSuccessful { get; set; }
        public T Data { get; set; }
    }
}
