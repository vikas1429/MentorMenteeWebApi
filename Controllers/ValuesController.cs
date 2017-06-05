using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data;
using System.Data.SqlClient;

namespace MentorMenteeWebApi.Controllers
{
    [RoutePrefix("[controller]/[action]")]
    public class ValuesController : ApiController
    {
        static string sqlstring ="Data Source = (LocalDB)\\MSSQLLocalDB;AttachDbFilename=|DataDirectory|MentorMentee.mdf;Integrated Security = True";
        
        
        // GET api/values
        [ActionName("GetCategory")]
        public IEnumerable<string> GetCategory()
        {
            SqlConnection sqlcon = new SqlConnection(sqlstring);
            sqlcon.Open();
            List<string> val = new List<string>() ;
            using (SqlCommand cmd = new SqlCommand("SELECT vchCategory from  TBlCategory", sqlcon))
            {
                cmd.CommandType = CommandType.Text;
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while(reader.Read())
                    {
                        val.Add(reader.GetValue(0).ToString());                     
                    }
                }
            }
            return val;
           // return new string[] { "value1", "value2" };
        }
        // GET api/values
        [ActionName("GetMentors")]
        public IEnumerable<string> GetMentors( string Category)
        {
            SqlConnection sqlcon = new SqlConnection(sqlstring);
            sqlcon.Open();
            List<string> val = new List<string>();
            string str = string.Format("SELECT username " +
                                                    " from  TBlUser U" +
                                                    " JOIN TBLMentor M" +
                                                    " ON U.id = M.MentorId" +
                                                    " JOIN TBlCategory C" +
                                                    " ON M.CategoryID = C.id" +
                                                    " WHERE C.vchCategory = @Category" );


            using (SqlCommand cmd = new SqlCommand(str, sqlcon))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@Category", SqlDbType.VarChar);
                cmd.Parameters["@Category"].Value = Category;
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        val.Add(reader.GetValue(0).ToString());
                    }
                }
            }
            return val;
            // return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
}
