using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Mail;

namespace STPC.DynamicForms.Infraestructure
{
    public class Email
    {
        private string _strServer = string.Empty;
        private string _strUser = string.Empty;
        private string _strPassword = string.Empty;
        private string _strDomain = string.Empty;
        private bool _booCredential = false;
        private string _alias = string.Empty;
        private int _port = 0;
        public NetworkCredential _objCredencial = null;


        public Email(string strServer, int strPort, string strUser, string StrPassword, string strDomain, string alias)
        {
            this._strServer = strServer;
            this._port = strPort;
            this._strUser = strUser;
            this._strPassword = StrPassword;
            this._strDomain = strDomain;
            this._alias = alias;

        }
        public Email()
        {


        }
        public void SendMail(string strFROM, string strTO, string strCC, string strSUBJECT, string strBODY)
        {
            System.Net.Mail.MailMessage msg = new System.Net.Mail.MailMessage();
            msg.To.Add(strTO);
            msg.From = new MailAddress(strFROM, _alias, System.Text.Encoding.UTF8);
            msg.Subject = strSUBJECT;
            msg.SubjectEncoding = System.Text.Encoding.UTF8;
            msg.Body = strBODY;
            msg.BodyEncoding = System.Text.Encoding.UTF8;
            msg.IsBodyHtml = true;

            //Aquí es donde se hace lo especial
            SmtpClient client = new SmtpClient();
            client.Credentials = new System.Net.NetworkCredential(this._strUser, this._strPassword);
            client.Port = this._port;
            client.Host = _strServer;
            client.EnableSsl = true; //Esto es para que vaya a través de SSL que es obligatorio con GMail
            try
            {
                client.Send(msg);
            }
            catch (System.Net.Mail.SmtpException ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadLine();
            }

        }

        private void NewCredencial(SmtpClient mSmtpClient)
        {
            if (_strDomain != string.Empty)
            {
                mSmtpClient.Credentials = new System.Net.NetworkCredential(_strUser, _strPassword, _strDomain);
            }
            else
            {
                mSmtpClient.Credentials = new System.Net.NetworkCredential(_strUser, _strPassword);
            }


        }


    }
}
