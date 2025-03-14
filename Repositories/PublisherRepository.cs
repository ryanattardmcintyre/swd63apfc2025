using Google.Cloud.PubSub.V1;
using Newtonsoft.Json;
using static System.Net.Mime.MediaTypeNames;

namespace PFC2025SWD63A.Repositories
{
    public class PublisherRepository
    {
        string _projectId;
        string _topicId;
        public PublisherRepository(string projectId, string topicId) 
        { 
            _projectId = projectId;
            _topicId = topicId;
        }


        public async Task<string> AddToRenderingQueue(string email, string uri)
        {
            TopicName topicName = TopicName.FromProjectTopic(_projectId, _topicId);
            PublisherClient publisher = await PublisherClient.CreateAsync(topicName);

            var jsonObject = new { email, uri };
            string jsonString = JsonConvert.SerializeObject(jsonObject, Formatting.Indented);
            string message = await publisher.PublishAsync(jsonString);
            return message;
 
        }
    }
}
