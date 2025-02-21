using Google.Cloud.Firestore;
using PFC2025SWD63A.Models;

namespace PFC2025SWD63A.Repositories
{
    //the idea of creating repository classes, is that they will facilitate the communication
    //with the cloud technologies such as Firestore

    public class FirestoreRepository
    {
        FirestoreDb db;
        public FirestoreRepository(string projectId) 
        {
           db = FirestoreDb.Create(projectId); //this is going to trigger an error
           //this line will seek that this application has enough rights to access the projectId
        }

        public async Task<WriteResult> UpdateOrAddUser(User user)
        {
            DocumentReference docRef = db.Collection("users").Document(user.Email);
            return await docRef.SetAsync(user);
        }

        public async Task<bool> UserExists(string email)
        {
            DocumentReference docRef = db.Collection("users").Document(email);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();
            return snapshot.Exists;
        }

        public async Task<WriteResult>  AddLoginLog(string email, string ipaddress)
        {
            DocumentReference docRef = db.Collection("users").Document(email).Collection("logs").Document();
            Dictionary<string, object> log = new Dictionary<string, object>
            {
                { "ipaddress", ipaddress },
                { "loggedAt", Google.Cloud.Firestore.Timestamp.GetCurrentTimestamp() }
            };
            return await docRef.SetAsync(log);
        }


        public async Task<WriteResult> AddFileToUser(string ownerEmail, string filename)
        {
            DocumentReference docRef = db.Collection("users").Document(ownerEmail).Collection("files")
                .Document(System.IO.Path.GetFileNameWithoutExtension(filename));
            Dictionary<string, object> log = new Dictionary<string, object>
            {
                { "filename", filename },
                { "uploadedAt", Google.Cloud.Firestore.Timestamp.GetCurrentTimestamp() }
            };
            return await docRef.SetAsync(log);
        }

        public async Task<WriteResult> AddUserToFile(string ownerEmail, string recipient, string filename, string permissionType)
        {
            DocumentReference docRef = db.Collection("users").Document(ownerEmail).Collection("files")
                .Document(System.IO.Path.GetFileNameWithoutExtension(filename)).Collection("permissions")
                .Document(recipient);
            Dictionary<string, object> log = new Dictionary<string, object>
            {
                { "email", recipient },
                { "updatedOn", Google.Cloud.Firestore.Timestamp.GetCurrentTimestamp() },
                { "type", permissionType}
            };
            return await docRef.SetAsync(log);
        }


        public async Task<List<string>> GetAllFilesForUser(string owner)
        {
            List<string> filenames = new List<string>();

            QuerySnapshot fileQuerySnapshot =
                await db.Collection("users").Document(owner).Collection("files").GetSnapshotAsync();
            
            foreach (DocumentSnapshot documentSnapshot in fileQuerySnapshot.Documents)
            {
                Dictionary<string, object> city = documentSnapshot.ToDictionary();
                foreach (KeyValuePair<string, object> pair in city)
                {
                    if (pair.Key == "filename") filenames.Add(pair.Value.ToString());
                }
            }

            return filenames;
        }
    }
}
