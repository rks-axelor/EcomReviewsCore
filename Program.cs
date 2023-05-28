using EcomReviews;
using EcomReviews.Classes;
using LocobuzzTelemetry;
using System;
using System.Configuration;
using System.Diagnostics;

class Program
{
    public static string ServiceName = "";
    public static ILogger logger = null;
    public static Dictionary<long, Thread> lstrunthreads = new Dictionary<long, Thread>();
    
    static void Main(string[] args)
    {
        AppSettings.FillAppSettingData();
        InitializeTelemetry();        
        AppDomain currentDomain = AppDomain.CurrentDomain;
        ServiceName = System.Reflection.Assembly.GetEntryAssembly().Location;
        ServiceName = ServiceName.Substring(ServiceName.LastIndexOf("\\")).Replace(".exe", "").Replace("\\", "");
        currentDomain.UnhandledException += new UnhandledExceptionEventHandler(MyHandler);
        currentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);
        logger.LogEvent("Service Started", ServiceName, null);
        string BrandID = AppSettings.BrandID ?? "";
        List<BrandDetails> lstBrands = new List<BrandDetails>();
        lstBrands = DatabaseOperations.GetEcomSettings();
        if(BrandID != null && BrandID.Length > 0)
        {
            lstBrands = lstBrands.Where(b => b.BrandID == Convert.ToInt64(BrandID)).ToList();
        }
        Console.WriteLine("Brand Found :"+lstBrands.Count);
        Console.WriteLine("Backup Testing");
        foreach (BrandDetails brand in lstBrands)
        {
            Console.WriteLine("Running For Brand :" + brand.BrandName);
            List<ProductDetails> lstProducts = DatabaseOperations.GetProductList(brand);
            Console.WriteLine("Product Found :" + lstProducts.Count);
            foreach (ProductDetails product in lstProducts)
            {
                Console.WriteLine("Running For Product");
                if (!CheckThreadStatus_Main(product.ProductID))
                {
                    StartThread:
                        if (lstrunthreads.Count < Convert.ToInt32(AppSettings.ThreadCount))
                        {
                            if (product.JobID == 0 && Convert.ToInt32(AppSettings.ServiceType) == 1)
                            {                           
                                ThreadStart starter = delegate
                                {
                                    BusinessLogics.GetJobID(product,brand);
                                };
                                Thread thread = new Thread(starter);
                                thread.Start();
                                lstrunthreads[product.ProductID] = thread;
                                Thread.Sleep(new TimeSpan(0, 0, Convert.ToInt32(AppSettings.ThreadWait)));
                            }
                            else if(product.JobID != 0 && Convert.ToInt32(AppSettings.ServiceType) == 2)
                            {
                                ThreadStart starter = delegate
                                {
                                    BusinessLogics.GetEcomReviews(product,brand);
                                };
                                Thread thread = new Thread(starter);
                                thread.Start();
                                lstrunthreads[product.ProductID] = thread;
                                Thread.Sleep(new TimeSpan(0, 0, Convert.ToInt32(AppSettings.ThreadWait)));
                            }                        
                        }
                        else
                        {
                            Thread.Sleep(new TimeSpan(0, 0, Convert.ToInt32(AppSettings.ThreadHold)));
                            goto StartThread;
                        }    
                }
                Console.WriteLine("Completed For Brand "+brand.BrandName);
            }
        }
        Console.WriteLine("First Iteration Completed");
        Console.ReadLine();
    }
    public static bool CheckThreadStatus_Main(long ID)
    {
        bool IsRunning = false;
        try
        {
            if (lstrunthreads.ContainsKey(ID))
            {
                var th = lstrunthreads[ID];
                if (th.ThreadState == System.Threading.ThreadState.Running || th.ThreadState == System.Threading.ThreadState.WaitSleepJoin)
                {
                    return true;
                }
                else
                {
                    try
                    {
                        lock (lstrunthreads)
                        {
                            lstrunthreads.Remove(ID);
                        }
                        th.Abort();
                    }
                    catch(ThreadAbortException ex)
                    {

                    }
                    catch (Exception ex)
                    {
                        LogGeneralError(ex, "Error in checking thread status", ServiceName);
                    }                   
                }
            }
        }
        catch (Exception ex)
        {
            LogGeneralError(ex, "Error in checking thread status", ServiceName); 
        }
        return IsRunning;
    }
    static void MyHandler(object sender, UnhandledExceptionEventArgs args)
    {
        Exception e = (Exception)args.ExceptionObject;
        Console.WriteLine("UnHandled Error :" + e.Message + " " + Environment.NewLine + " Serivce Will Restart in 20 sec");
        logger.LogEvent("Service Stopped", ServiceName);
        Thread.Sleep(new TimeSpan(0, 0, 20));
        Process.Start(System.Reflection.Assembly.GetEntryAssembly().Location);
        Process.GetCurrentProcess().Kill();

    }
    static void CurrentDomain_ProcessExit(object sender, EventArgs e)
    {
        logger.LogEvent("Service Stopped", ServiceName);
    }

    private static void InitializeTelemetry()
    {
        logger = Logger.Instance;
    }
    public static void LogGeneralError(Exception StackTrace, string Message, String ServiceName)
    {
        logger.LogError(StackTrace, Message, ServiceName);
    }

    public static void LogSpecificError(Exception StackTrace, Dictionary<string, string> properties, string ServiceName)
    {
        logger.LogError(StackTrace, properties, ServiceName);
    }



}
