using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace CRWestropp.Utilities {
	public static class Emailer {
		public class EmailOptions {
			public string SmtpServer { get; set; }
			public string Username { get; set; }
			public string Password { get; set; }
			public int Port { get; set; }
		}

		public static void SendEmail(EmailOptions options, string to, string from, string subject, string message) {
			var client = new SmtpClient {
				Host = options.SmtpServer,
				Port = options.Port,
				EnableSsl = true,
				DeliveryMethod = SmtpDeliveryMethod.Network,
				UseDefaultCredentials = false,
				Credentials = new NetworkCredential(options.Username, options.Password)
			};

			var mailMessage = new MailMessage() {
				From = new MailAddress(from, "CHABS No Reply"),
				To = { to },
				Body = message,
				Subject = subject
			};
			try {
				client.Send(mailMessage);
			} catch (Exception ex) {
				throw new Exception("Could not send email", ex);
			}
		}
	}
}
