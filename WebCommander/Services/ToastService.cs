using System;

namespace WebCommander.Services
{
    public enum ToastType
    {
        Info,
        Success,
        Warning,
        Error
    }

    public class ToastMessage
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public ToastType Type { get; set; } = ToastType.Info;
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }

    public class ToastService
    {
        public event Action<ToastMessage>? OnShow;

        public void ShowToast(string title, string message, ToastType type)
        {
            OnShow?.Invoke(new ToastMessage
            {
                Title = title,
                Message = message,
                Type = type
            });
        }

        public void ShowSuccess(string message, string title = "Success")
        {
            ShowToast(title, message, ToastType.Success);
        }

        public void ShowError(string message, string title = "Error")
        {
            ShowToast(title, message, ToastType.Error);
        }

        public void ShowInfo(string message, string title = "Info")
        {
            ShowToast(title, message, ToastType.Info);
        }

        public void ShowWarning(string message, string title = "Warning")
        {
            ShowToast(title, message, ToastType.Warning);
        }
    }
}
