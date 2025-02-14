using Google.Cloud.Firestore;

namespace PFC2025SWD63A.Models
{

    [FirestoreData] //this attribute makes the class recognized by Firestore as a Document
    public class User
    {
        [FirestoreProperty] //The property will be mapped onto a Firestore attribute
        public string Email { get; set; }
        
        [FirestoreProperty]
        public string FirstName { get; set; }
        
        [FirestoreProperty]
        public string LastName { get; set; }

    }
}
