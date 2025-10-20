using System;
using System.Collections.Generic;
using System.Text;

namespace RestX.WebApp.Services.Interfaces
{
    public interface IExceptionHandler
    {
        void RaiseException(Exception ex, string customMessage = "");
    }
}
