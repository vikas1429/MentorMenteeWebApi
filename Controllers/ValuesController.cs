using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data;
using System.Data.SqlClient;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MentorMenteeWebApi.Controllers
{
    

    [RoutePrefix("[controller]/[action]")]
    public class ValuesController : ApiController
    {
       // static string sqlstring1 ="Data Source = (LocalDB)\\MSSQLLocalDB;AttachDbFilename=|DataDirectory|MentorMentee.mdf;Integrated Security = True";
        //"Data Source=mentormentee.database.windows.net;Initial Catalog=Mentormentee;Integrated Security=False;User ID=rahulp;Password=Rahul@123;Connect Timeout=60;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
        static string sqlstring = System.Configuration.ConfigurationManager.ConnectionStrings["LocalDB"].ConnectionString;      
        struct Mentor
        {
           public string name;
           public string category;
           public string schedule;
        };

        [ActionName("Options")]
        [HttpOptions]
        public HttpResponseMessage Options()
        {

            return new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
        }

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

        private string GetData(string code)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://www.linkedin.com/");

            // Add an Accept header for JSON format.
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
            JObject obj = new JObject();
            var stringContent = new StringContent(obj.ToString());
            string uri =string.Format("oauth/v2/accessToken?grant_type=authorization_code&redirect_uri=http://localhost:3000/linkedconnect&client_id=860l38mgbq37qs&client_secret=uRJWOyIsAzXQqS1n&code={0}",code);
            HttpResponseMessage response =  client.PostAsync(uri, stringContent).Result;

            if (response.IsSuccessStatusCode)
            {
                int i = 0;
                var res = response.Content.ReadAsAsync<JObject>().Result;
                var foo = res["access_token"].ToString();
                return foo;
            }
            else
            {
                return "error";
            }
        }

        private JObject GetUserData(string accessToken)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://api.linkedin.com/");

            // Add an Accept header for JSON format.
         //   client.DefaultRequestHeaders.Accept.Add(
         //       new MediaTypeWithQualityHeaderValue("application/json"));
            
            JObject obj = new JObject();
            var stringContent = new StringContent(obj.ToString());
            string uri = string.Format("/v1/people/~?format=json");
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);
           // client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", accessToken);
            HttpResponseMessage response = client.GetAsync(uri).Result;

            if (response.IsSuccessStatusCode)
            {
               
                var res = response.Content.ReadAsAsync<JObject>().Result;
                return res;
               
              //  return foo;
            }
            else
            {
                var res = response.Content.ReadAsAsync<JObject>().Result;
                return res;
                //var foo = res["access_token"].ToString();
                //return "error";
            }
        }

        [HttpGet]
         [ActionName("CheckUser")]
        public HttpResponseMessage CheckUser( string code)
        {


           string access_token = GetData( code);
            JObject res = GetUserData(access_token);

            var user = res["firstName"].ToString();
            var linkedinid = res["id"].ToString();

            //Create entry for the user and its linkedin id in the database
            PostUser(user, linkedinid);

            //return the username to front end to display on the dashboard.
            HttpRequestMessage resp = new HttpRequestMessage();
            resp.SetConfiguration(new HttpConfiguration());

            //  SqlConnection sqlcon = new SqlConnection(sqlstring);
            ////  return resp.CreateResponse(sqlstring);
            //  sqlcon.Open();
            // // return resp.CreateResponse(true);
            //  bool val = false;
            //string str = string.Format("SELECT username from tblUser where" +
            //    " username =@username and password =@pwd") ;


            //  using (SqlCommand cmd = new SqlCommand(str, sqlcon))
            //  {

            //      cmd.CommandType = CommandType.Text;
            //      cmd.Parameters.Add("@username", SqlDbType.VarChar);
            //      cmd.Parameters["@username"].Value = "vikas";
            //      cmd.Parameters.Add("@pwd", SqlDbType.VarChar);
            //      cmd.Parameters["@pwd"].Value = "vikas";

            //      using (SqlDataReader reader = cmd.ExecuteReader())
            //      {
            //          if (reader.Read())
            //          {

            //              return resp.CreateResponse(true);
            //          }

            //          else
            //             return resp.CreateResponse(false);
            //      }
            return resp.CreateResponse(user);

        }
           
           
        

        // GET api/values
        [ActionName("GetMentors")]
        public HttpResponseMessage GetMentors( string Category)
        {
            HttpRequestMessage resp = new HttpRequestMessage();
            resp.SetConfiguration(new HttpConfiguration());

            SqlConnection sqlcon = new SqlConnection(sqlstring);
            sqlcon.Open();
            List<Mentor> val = new List<Mentor>();
          string str = string.Format("SELECT U.username,C.vchCategory,M.dtSchedule " +
                                                    " from  TBlUser U" +
                                                    " LEFT OUTER JOIN TBLMentor M" +
                                                    " ON U.id = M.MentorId" +
                                                    " JOIN TBlCategory C" +
                                                    " ON M.CategoryID = C.id" +
                                                    " WHERE C.vchCategory = @Category") ;


            using (SqlCommand cmd = new SqlCommand(str, sqlcon))
            {
                SqlParameter param = new SqlParameter();
                param.ParameterName = "@Category";
                param.Value = Category;

                // 3. add new parameter to command object
                cmd.Parameters.Add(param);
               // cmd.CommandType = CommandType.Text;
               // cmd.Parameters.Add("@Category", SqlDbType.VarChar);
              //  cmd.Parameters["@Category"].Value = Category;
               
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Mentor m = new Mentor();
                        m.name = reader.GetValue(0).ToString();
                        m.category = reader.GetValue(1).ToString();
                        m.schedule = reader.GetValue(2).ToString();
                         val.Add(m);
                    }
                    return resp.CreateResponse(val);
                }
            }
           
            // return new string[] { "value1", "value2" };
        }

        [ActionName("GetAssociatedMentors")]
        public HttpResponseMessage GetAssociatedMentors(string mentee)
        {
            HttpRequestMessage resp = new HttpRequestMessage();
            resp.SetConfiguration(new HttpConfiguration());

            SqlConnection sqlcon = new SqlConnection(sqlstring);
            sqlcon.Open();
            List<Mentor> val = new List<Mentor>();
            string str = string.Format("procGetAssociatedMentors");


            using (SqlCommand cmd = new SqlCommand(str, sqlcon))
            {
                 cmd.CommandType = CommandType.StoredProcedure;
                 cmd.Parameters.Add("@mentee", SqlDbType.VarChar);
                 cmd.Parameters["@mentee"].Value = mentee;

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Mentor m = new Mentor();
                        m.name = reader.GetValue(0).ToString();
                        m.category = reader.GetValue(1).ToString();
                        m.schedule = reader.GetValue(2).ToString();
                        val.Add(m);
                    }
                    return resp.CreateResponse(val);
                }
            }

            // return new string[] { "value1", "value2" };
        }
        [ActionName("GetAssociatedMentees")]
        public HttpResponseMessage GetAssociatedMentees(string mentor)
        {
            HttpRequestMessage resp = new HttpRequestMessage();
            resp.SetConfiguration(new HttpConfiguration());

            SqlConnection sqlcon = new SqlConnection(sqlstring);
            sqlcon.Open();
            List<Mentor> val = new List<Mentor>();
            string str = string.Format("procGetAssociatedMentees");


            using (SqlCommand cmd = new SqlCommand(str, sqlcon))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@mentor", SqlDbType.VarChar);
                cmd.Parameters["@mentor"].Value = mentor;

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Mentor m = new Mentor();
                        m.name = reader.GetValue(0).ToString();
                        m.category = reader.GetValue(1).ToString();
                        m.schedule = reader.GetValue(2).ToString();
                        val.Add(m);
                    }
                    return resp.CreateResponse(val);
                }
            }

            // return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet]
        [ActionName("Getid")]
        public string Getid(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpGet]
        [ActionName("AddMentorToMentee")]
        public HttpResponseMessage AddMentorToMentee(string Category, string Schedule, string Mentorname,string Menteename)
        {
            HttpRequestMessage resp = new HttpRequestMessage();
            resp.SetConfiguration(new HttpConfiguration());
            SqlConnection sqlcon = new SqlConnection(sqlstring);
            sqlcon.Open();
            List<Mentor> val = new List<Mentor>();
            string str = "procAddNewMentortoMenee";
            using (SqlCommand cmd = new SqlCommand(str, sqlcon))
            {                              
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Category", SqlDbType.VarChar);
                cmd.Parameters.Add("@schedule", SqlDbType.DateTime);
                cmd.Parameters.Add("@mentorname", SqlDbType.VarChar);
                cmd.Parameters.Add("@menteename", SqlDbType.VarChar);
                cmd.Parameters["@Category"].Value = Category;
                cmd.Parameters["@schedule"].Value = Schedule;
                cmd.Parameters["@mentorname"].Value = Mentorname;
                cmd.Parameters["@menteename"].Value = Menteename;
                cmd.ExecuteNonQuery();             
              }
            return resp.CreateResponse(HttpStatusCode.OK, "done");
        }
        [HttpGet]
        [ActionName("PostMentors")]
        public HttpResponseMessage PostMentors(string Category, string Schedule, string Mentorname)
        {
            HttpRequestMessage resp = new HttpRequestMessage();
            resp.SetConfiguration(new HttpConfiguration());
            SqlConnection sqlcon = new SqlConnection(sqlstring);
            sqlcon.Open();
            List<Mentor> val = new List<Mentor>();
            string str = "procAddNewMentor";
            using (SqlCommand cmd = new SqlCommand(str, sqlcon))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Category", SqlDbType.VarChar);
                cmd.Parameters.Add("@schedule", SqlDbType.VarChar);
                cmd.Parameters.Add("@mentorname", SqlDbType.VarChar);
                cmd.Parameters["@Category"].Value = Category;
                cmd.Parameters["@schedule"].Value = Schedule;
                cmd.Parameters["@mentorname"].Value = Mentorname;
                cmd.ExecuteNonQuery();
            }
            return resp.CreateResponse(HttpStatusCode.OK, "done");
        }

       // [HttpGet]
       // [ActionName("PostUser")]
        private void PostUser(string username, string password)
        {
          //  HttpRequestMessage resp = new HttpRequestMessage();
          //  resp.SetConfiguration(new HttpConfiguration());
            SqlConnection sqlcon = new SqlConnection(sqlstring);
            sqlcon.Open();
            
            string str = "procAddNewUser";
            using (SqlCommand cmd = new SqlCommand(str, sqlcon))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Username", SqlDbType.VarChar);
                cmd.Parameters.Add("@Password", SqlDbType.VarChar);
                cmd.Parameters["@Username"].Value = username;
                cmd.Parameters["@Password"].Value = password;          
                cmd.ExecuteNonQuery();
            }
            //return resp.CreateResponse(HttpStatusCode.OK, "done");
        }
        // GET api/values
        [ActionName("GetUserSchedule")]
        public HttpResponseMessage GetUserSchedule(string username)
        {
            HttpRequestMessage resp = new HttpRequestMessage();
            resp.SetConfiguration(new HttpConfiguration());

            SqlConnection sqlcon = new SqlConnection(sqlstring);
            sqlcon.Open();
            List<Mentor> val = new List<Mentor>();
            string str = string.Format("spGetUserSchedule");


            using (SqlCommand cmd = new SqlCommand(str, sqlcon))
            {
              
                 cmd.CommandType = CommandType.StoredProcedure;
                 cmd.Parameters.Add("@username", SqlDbType.VarChar);
                  cmd.Parameters["@username"].Value = username;

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Mentor m = new Mentor();
                        m.name = username;
                        m.category = reader.GetValue(0).ToString();
                        m.schedule = reader.GetValue(1).ToString();
                        val.Add(m);
                    }
                    return resp.CreateResponse(val);
                }
            }

            // return new string[] { "value1", "value2" };
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
