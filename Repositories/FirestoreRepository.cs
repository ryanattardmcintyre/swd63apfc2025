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


    }
}
