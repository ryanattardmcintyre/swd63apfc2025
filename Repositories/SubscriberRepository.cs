using Google.Cloud.PubSub.V1;
using Google.Cloud.Storage.V1;
using Grpc.Core;
using Newtonsoft.Json;
using System.Security.AccessControl;
using static System.Net.Mime.MediaTypeNames;
using System.IO;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;

namespace PFC2025SWD63A.Repositories
{
    public class SubscriberRepository
    {
        string _projectId;
        string _topicId;
        string _subscriptionId;
        string _bucketId;
        public SubscriberRepository(string projectId, string topicId, string subscriptionId, string bucketId)
        {
            _bucketId = bucketId;
            _projectId = projectId;
            _topicId = topicId;
            _subscriptionId = subscriptionId;
        }

        public void PullMessagesSync(bool acknowledge=true)
        {
            SubscriptionName subscriptionName = SubscriptionName.FromProjectSubscription(_projectId, _subscriptionId);
            SubscriberServiceApiClient subscriberClient = SubscriberServiceApiClient.Create();
          
            try
            {
                // Pull messages from server,
                // allowing an immediate response if there are no messages.
                PullResponse response = subscriberClient.Pull(subscriptionName, maxMessages: 1);
                // Print out each received message.

                ReceivedMessage message = response.ReceivedMessages.FirstOrDefault();

                if (message != null)
                {
                    string text = message.Message.Data.ToStringUtf8();
                    dynamic obj = JsonConvert.DeserializeObject(text);
                    string uri = obj.uri;

                    // If acknowledgement required, send to server.
                    if (acknowledge)
                    {
                        subscriberClient.Acknowledge(subscriptionName, response.ReceivedMessages.Select(msg => msg.AckId));
                    }

                    var storage = StorageClient.Create();
                    MemoryStream stream = new MemoryStream();
                    storage.DownloadObject(_bucketId, uri, stream);

                    MemoryStream msPdf = ConvertToPdf(stream);

                    string newFilename = System.IO.Path.GetFileNameWithoutExtension(uri) + ".pdf";

                    storage.UploadObject(_bucketId, newFilename, "application/octet-stream", msPdf);


                    //we send an email?
                    //we add a status field in firestore?
                }
             
            }
            catch (RpcException ex) when (ex.Status.StatusCode == StatusCode.Unavailable)
            {
                // UNAVAILABLE due to too many concurrent pull requests pending for the given subscription.
            }
            catch (Exception ex)
            {
                //solution
                //we publish again the message read
            }
          
        }

        public MemoryStream ConvertToPdf(MemoryStream inputStream)
        {
            // Create a new MemoryStream for the PDF
            MemoryStream pdfStream = new MemoryStream();

            // Create a PDF document
            PdfDocument document = new PdfDocument();
            PdfPage page = document.AddPage();
            XGraphics gfx = XGraphics.FromPdfPage(page);

            // Read text from the input MemoryStream
            inputStream.Position = 0; // Reset to start
            using (StreamReader reader = new StreamReader(inputStream))
            {
                string text = reader.ReadToEnd();

                // Draw the text on the page (simple positioning)
                XFont font = new XFont("Arial", 12);
                gfx.DrawString(text, font, XBrushes.Black, new XRect(10, 10, page.Width - 20, page.Height - 20), XStringFormats.TopLeft);
            }

            // Save the PDF to the MemoryStream
            document.Save(pdfStream, false); // 'false' keeps the stream open
            pdfStream.Position = 0; // Reset position for further use

            return pdfStream;
        }
    }
}
