using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcomReviews
{
    public class AppSettings
    {
        public static string BrandID = "";
        public static string ServiceType = "";
        public static string SinceDays_Post = "";
        public static string SinceDays_Reviews = "";
        public static string ConnectionString = "";
        public static string TelementryConnectionString = "";
        public static string ThreadCount = "";
        public static string ThreadHold = "";
        public static string ThreadWait = "";
        public static string spiderman_token = "";
        public static string JobIDURL = "";
        public static string ReviewURL = "";
        public static string ServerUrl = "";
        public static IConfigurationBuilder builder = null;
        public static void FillAppSettingData()
        {
            builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("AppSettings.json");

            var configuration = builder.Build();
            try
            {
                ConnectionString = configuration["ConnectionStrings:ConnectionString"];
                TelementryConnectionString = configuration["ConnectionStrings:TelementryConnectionString"];
                BrandID = configuration["BrandID"];
                SinceDays_Post = configuration["SinceDays_Post"];
                SinceDays_Reviews = configuration["SinceDays_Reviews"];
                ThreadCount = configuration["ThreadCount"];
                ThreadHold = configuration["ThreadHold"];
                ThreadWait = configuration["ThreadWait"];
                spiderman_token = configuration["spiderman_token"];
                JobIDURL = configuration["JobIDURL"];
                ReviewURL = configuration["ReviewURL"];
                ServerUrl = configuration["ServerUrl"];
                ServiceType = configuration["ServiceType"];
            }
            catch (Exception ex)
            {
                Program.LogGeneralError(ex,"Error while reading config data",Program.ServiceName);
            }
        }
    }
}
