namespace WebCommander.Commands
{
    public class CommandResult
    {
        public string Message { get; set; }
        public string Error { get; set; }
        public string? TaskId { get; set; }
        public bool IsSuccess {get;set;}

        public CommandResult Succeed(string message = null, string taskId = null)
        {
            this.IsSuccess = true;
            this.Message = message; 
            this.Error = null;
            this.TaskId = taskId;
            return this;
        }

        public CommandResult Failed(string error = null)
        {
            this.IsSuccess = false;
            this.Error = error;
            this.TaskId = null;
            this.Message = null;
            return this;
        }
    }
}