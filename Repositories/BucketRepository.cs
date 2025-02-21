using System.Security.AccessControl;
using System.Text;
using Google.Apis.Storage.v1.Data;
using Google.Cloud.Storage.V1;

namespace PFC2025SWD63A.Repositories
{

    public enum BucketRoles {OWNER, READER};

    public class BucketRepository
    {
     
        string _bucketId;
        public BucketRepository(string bucketName) { 
            _bucketId = bucketName;
        }

        public async Task<Google.Apis.Storage.v1.Data.Object> Upload(string filename, MemoryStream msUpload)
        {
            var storage = StorageClient.Create();
            msUpload.Position = 0;
            return await storage.UploadObjectAsync(_bucketId, filename, "application/octet-stream", msUpload);
        }


        public async Task<Google.Apis.Storage.v1.Data.Object> AssignPermission(string filename, string recipient, BucketRoles role)
        {
            var storage = StorageClient.Create();
            var storageObject = storage.GetObject(_bucketId, filename, new GetObjectOptions
            {
                Projection = Projection.Full
            });

            string roleStr = role == BucketRoles.OWNER ? "OWNER" : "READER";


            storageObject.Acl.Add(new ObjectAccessControl
            {
                Bucket = _bucketId,
                Entity = $"user-{recipient}",
                Role = roleStr,
            });

            return  await storage.UpdateObjectAsync(storageObject);
        }
    }
}
