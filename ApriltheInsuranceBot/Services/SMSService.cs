using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ApriltheInsuranceBot.Services
{
    public class SMSService
    {
        public static async Task SendMessageAsync(string phone, string msg)
        {
            SMSGlobal.MobileWorksPortTypeClient client = new SMSGlobal.MobileWorksPortTypeClient();
            var ticket = await client.apiValidateLoginAsync("ykp2mzmx", "dKhfgQae");
            XElement token = XElement.Parse(ticket);

            //await client.apiSendSmsAsync(token.Value, "api2way", phone, msg, "text", "0", "");
        }
    }
}
