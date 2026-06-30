using AjoCoreBackend.Application.Interfaces.Services;
using Hangfire;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace AjoCoreBackend.Infrastructure.Services
{
    public class HangfireBackgroundService : IHangfireBackGroundService
    {
        public void ScheduleTask<T>(Expression<Func<T, Task>> methodCall,DateTime scheduleAt)
        {
             BackgroundJob.Schedule<T>(methodCall, scheduleAt);

        }

        public void EnqueTask<T>(Expression<Func<T,Task>> method)
        {
            BackgroundJob.Enqueue<T>(method);
        }
    }
}
