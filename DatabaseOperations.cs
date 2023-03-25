using EcomReviews.Classes;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcomReviews
{
    public class DatabaseOperations
    {
        public static List<BrandDetails> GetEcomSettings()
        {
            List<BrandDetails> brandDetailsList = new List<BrandDetails>();
            var db = new SqlDatabase(AppSettings.ConnectionString);
            IDataReader reader = null;
            try
            {
                reader = db.ExecuteReader("LB3_GetEcommerceSettingDetails_V51");
                while (reader.Read())
                {
                    BrandDetails brandDetails = new BrandDetails();
                    brandDetails.CategoryName = Convert.ToString(reader["CategoryName"]);
                    brandDetails.BrandName = Convert.ToString(reader["BrandName"]);
                    brandDetails.BrandID = long.Parse(Convert.ToString(reader["BrandID"]));
                    brandDetails.CategoryID = long.Parse(Convert.ToString(reader["CategoryID"]));
                    brandDetailsList.Add(brandDetails);
                }
            }
            catch (Exception ex)
            {
              Program.LogGeneralError(ex,"error while getting brand details setting",Program.ServiceName);
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
            return brandDetailsList;
        }

        internal static void SaveJobId(long iD, int jobID,BrandDetails brand)
        {
            Database db = new SqlDatabase(AppSettings.ConnectionString);
            IDataReader reader = null;
            try
            {
                reader = db.ExecuteReader("LB3_Update_ECJobID_Machine_V51", brand.CategoryName, brand.BrandID, iD,jobID);

            }
            catch(Exception ex)
            {
                Dictionary<string, string> properties = new Dictionary<string, string>();
                properties.Add("CategoryID", brand.CategoryID.ToString());
                properties.Add("BrandID", brand.BrandID.ToString());
                properties.Add("ID", iD.ToString());
                properties.Add("JobID", jobID.ToString());
                properties.Add("Message", "error while saving jobid");
                Program.LogSpecificError(ex, properties, Program.ServiceName);
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }

        public static List<ProductDetails> GetProductList(BrandDetails brand)
        {
            List<ProductDetails> productList = new List<ProductDetails>();
            var db = new SqlDatabase(AppSettings.ConnectionString);
            IDataReader reader = null;
            try
            {
                if(Convert.ToInt32(AppSettings.ServiceType) == 1)
                {
                    reader = db.ExecuteReader("LB3_Get_ECommercePostDetails_V51", brand.CategoryName, brand.BrandID, brand.BrandName, Channeltype.E_Commerce_Posts, int.Parse(AppSettings.SinceDays_Post));
                }
                else if(Convert.ToInt32(AppSettings.ServiceType) == 2)
                {
                    reader = db.ExecuteReader("LB3_Get_ECommercePostDetails_JobID_V51", brand.CategoryName, brand.BrandID, brand.BrandName, Channeltype.E_Commerce_Posts, int.Parse(AppSettings.SinceDays_Post));
                }                
                while (reader.Read())
                {
                    ProductDetails product = new ProductDetails();
                    product.ID = Convert.ToString(reader["ProductID"]);
                    product.ProductID = Convert.ToInt64(reader["EcReviewID"]);
                    product.Title = Convert.ToString(reader["Title"]);
                    product.URL = Convert.ToString(reader["URL"]);
                    if(reader["JobID"] != DBNull.Value)
                        product.JobID = Convert.ToInt32(reader["JobID"]);
                    productList.Add(product);

                }
            }
            catch (Exception ex)
            {
                Dictionary<string, string> properties = new Dictionary<string, string>();
                properties.Add("CategoryID", brand.CategoryID.ToString());
                properties.Add("BrandID", brand.BrandID.ToString());
                properties.Add("Message", "error while getting product list");
                Program.LogSpecificError(ex, properties, Program.ServiceName);
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
            return productList;
        }
    }
}
