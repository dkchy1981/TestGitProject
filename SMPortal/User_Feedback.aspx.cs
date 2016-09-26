using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Globalization;
using System.Net.Mail;
using System.Threading.Tasks;

namespace SMPortal
{
    public partial class User_Feedback : System.Web.UI.Page
    {
        public enum Emailfor
        {
            Success,
            Failure
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                DateTime date = (DateTime)SqlDateTime.MinValue;
                string format = "dd/MM/yyyy HH:mm:ss";
                string dateString = string.Empty;
                CultureInfo provider = CultureInfo.InvariantCulture;


                if (Request.QueryString != null &&
                    !string.IsNullOrWhiteSpace(Request.QueryString["Date"]) &&
                    !string.IsNullOrWhiteSpace(Request.QueryString["name"]) &&
                    !string.IsNullOrWhiteSpace(Request.QueryString["ir"]) &&
                    !string.IsNullOrWhiteSpace(Request.QueryString["feed"]))
                {

                    dateString = Request.QueryString["Date"].ToString();
                    try
                    {
                        date = DateTime.ParseExact(dateString, format, provider);
                    }
                    catch
                    {
                        if (!DateTime.TryParse(Request.QueryString["Date"].ToString(), out date))
                        {
                            Response.Write("<script> alert('" + ConfigurationManager.AppSettings["DateFormatException"] + "');</script>");
                        }
                    }


                    if (date != (DateTime)SqlDateTime.MinValue && date != DateTime.MinValue)
                    {

                        string incidenceRecordNum = Request.QueryString["ir"];
                        string name = Request.QueryString["name"];
                        string feed = Request.QueryString["feed"];

                        string strCommand = "INSERT INTO User_Feedback_Data VALUES('" + date.Date.ToString("dd/MM/yyyy") + "', '" + incidenceRecordNum + "', '" + name + "', '" + feed + "')";
                        SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SMPortalDB"].ConnectionString);
                        try
                        {
                            con.Open();
                            SqlCommand command = new SqlCommand(strCommand, con);
                            if (command.ExecuteNonQuery() > 0)
                            {
                                SendEmail(Emailfor.Success, incidenceRecordNum, name, feed, date);
                            }
                            else
                            {
                                SendEmail(Emailfor.Failure, incidenceRecordNum, name, feed, date);
                            }
                        }
                        catch (Exception ex)
                        {
                            string message = ex.Message;
                        }
                        finally
                        {
                            con.Close();
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Send email and return success/failure
        /// </summary>
        /// <param name="failure"></param>
        /// <returns></returns>
        private bool SendEmail(Emailfor type, string incidenceRecordNum, string name, string feed, DateTime date)
        {
            try
            {
                string emailBody = string.Empty;
                string emailSubject = string.Empty;
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient(ConfigurationManager.AppSettings["SMTPClient"]);

                mail.From = new MailAddress(ConfigurationManager.AppSettings["FromEmail"]);
                mail.To.Add(ConfigurationManager.AppSettings["ToEmail"]);
                switch (type)
                {
                    case Emailfor.Success:
                        {
                            emailBody = System.IO.File.ReadAllText(@Server.MapPath("~/HTML/Success.html"));
                            emailSubject = ConfigurationManager.AppSettings["SuccessEmailSub"];

                        }
                        break;
                    case Emailfor.Failure:
                        {
                            emailBody = System.IO.File.ReadAllText(@Server.MapPath("~/HTML/Failure.html"));
                            emailSubject = ConfigurationManager.AppSettings["FailureEmailSub"];
                        }
                        break;
                    default:
                        break;
                }
                emailBody = emailBody.Replace("[[NAME]]", name);
                emailBody = emailBody.Replace("[[IR]]", incidenceRecordNum);
                emailBody = emailBody.Replace("[[FEED]]", feed);
                emailBody = emailBody.Replace("[[DATE]]", date.ToString("dd/MM/yyyy"));

                mail.Subject = emailSubject;
                mail.Body = emailBody;
                mail.IsBodyHtml = true;

                SmtpServer.Port = 25;
                //SmtpServer.Credentials = new System.Net.NetworkCredential("username", "password");
                //SmtpServer.EnableSsl = true;

                SmtpServer.Send(mail);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}