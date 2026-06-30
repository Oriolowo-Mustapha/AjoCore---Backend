using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace AjoCoreBackend.Application.Interfaces.Services
{
    public interface IHangfireBackGroundService
    {
      void ScheduleTask<T>(Expression<Func<T, Task>> methodCall, DateTime scheduleAt);
      void EnqueTask<T>(Expression<Func<T, Task>> method);
    }
}
