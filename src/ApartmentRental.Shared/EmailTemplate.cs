namespace ApartmentRental.Shared;

// One consistent, branded, table-based HTML shell for every email the app
// sends (magic link, verification, payment reminders). Deliberately avoids
// flexbox/div-based layout - Gmail (web, iOS, and Android) strips or
// mis-renders those, so everything here is nested <table>/<td> the way
// email clients actually expect.
public static class EmailTemplate
{
    public static string Build(string heading, string bodyHtml, string? buttonText = null, string? buttonUrl = null)
    {
        var buttonRow = buttonText is not null && buttonUrl is not null
            ? $@"
        <tr>
          <td style=""padding:8px 32px 4px;"">
            <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"">
              <tr>
                <td align=""left"">
                  <table role=""presentation"" cellpadding=""0"" cellspacing=""0"">
                    <tr>
                      <td bgcolor=""#5B54F0"" style=""border-radius:10px;"">
                        <a href=""{buttonUrl}"" target=""_blank""
                           style=""display:block;padding:13px 28px;color:#ffffff;text-decoration:none;
                                  font-weight:600;font-size:14px;font-family:Arial,Helvetica,sans-serif;"">
                          {buttonText}
                        </a>
                      </td>
                    </tr>
                  </table>
                </td>
              </tr>
            </table>
          </td>
        </tr>"
            : string.Empty;

        return $@"<!DOCTYPE html>
<html lang=""en"">
<head>
<meta charset=""UTF-8"">
<meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
<title>{heading}</title>
</head>
<body style=""margin:0;padding:0;background-color:#F7F7FB;font-family:Arial,Helvetica,sans-serif;"">
  <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background-color:#F7F7FB;"">
    <tr>
      <td align=""center"" style=""padding:40px 16px;"">
        <table role=""presentation"" width=""480"" cellpadding=""0"" cellspacing=""0""
               style=""max-width:480px;width:100%;background-color:#ffffff;border-radius:16px;border:1px solid #E7E7F1;"">
          <tr>
            <td style=""padding:32px 32px 0;"">
              <table role=""presentation"" cellpadding=""0"" cellspacing=""0"">
                <tr>
                  <td width=""28"" height=""28"" bgcolor=""#5B54F0"" align=""center"" valign=""middle""
                      style=""border-radius:8px;font-weight:700;font-size:14px;color:#ffffff;font-family:Arial,Helvetica,sans-serif;"">
                    A
                  </td>
                  <td style=""padding-left:8px;font-weight:700;font-size:15px;color:#14151F;font-family:Arial,Helvetica,sans-serif;"">
                    Apartly
                  </td>
                </tr>
              </table>
            </td>
          </tr>
          <tr>
            <td style=""padding:24px 32px 8px;"">
              <h1 style=""margin:0;font-size:19px;font-weight:700;color:#14151F;font-family:Arial,Helvetica,sans-serif;"">{heading}</h1>
            </td>
          </tr>
          <tr>
            <td style=""padding:0 32px;color:#4A4B5C;font-size:14px;line-height:22px;font-family:Arial,Helvetica,sans-serif;"">
              {bodyHtml}
            </td>
          </tr>{buttonRow}
          <tr>
            <td style=""padding:28px 32px 32px;"">
              <p style=""margin:0;font-size:12px;color:#9C9DAE;font-family:Arial,Helvetica,sans-serif;"">
                If you didn't request this, you can safely ignore this email.
              </p>
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </table>
</body>
</html>";
    }
}
