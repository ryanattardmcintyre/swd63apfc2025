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
        private PublisherRepository _publisherRepository;
        public FileController(BucketRepository bucketRepository, FirestoreRepository firestoreRepository, PublisherRepository publisherRepository) {
            _bucketRepository = bucketRepository;
            _firestoreRepository = firestoreRepository;
            _publisherRepository = publisherRepository;
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


            List<FileListingViewModel> uploadedFiles = await _firestoreRepository.GetUploadedFilesForUser(currentlyLoggedInUser);
            var sharedFiles = await _firestoreRepository.GetSharedFilesForUser(currentlyLoggedInUser);

            FilesModel mymodel = new FilesModel();
            mymodel.UploadedFiles = uploadedFiles;
            mymodel.SharedFiles = sharedFiles;

            return View(mymodel);

        }


        public async Task<IActionResult> ExportPdf(string fileId)
        {
            //1. query the  firestore for details about the file
            string currentlyLoggedInUser = User.Claims.FirstOrDefault(x => x.Type.Contains("email")).Value;
            string fileNameToBeRendered = "";
            List<FileListingViewModel> uploadedFiles = await _firestoreRepository.GetUploadedFilesForUser(currentlyLoggedInUser);
            if(uploadedFiles.Count(x=> x.Filename.Contains(fileId))>0)
            {
                //file found
                fileNameToBeRendered = uploadedFiles.SingleOrDefault(x => x.Filename.Contains(fileId)).Filename;
            }
            else
            {
                List<string> sharedFiles = await _firestoreRepository.GetSharedFilesForUser(currentlyLoggedInUser);
                if (sharedFiles.Count(x => x.Contains(fileId)) > 0)
                {
                    //file found
                    fileNameToBeRendered = sharedFiles.SingleOrDefault(x => x.Contains(fileId));
                }
                else
                {
                    TempData["error"] = "File wasn't found";
                    return RedirectToAction("Index");
                }
            }

            //2. after details obtained, we publish to the topic
            string result = await _publisherRepository.AddToRenderingQueue(currentlyLoggedInUser, fileNameToBeRendered);

            TempData["message"] = "Process started...once it is ready you will be notified";
            return RedirectToAction("Index");
        }


        public IActionResult Render([FromServices] SubscriberRepository subscriberRepository)
        {
            subscriberRepository.PullMessagesSync(true); //pull from queue //conversion to pdf
            //notify the user about the end of the process

            


            return Content("done");
        }


    }
}
