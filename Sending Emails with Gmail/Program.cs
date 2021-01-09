using System;
using System.Net.Mail;
using System.Net;
using System.Text.RegularExpressions;

public class Program
{
    public static void Main()
    {
        const string toEmail = "toAddr@gmail.com"; // <--------------------------------------------------------------------------------------------put a valid email here
        const string fromEmail = "mockupAddr@mockupSrv.com"; ///doesn't matter because Google only allows the owner of the account to sent the email
		const string subject = "Hello from .NET";
        const string msg = "Main message body";

        try
        {
            Console.WriteLine(EmailTools.SendMailWithouAttachment(toEmail, fromEmail, subject, msg));
        }
        catch (Exception ex) { Console.WriteLine("Change the toEmail and put a valid Google Account and Password inside the SendMailWithoutAttachment Method\n"); }

        Console.ReadLine();
    }
}

public static class EmailTools
{
    public static bool ValidateEmailAddr(string mailAddr)
    {
        try
        {
            string textToVaLidate = mailAddr;
            Regex regexExpression = new Regex(@"\w+@[a-zA-Z_]+?\.[a-zA-Z]{2,3}");
            if (regexExpression.IsMatch(textToVaLidate)) return true;
            return false;
        }
        catch (Exception) { throw; }
    }

    public static string SendMailWithouAttachment(string destination, string From, string subject, string msg)
    {
        try
        {
            if (ValidateEmailAddr(destination) == false) return "Invalid Destination: " + destination;
            MailMessage mailMsg = new MailMessage(From, destination, subject, msg);
            SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
            client.EnableSsl = true;

            //NetworkCredential receives a login and password. You have to allow 3rd party access on the GOOGLE ACCOUNT for this code to work.
            //It won't work if account has 2FA enabled.. so disable it. 
            NetworkCredential cred = new NetworkCredential("anyAccountWithout2FAandAllowed3rdPartyConnections@gmail.com", "yourPassword"); // <------------PUT A VALID GOOGLE ACCOUNT AND PASSWORD !!!!!!!!!!
            client.Credentials = cred;
            client.Send(mailMsg);
            return "Message Sent to  " + destination + " at " + DateTime.Now.ToString() + ".";
        }
        catch (Exception ex)
        {
            string error = "\n"+ex.InnerException?.ToString();
            return ex.Message.ToString() + error;
        }
    }

}

