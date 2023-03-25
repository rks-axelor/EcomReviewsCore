using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcomReviews.Classes
{
    public class Model
    {
    }
    public class BrandDetails
    {
        public long BrandID { get; set; }
        public long CategoryID { get; set; }
        public string BrandName { get; set; }
        public string CategoryName { get; set; }
    }
    public class ProductDetails
    {
        public string ID { get; set; }
        public long ProductID { get; set; }
        public string Title { get; set; }
        public string URL { get; set; }
        public int JobID { get; set; }
    }
    public class Reviews
    {
        public long ReviewSiteID { get; set; }
        public string CommentMessage { get; set; }
        public DateTime CommentTime { get; set; }
        public string AuthorImage { get; set; }
        public string AuthorURL { get; set; }
        public string AuthorName { get; set; }
        public long ECReviewId { get; set; }
        public string Title { get; set; }
        public string BrandName { get; set; }
        public string CategoryName { get; set; }
        public string Rating { get; set; }
        public string Url { get; set; }
        public string CommentReviewID { get; set; }
    }

    public class Response
    {
        public int id { get; set; }
        public string name { get; set; }
        public string date { get; set; }
        public string comment { get; set; }
    }

    public class Review
    {
        public long id { get; set; }
        public string name { get; set; }
        public string date { get; set; }
        public decimal rating_value { get; set; }
        public string review_text { get; set; }
        public string url { get; set; }
        public string profile_picture { get; set; }
        public object location { get; set; }
        public string review_title { get; set; }
        public bool verified_order { get; set; }
        public object language_code { get; set; }
        public string reviewer_title { get; set; }
        public object unique_id { get; set; }
        public object meta_data { get; set; }
        public Response response { get; set; }
    }

    public class Root
    {
        public bool success { get; set; }
        public int status { get; set; }
        public int job_id { get; set; }
        public string source_url { get; set; }
        public string source_name { get; set; }
        public object place_id { get; set; }
        public object external_identifier { get; set; }
        public object meta_data { get; set; }
        public object unique_id { get; set; }
        public int review_count { get; set; }
        public double average_rating { get; set; }
        public string last_crawl { get; set; }
        public string crawl_status { get; set; }
        public decimal percentage_complete { get; set; }
        public int result_count { get; set; }
        public int credits_used { get; set; }
        public object from_date { get; set; }
        public object blocks { get; set; }
        public List<Review> reviews { get; set; }
    }
    public class Job
    {
        public bool success { get; set; }
        public int job_id { get; set; }
        public int status { get; set; }
        public string message { get; set; }
    }
    public enum Channeltype
    {
        E_Commerce_Posts= 33,
        E_Commerce_Reviews = 34
    }
}
