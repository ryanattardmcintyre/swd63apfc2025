using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PFC2025SWD63A.Models;
using PFC2025SWD63A.Repositories;

namespace PFC2025SWD63A.Controllers
{

    [Authorize]
    public class FileController : Controller
    {
        private BucketRepository _bucketRepository;
        private FirestoreRepository _firestoreRepository;
        public FileController(BucketRepository bucketRepository, FirestoreRepository firestoreRepository) {
            _bucketRepository = bucketRepository;
            _firestoreRepository = firestoreRepository;
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(IFormFile file, string recipient) //ryanattard@gmail.com;joeborg@gmail.com;...
        {
            string owner = User.Claims.FirstOrDefault(x => x.Type.Contains("email")).Value;

            string uniqueFilename = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(file.FileName).ToLower();

            //1. upload the file
            MemoryStream msUpload = new MemoryStream();
            file.CopyTo(msUpload); msUpload.Position = 0;
            await _bucketRepository.Upload(uniqueFilename, msUpload);

            //2. we need to update the firestore
            await _firestoreRepository.AddFileToUser(owner, uniqueFilename);

            string [] recipients = recipient.Split(';');
            foreach (var r in recipients)
            {
                await _firestoreRepository.AddUserToFile(owner, r, uniqueFilename, "reader");

                await _firestoreRepository.AddFileToAsShared(owner, r, uniqueFilename);
            }
            await _firestoreRepository.AddUserToFile(owner, owner, uniqueFilename, "owner");

            //3. to assign permissions to the recipient + the owner

            foreach (var r in recipients)
            {
                await _bucketRepository.AssignPermission(uniqueFilename, r, BucketRoles.READER);
            }
            await _bucketRepository.AssignPermission(uniqueFilename, owner, BucketRoles.OWNER);

            return View();
        }



        public async Task<IActionResult> Index()
        {
            string currentlyLoggedInUser  = User.Claims.FirstOrDefault(x => x.Type.Contains("email")).Value;


            var uploadedFiles = await _firestoreRepository.GetUploadedFilesForUser(currentlyLoggedInUser);
            var sharedFiles = await _firestoreRepository.GetSharedFilesForUser(currentlyLoggedInUser);

            FilesModel mymodel = new FilesModel();
            mymodel.UploadedFiles = uploadedFiles;
            mymodel.SharedFiles = sharedFiles;

            return View(mymodel);

        }


    }
}
