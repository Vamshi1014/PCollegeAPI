
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Flyurdreamcommands.Models.Datafields;

namespace Flyurdreamcommands.Repositories.Abstract
{
  public interface IEmailService
    {
        void EmailNotification(User user, Email mailArgs, string[] attachmentPaths = null);
        void InsertEmailHistory(int UserId, Email mailArgs);
       void SendOtpEmial(UserPasscode userPasscode, Email mailArgs, string[] attachmentPaths = null);
    }
}
