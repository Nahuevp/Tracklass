namespace Tracklass.API
{
    public class Response<T>
    {
        public bool IsSuccess { get; set; }

        public T Result { get; set; }

        public string Message { get; set; }
    }
}
