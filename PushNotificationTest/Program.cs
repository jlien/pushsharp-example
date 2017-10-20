using System;
using Newtonsoft.Json.Linq;
using PushSharp.Apple;

namespace PushNotificationTest
{
    public class Program
    {
        private const string DeviceToken = "<INSERT YOUR DEVICE TOKEN>";

        public static void Main(string[] args)
        {
            Console.WriteLine("starting");

            var config = new ApnsConfiguration(
                ApnsConfiguration.ApnsServerEnvironment.Sandbox,
                "path_to_cert.p12", // in dev mode place the cert in /bin/debug
                ""); // set the password to the cert here if applicable

            Console.WriteLine("Configuring Broker");
            var apnsBroker = new ApnsServiceBroker(config);

            apnsBroker.OnNotificationFailed += (notification, aggregateEx) =>
            {
                aggregateEx.Handle(ex =>
                {
                    // See what kind of exception it was to further diagnose
                    var exception = ex as ApnsNotificationException;
                    if (exception != null)
                    {
                        var notificationException = exception;

                        // Deal with the failed notification
                        var apnsNotification = notificationException.Notification;
                        var statusCode = notificationException.ErrorStatusCode;

                        Console.WriteLine($"Apple Notification Failed: ID={apnsNotification.Identifier}, Code={statusCode}");

                    }
                    else
                    {
                        // Inner exception might hold more useful information like an ApnsConnectionException           
                        Console.WriteLine($"Apple Notification Failed for some unknown reason : {ex.InnerException}");
                    }

                    // Mark it as handled
                    return true;
                });
            };

            apnsBroker.OnNotificationSucceeded += (notification) => {
                Console.WriteLine("Apple Notification Sent!");
            };

            Console.WriteLine("Starting Broker");
            apnsBroker.Start();

            Console.WriteLine("Queueing Notification");
            apnsBroker.QueueNotification(new ApnsNotification
            {
                DeviceToken = DeviceToken,
                // Say "Hello World", No badge, Use the default system sound
                Payload = JObject.Parse("{\"aps\":{\"alert\": \"Hello World!\", \"badge\":0, \"sound\":\"default\"}}")
            });

            Console.WriteLine("Stopping Broker");
            apnsBroker.Stop();
        }
    }
}
