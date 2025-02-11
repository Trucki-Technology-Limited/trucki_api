using Newtonsoft.Json;

namespace trucki.Models.ResponseModels;

public class ApiResponseModel<T>
{
    public string Message { get; set; }
    public bool IsSuccessful { get; set; }
    public int StatusCode { set; get; }
    public T Data { get; set; }

    public ApiResponseModel(T data, string message, bool isSuccessful, int reponseCode)
    {

        Message = message;
        IsSuccessful = isSuccessful;
        StatusCode = reponseCode;
        Data = data;
    }
    public ApiResponseModel()
    {

    }


    public static ApiResponseModel<T> Fail(string errorMessage, int statusCode)
    {
        return new ApiResponseModel<T> { Message = errorMessage, StatusCode = statusCode, IsSuccessful = false };
    }

    public static ApiResponseModel<T> Success(string successMessage, T data, int statusCode)
    {
        return new ApiResponseModel<T> { Data = data, Message = successMessage, StatusCode = statusCode, IsSuccessful = true };
    }

    public static ApiResponseModel<T> Success(string successMessage, int statusCode)
    {
        return new ApiResponseModel<T> { Message = successMessage, StatusCode = statusCode, IsSuccessful = true };
    }
    public override string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }
}

  public class PagedResponse<T>
    {
        public IEnumerable<T> Data { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }