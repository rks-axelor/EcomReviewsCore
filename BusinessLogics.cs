using EcomReviews.Classes;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaggingBLL;
using TaggingBLL.Classes;
using TaggingBLL.Models;
using BrandDetails = EcomReviews.Classes.BrandDetails;

namespace EcomReviews
{
    public class BusinessLogics
    {
        internal static void GetJobID(ProductDetails product,BrandDetails brand)
        {
            try
            {
                //API calls to get Job ID 
                string apicall = AppSettings.JobIDURL;//"https://app.datashake.com/api/v2/profiles/add?url="
                var client = new RestClient(apicall + product.URL);
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("spiderman-token", AppSettings.spiderman_token);//"0b83fb44d45a025fbca7a6b08ee791ff5e678e21"
                IRestResponse response = client.Execute(request);
                if (response.Content != null && response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var json = response.Content;
                    Console.WriteLine(json);
                    Job JobClass = JsonConvert.DeserializeObject<Job>(json);
                    product.JobID = JobClass.job_id;
                }
                try
                {
                    // Save Job id to Database
                    DatabaseOperations.SaveJobId(product.ProductID, product.JobID,brand);
                }
                catch (Exception ex)
                {
                    Dictionary<string, string> properties = new Dictionary<string, string>();
                    properties.Add("CategoryID", brand.CategoryID.ToString());
                    properties.Add("BrandID", brand.BrandID.ToString());
                    properties.Add("ID", product.ID.ToString());
                    properties.Add("JobID", product.JobID.ToString());
                    properties.Add("Message", "error while api call for jobid");
                    Program.LogSpecificError(ex, properties, Program.ServiceName);
                }                
               // GetEcomReviews(product,brand);
            }
            catch (Exception ex)
            {
                Program.LogGeneralError(ex, "error while api call for jobid", Program.ServiceName);
            }
            finally
            {
                if (Program.lstrunthreads.ContainsKey(product.ProductID))
                {
                    lock (Program.lstrunthreads)
                    {
                        Program.lstrunthreads.Remove(product.ProductID);
                    }
                }
            }

        }

