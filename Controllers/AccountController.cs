using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Google.Cloud.Firestore.V1;
using PFC2025SWD63A.Repositories;
using PFC2025SWD63A.Models;

namespace PFC2025SWD63A.Controllers
{
    public class AccountController : Controller
    {
        FirestoreRepository _firestoreRepository;
        public AccountController(FirestoreRepository firestoreRepository) {
            _firestoreRepository = firestoreRepository;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            //once the user logs in successfully, the user will end up here
            //so we need to create a  record for the user in the db

            User myUser = new User();
            myUser.FirstName = User.Claims.FirstOrDefault(x => x.Type.Contains("givenname")).Value;
            myUser.LastName = User.Claims.FirstOrDefault(x => x.Type.Contains("surname")).Value;
            myUser.Email = User.Claims.FirstOrDefault(x => x.Type.Contains("email")).Value;

            //validating email - cannot be blank

            if(await (_firestoreRepository.UserExists(myUser.Email)) == false)
                 await _firestoreRepository.UpdateOrAddUser(myUser);

            //we will update the logs of when the user logged in....
            await _firestoreRepository.AddLoginLog(myUser.Email, Request.HttpContext.Connection.RemoteIpAddress.ToString());
            //this is for logged in users
            return View();
        }
 
        public async Task<IActionResult> Logout()
        {
            // using Microsoft.AspNetCore.Authentication;
            await HttpContext.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

    }
}
