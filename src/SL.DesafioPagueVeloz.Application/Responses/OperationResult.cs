namespace SL.DesafioPagueVeloz.Application.Responses
{
    public class OperationResult<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = new();
        public DateTime Timestamp { get; set; }

        public OperationResult()
        {
            Timestamp = DateTime.UtcNow;
        }

        public static OperationResult<T> SuccessResult(T data, string message = "Operação realizada com sucesso")
        {
            return new OperationResult<T>
            {
                Success = true,
                Data = data,
                Message = message
            };
        }

        public static OperationResult<T> FailureResult(string message, List<string>? errors = null)
        {
            return new OperationResult<T>
            {
                Success = false,
                Message = message,
                Errors = errors ?? new List<string>()
            };
        }

        public static OperationResult<T> FailureResult(string message, string error)
        {
            return new OperationResult<T>
            {
                Success = false,
                Message = message,
                Errors = new List<string> { error }
            };
        }
    }
}