        internal static void GetEcomReviews(ProductDetails product,BrandDetails brand)
        {
            Dictionary<string, string> properties = new Dictionary<string, string>();
            properties.Add("Category ID ", brand.CategoryID.ToString());
            properties.Add("Brand ID ", brand.BrandID.ToString());
            properties.Add("Account ID",product.JobID.ToString());
            Program.logger.LogEvent("Start", Program.ServiceName, properties);
            //API Calls to get reviews
            try
            {
                string apicall = AppSettings.ReviewURL;//"https://app.datashake.com/api/v2/profiles/reviews?job_id="
                var client = new RestClient(apicall + product.JobID);
                client.Timeout = -1;
                var request = new RestRequest(Method.GET);
                request.AddHeader("spiderman-token", AppSettings.spiderman_token);//"0b83fb44d45a025fbca7a6b08ee791ff5e678e21"
                IRestResponse response = client.Execute(request);
                if (response.Content != null && response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    try
                    {
                         Root Response = JsonConvert.DeserializeObject<Root>(response.Content);
                        if (Response != null && Response.review_count > 0)
                        {
                            //iterate on each review and map it according to Review class
                            foreach (Review Review in Response.reviews)
                            {
                                MapAndSaveReview(Review, product.ProductID,brand);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Dictionary<string, string> prop = new Dictionary<string, string>();
                        prop.Add("CategoryID", brand.CategoryID.ToString());
                        prop.Add("BrandID", brand.BrandID.ToString());
                        prop.Add("ID", product.ID.ToString());
                        prop.Add("JobID", product.JobID.ToString());
                        prop.Add("Message", "error in Deserialize response object");
                        Program.LogSpecificError(ex, prop, Program.ServiceName);
                    }                    
                }
                Dictionary<string, string> Properties = new Dictionary<string, string>();
                Properties.Add("Category ID ", brand.CategoryID.ToString());
                Properties.Add("Brand ID ", brand.BrandID.ToString());
                Properties.Add("Account ID", product.JobID.ToString());
                Program.logger.LogEvent("End", Program.ServiceName, Properties);
            }
            catch (Exception ex)
            {
                Dictionary<string, string> Properties = new Dictionary<string, string>();
                Properties.Add("CategoryID", brand.CategoryID.ToString());
                Properties.Add("BrandID", brand.BrandID.ToString());
                Properties.Add("ID", product.ID.ToString());
                Properties.Add("JobID", product.JobID.ToString());
                Properties.Add("Message", "error in api calls for reviews");
                Program.LogSpecificError(ex, Properties, Program.ServiceName);
            }
            finally
            {
                if(Program.lstrunthreads.ContainsKey(product.ProductID))
                {
                    lock (Program.lstrunthreads)
                    {
                        Program.lstrunthreads.Remove(product.ProductID);
                    }
                }
            }
        }

        private static void MapAndSaveReview(Review review,long productID,BrandDetails brand)
        {
            EcommerceWebsitesCommentDetails ProductReview = new EcommerceWebsitesCommentDetails();
            try
            {
                
                ProductReview.Title = review.review_title;
                ProductReview.ReviewID = productID;
                ProductReview.CommentMessage = review.review_text;
                ProductReview.Rating = Convert.ToString(review.rating_value);
                ProductReview.AuthorName = review.name;
                ProductReview.AuthorImage = review.profile_picture;
                ProductReview.PostURL = review.url;
                ProductReview.CommentTime = Convert.ToDateTime(review.date);
                if (ProductReview.CommentTime < DateTime.UtcNow.AddDays(-int.Parse(AppSettings.SinceDays_Reviews)))
                    return;
                ProductReview.Content = review.review_title + " " + review.review_text;
                ProductReview.CommentReviewID = Convert.ToString(review.id);

                Brand _BrandDetails = new Brand();
                _BrandDetails.BrandID = brand.BrandID;
                _BrandDetails.CategoryID = brand.CategoryID;
                _BrandDetails.CategoryName = brand.CategoryName;
                _BrandDetails.BrandName = brand.BrandName;
                TaggingClass Tag = new TaggingClass(AppSettings.ConnectionString);
                bool IsAction = true;

                var Data = Tag.SaveAndTagData(_BrandDetails, ProductReview,34, false, "", 0, ref IsAction);
                if (Data.TagID != 0)
                {
                    SendPushNotification(_BrandDetails, Data, "U", ProductReview);
                }

            }
            catch (Exception ex)
            {
                Dictionary<string, string> properties = new Dictionary<string, string>();
                properties.Add("CategoryID", brand.CategoryID.ToString());
                properties.Add("BrandID", brand.BrandID.ToString());
                properties.Add("ID", productID.ToString());
                properties.Add("Message", "error while saving reviews");
                Program.LogSpecificError(ex, properties, Program.ServiceName);
            }
        }
        public static void SendPushNotification(Brand setting, OutputData Data, string ActionableUnactionable, EcommerceWebsitesCommentDetails CommentData)
        {
            try
            {
                JsonSerializerSettings microsoftDateFormatSettings = new JsonSerializerSettings
                {
                    DateFormatHandling = DateFormatHandling.MicrosoftDateFormat
                };

                TaggingClass ch = new TaggingClass(AppSettings.ConnectionString);
                ChannelInfoDetails data = ch.AllDetailsTagID(setting.CategoryName, setting.BrandName, setting.BrandID, setting.CategoryID, Data.TagID.ToString(), Data.IsTicket);

                data.CategoryJson = Data.CategoryJson;
                data.BrandID = setting.BrandID;
                data.Language = CommentData.language;
                data.BrandName = setting.BrandName;
                data.CategoryName = setting.CategoryName;
                data.Channel = 34;
                data.ChannelName = "E-Commerce Websites";
                data.isMachineTagged = 1;
                data.IsRequestForManager = 0;
                data.LikeStatus = 0;
                data.LinsertedDate = DateTime.SpecifyKind(data.LinsertedDate, DateTimeKind.Utc);
                data.NewMention = setting.BrandID + "/33/" + data.ID;
                data.TagID = Data.TagID;
                data.TagPriority = 0;
                data.Title = CommentData.Title;
                data.RecordDate = DateTime.SpecifyKind(CommentData.CommentTime, DateTimeKind.Utc);
                data.Description = CommentData.Description;
                data.domain = new Uri(CommentData.URL).Host;
                data.PostURL = CommentData.URL;

                if (data.RecordDate > DateTime.UtcNow.AddDays(-2))
                {
                    string JsonData = "{\"Message\":\"New Data found for E-Commerce Websites \",\"BrandId\":" + setting.BrandID + ",\"Channel\":\"E-Commerce Websites\",\"DataCount\":1,\"IsNonActionable\":" + (ActionableUnactionable == "A" ? 0 : 1) + ",\"ActionableCount\":" + (ActionableUnactionable == "A" ? 1 : 0) + ",\"Data\":" + JsonConvert.SerializeObject(data, microsoftDateFormatSettings) +
                    (Data.UserID == 0 ? "" : ",\"UserID\": \"" + Data.UserID + "\"") + "}";
                    var client = new RestSharp.RestClient(AppSettings.ServerUrl);
                    var request = new RestSharp.RestRequest("WebHook/Broadcast", RestSharp.Method.POST);
                    request.AddParameter("groupID", setting.CategoryID.ToString());
                    request.AddParameter("message", JsonData);
                    request.AddParameter("UserRole", "Admin");
                    request.AddParameter("Mode", "1");
                    var output = client.Execute(request);
                }
            }
            catch (Exception ex)
            {

            }
        }
    }
}
