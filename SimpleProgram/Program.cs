using System;
using System.ComponentModel;    //
using System.Threading;         //

namespace ConsoleBackgroundMaker
{
    class DoBackgroundwork
    {
        BackgroundWorker bgWorker = new BackgroundWorker();

        public long BackgroundTotal { get; private set; }
        public bool CompleteNormally { get; private set; }

        //构造方法
        public DoBackgroundwork()
        {
            //设置BackgroundWork属性
            bgWorker.WorkerReportsProgress = true;
            bgWorker.WorkerSupportsCancellation = true;

            //把处理程序连接到BackgroundWorker对象
            bgWorker.DoWork += DoWork_Handler;
            bgWorker.ProgressChanged += ProgressChanged_Handler;
            bgWorker.RunWorkerCompleted += RunWorkerCompleted_Handler;
        }

        public void StartWorker()
        {
            if (!bgWorker.IsBusy)
                bgWorker.RunWorkerAsync();
        }

        //计算从0到输入值的总数
        public static long CalculateTheSequence(long value)
        {
            long total = 0;
            for (int i = 0; i < value; i++)
                total += i;
            return total;
        }

        public void DoWork_Handler(object sender, DoWorkEventArgs args)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            //进行后台计算
            long total = 0;
            for (int i = 0; i <= 5; i++)
            {
                //每一次迭代都检查是否应该取消了
                if (worker.CancellationPending)
                {
                    args.Cancel = true;
                    worker.ReportProgress(-1);
                    break;
                }
                else
                {
                    //如果没有被取消则继续计算
                    total += CalculateTheSequence(i * 10000000);
                    worker.ReportProgress(i * 20);
                    //让程序慢点，这样输出可以漂亮点
                    Thread.Sleep(300);
                }
            }
            args.Result = total;
        }

        //处理后台线程的输入
        public void ProgressChanged_Handler(object sender, ProgressChangedEventArgs args)
        {
            string output = args.ProgressPercentage == -1 ? "      canclled"
                                                         : string.Format("       {0}%", args.ProgressPercentage);
            Console.WriteLine(output);
        }

        //后台线程完成之后，总结并保存总和
        public void RunWorkerCompleted_Handler(object sender, RunWorkerCompletedEventArgs args)
        {
            CompleteNormally = !args.Cancelled;
            BackgroundTotal = args.Cancelled
                                ? 0
                                : (long)args.Result;
        }

        public void Cancle()
        {
            if (bgWorker.IsBusy)
                bgWorker.CancelAsync();
        }
    }

    class Program
    {
        static void Main()
        {
            GiveInstructionsToTheUser();
            OutputTheSummaryHeaders();

            //创建并打开Background Worker
            DoBackgroundwork bgw = new DoBackgroundwork();
            bgw.StartWorker();

            //在主线程启动计算。对于每一次循环，检查是否用户已经取消了后台线程。
            //在计算之后，进行短暂的休眠，这样程序可以慢一点，使得主线程不会比后台更快
            long mainTotal = 0;
            for (int i = 0; i < 5; i++)
            {
                if (Program.CheckForCancelInput())
                    bgw.Cancle();
                mainTotal += DoBackgroundwork.CalculateTheSequence(100000000);
                Thread.Sleep(200);
                Console.WriteLine("   {0}%",(i+1)*20);
            }
            SummarizeResults(bgw, mainTotal);
            Console.ReadLine();
        }

        private static void GiveInstructionsToTheUser()
        {
            Console.WriteLine("Press <Enter> to start background worker.");
            Console.WriteLine("Press <Enter> again to cancel background worker.");
            Console.ReadLine();
        }

        private static void OutputTheSummaryHeaders()
        {
            Console.WriteLine("       Main Background");
            Console.WriteLine("----------------------");
        }

        private static void SummarizeResults(DoBackgroundwork bgw, long mainTotal)
        {
            if (bgw.CompleteNormally)
            {
                Console.WriteLine("\nBackground completed Normally");
                Console.WriteLine("Background total = {0}", bgw.BackgroundTotal);
            }
            else
            {
                Console.WriteLine("\nBackground     Cancelled");
            }
            Console.WriteLine("Main total = {0}", mainTotal);
        }

        private static bool CheckForCancelInput()
        {
            bool doCancel = Console.KeyAvailable;
            if (doCancel)
                Console.ReadKey();
            return doCancel;
        }
    }
}