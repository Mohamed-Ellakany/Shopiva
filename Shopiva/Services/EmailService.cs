using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Shopiva.Abstractions.Options;
using System.Net.Mail;

namespace Shopiva.Services
{
    public class EmailService(IOptions<EmailOptions> emailOptions) : IEmailService
    {
        private readonly EmailOptions _emailOptions = emailOptions.Value;

        public async Task<Result<bool>> SendAsync(
            string toEmail,
            string subject,
            string htmlBody,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_emailOptions.SenderName, _emailOptions.SenderEmail));
                message.To.Add(MailboxAddress.Parse(toEmail));
                message.Subject = subject;
                message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

                using var client = new MailKit.Net.Smtp.SmtpClient();
                await client.ConnectAsync(
                    _emailOptions.SmtpHost,
                    _emailOptions.SmtpPort,
                    _emailOptions.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None,
                    cancellationToken);

                await client.AuthenticateAsync(_emailOptions.SenderEmail, _emailOptions.Password, cancellationToken);
                await client.SendAsync(message, cancellationToken);
                await client.DisconnectAsync(true, cancellationToken);

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return Result.Failure<bool>(new Error("Email.SendFailed", $"Failed to send email: {ex.Message}"));
            }
        }

        public Task<Result<bool>> SendPasswordResetOtpAsync(
            string toEmail, string userName, string otp,
            CancellationToken cancellationToken = default)
            => SendAsync(toEmail, "Your Shopiva Password Reset Code", BuildPasswordResetOtpEmail(userName, otp), cancellationToken);

        public Task<Result<bool>> SendEmailConfirmationOtpAsync(
            string toEmail, string userName, string otp,
            CancellationToken cancellationToken = default)
            => SendAsync(toEmail, "Your Shopiva Email Confirmation Code", BuildEmailConfirmationOtpEmail(userName, otp), cancellationToken);

        // ─────────────────────────────────────────────────────────────
        // HTML Templates
        // ─────────────────────────────────────────────────────────────

        private static string OtpBox(string otp)
        {
            // Render each digit in its own box for a modern OTP appearance
            var digits = string.Concat(otp.Select(d =>
                $"<span style='display:inline-block;width:48px;height:60px;line-height:60px;margin:0 4px;" +
                $"font-size:28px;font-weight:700;text-align:center;border-radius:8px;" +
                $"background:#F3F4F6;color:#1F2937;border:1px solid #D1D5DB;'>{d}</span>"));
            return $"<div style='text-align:center;margin:28px 0;'>{digits}</div>";
        }

        private static string BuildPasswordResetOtpEmail(string userName, string otp) => $"""
            <!DOCTYPE html>
            <html lang="en">
            <head>
              <meta charset="UTF-8"/>
              <meta name="viewport" content="width=device-width,initial-scale=1.0"/>
              <title>Reset Your Password</title>
            </head>
            <body style="font-family:'Segoe UI',Arial,sans-serif;background:#F4F4F7;margin:0;padding:0;">
              <div style="max-width:520px;margin:40px auto;background:#fff;border-radius:10px;overflow:hidden;box-shadow:0 2px 10px rgba(0,0,0,.1);">
                
                <!-- Header -->
                <div style="background:#4F46E5;padding:28px 40px;text-align:center;">
                  <h1 style="color:#fff;margin:0;font-size:22px;letter-spacing:1px;">🛍️ Shopiva</h1>
                </div>

                <!-- Body -->
                <div style="padding:36px 40px;color:#374151;">
                  <h2 style="margin:0 0 12px;font-size:20px;">Password Reset</h2>
                  <p style="margin:0 0 8px;line-height:1.7;">Hi <strong>{userName}</strong>,</p>
                  <p style="margin:0 0 24px;line-height:1.7;color:#6B7280;">
                    We received a request to reset your password. Enter the code below in the app.
                    It expires in <strong>10 minutes</strong>.
                  </p>

                  {OtpBox(otp)}

                  <hr style="border:none;border-top:1px solid #E5E7EB;margin:28px 0;"/>
                  <p style="font-size:13px;color:#9CA3AF;line-height:1.6;">
                    If you didn't request a password reset, you can safely ignore this email.
                    Your password will not be changed.
                  </p>
                </div>

                <!-- Footer -->
                <div style="background:#F9FAFB;padding:18px 40px;text-align:center;font-size:12px;color:#9CA3AF;">
                  &copy; {DateTime.UtcNow.Year} Shopiva. All rights reserved.
                </div>
              </div>
            </body>
            </html>
            """;

        private static string BuildEmailConfirmationOtpEmail(string userName, string otp) => $"""
            <!DOCTYPE html>
            <html lang="en">
            <head>
              <meta charset="UTF-8"/>
              <meta name="viewport" content="width=device-width,initial-scale=1.0"/>
              <title>Confirm Your Email</title>
            </head>
            <body style="font-family:'Segoe UI',Arial,sans-serif;background:#F4F4F7;margin:0;padding:0;">
              <div style="max-width:520px;margin:40px auto;background:#fff;border-radius:10px;overflow:hidden;box-shadow:0 2px 10px rgba(0,0,0,.1);">

                <!-- Header -->
                <div style="background:#10B981;padding:28px 40px;text-align:center;">
                  <h1 style="color:#fff;margin:0;font-size:22px;letter-spacing:1px;">🛍️ Shopiva</h1>
                </div>

                <!-- Body -->
                <div style="padding:36px 40px;color:#374151;">
                  <h2 style="margin:0 0 12px;font-size:20px;">Confirm Your Email</h2>
                  <p style="margin:0 0 8px;line-height:1.7;">Hi <strong>{userName}</strong>,</p>
                  <p style="margin:0 0 24px;line-height:1.7;color:#6B7280;">
                    Thanks for signing up! Enter the code below to verify your email address.
                    It expires in <strong>10 minutes</strong>.
                  </p>

                  {OtpBox(otp)}

                  <hr style="border:none;border-top:1px solid #E5E7EB;margin:28px 0;"/>
                  <p style="font-size:13px;color:#9CA3AF;line-height:1.6;">
                    If you didn't create a Shopiva account, you can safely ignore this email.
                  </p>
                </div>

                <!-- Footer -->
                <div style="background:#F9FAFB;padding:18px 40px;text-align:center;font-size:12px;color:#9CA3AF;">
                  &copy; {DateTime.UtcNow.Year} Shopiva. All rights reserved.
                </div>
              </div>
            </body>
            </html>
            """;
    }
}