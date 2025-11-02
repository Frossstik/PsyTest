namespace PsyTest.ServiceIdentity.Common
{
    public sealed class AppResult
    {
        public bool Succeeded { get; }
        public string Message { get; }
        public IEnumerable<string> Errors { get; }

        private AppResult(bool ok, string message, IEnumerable<string>? errors = null)
        {
            Succeeded = ok;
            Message = message;
            Errors = errors ?? Array.Empty<string>();
        }

        public static AppResult Ok(string message = "OK") => new(true, message);
        public static AppResult Fail(params string[] errors) =>
            new(false, "FAIL", errors);
    }
}
