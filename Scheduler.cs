using System;
using System.Threading;
using System.Windows.Forms;

public class Scheduler
{
    ListView jobs;
    System.Threading.Timer timer;
    System.Threading.TimerCallback timerCallback;
    delegate void TimerTickCallback(Object obj);
    TimerTickCallback timerTickCallback;
    static int maxJobsRunning = 2;
    public static int numJobsRunning = 0;
    public static int numNetJobsRunning = 0;

    public Scheduler(ListView _jobs)
    {
        jobs = _jobs;
        timerCallback = new System.Threading.TimerCallback(TimerTick);
        timerTickCallback = new TimerTickCallback(InvokedTimerTick);
        timer = new System.Threading.Timer(timerCallback, this, 0, 200);
    }

    public static void InvokedTimerTick(Object obj)
    {
        ListView jobs = (ListView)obj;

        foreach (JobEntry je in jobs.Items)
        {
            if (je.stat == Status.queued && numJobsRunning < maxJobsRunning)
            {
                if (je._inputFile.SaveFile(je.InputChanged()))
                    je.StartJob(false);
            }
            else if (!je.Locked && je.stat == Status.rqueued && numNetJobsRunning < maxJobsRunning)
            {
                if (je._inputFile.SaveFile(je.InputChanged()))
                {
                    je.Locked = true;
                    je.StartRemoteJob();
                }
            }
        }
    }

    public static void TimerTick(Object obj)
    {
        //This procedure is called in a separate thread
        //Pass the tick to the tick procedure of the main thread
        try
        {
            Scheduler sched = (Scheduler)obj;
            sched.jobs.Invoke(sched.timerTickCallback, new Object[] { sched.jobs });
        }
        catch
        {
        }
    }

    public void Dispose()
    {
        timer.Dispose();
    }
}
