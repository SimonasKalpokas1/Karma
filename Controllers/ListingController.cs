using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Karma.Models;
using Karma.Repositories;
using Karma.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Linq;

namespace Karma.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ListingController : ControllerBase
    {
        private readonly ILogger<ListingController> _logger;

        private readonly IListingRepository _listingRepository;

        private readonly IUserService _userService;

        public event EventHandler nofitication;

        public ListingController(ILogger<ListingController> logger, IListingRepository listingRepository, IUserService userService)
        {
            _logger = logger;
            _listingRepository = listingRepository;
            _userService = userService;
        }

        [HttpPost]
        [Authorize]
        public ActionResult Post(Listing listing)
        {
            var random = new Random();
            listing.Id = random.Next(9999).ToString(); // temp fix for id generation, later this should be assigned in DB.
            listing.DatePublished = DateTime.UtcNow; //temp fix for curr date with form submit

            string userId = User.FindFirst(ClaimTypes.Name)?.Value;
            listing.OwnerId = userId;
            _listingRepository.AddListing(listing);
            return StatusCode(StatusCodes.Status200OK);
        }

        [HttpGet]
        public IEnumerable<Listing> GetAllListings()
        {
            return _listingRepository.GetAllListings();
        }

        [HttpGet("userId={id}")]
        [Authorize]
        public ActionResult<IEnumerable<Listing>> GetListingsOfUser(string id)
        {   
            string userId = User.FindFirst(ClaimTypes.Name)?.Value;
            if(id != userId)
                return Unauthorized();

            return _listingRepository.GetAllUserListings(id).ToList();
        }

        [HttpGet("requesteeId={id}")]
        [Authorize]
        public ActionResult<IEnumerable<Listing>> GetRequestedListingsOfUser(string id)
        {
            string userId = User.FindFirst(ClaimTypes.Name)?.Value;
            if (id != userId)
                return Unauthorized();

            return _listingRepository.GetRequestedListings(id).ToList();
        }

        [HttpGet("{id}")]
        public IActionResult GetListingById(string id)
        {
            var listing =  _listingRepository.GetListingById(id);
            return listing != null ? Ok(listing) : NotFound();
        }

        [HttpGet("request/{id}")]
        [Authorize]
        public IActionResult RequestListing(string id)
        {
            string userId = User.FindFirst(ClaimTypes.Name)?.Value;
            var listing = _listingRepository.GetListingById(id);
            var user = _userService.GetUserById(userId);
            if (listing.OwnerId == userId)
                return Forbid();
            
            if(listing.RequestedUserIDs.Contains(userId) || user.RequestedListings.Contains(id))
                return Conflict();

            listing.RequestedUserIDs.Add(userId);
            _listingRepository.UpdateListing(listing);
            Console.WriteLine("console write line works");
            nofitication += nofiticationHandler;
            onNofitication();
            user.RequestedListings.Add(listing.Id);
            
            return Ok();
        }

        protected virtual void onNofitication()
        {
            if (nofitication != null) nofitication(this);
        }

        private static void nofiticationHandler (object sender)
        {
            Console.WriteLine("Nofitication event works!!!");
        }

        [HttpDelete("{id}")]
        [Authorize]
        public IActionResult DeleteListing(string id)
        {
            string userId = User.FindFirst(ClaimTypes.Name)?.Value;
            var listing = _listingRepository.GetListingById(id);
            if (listing.OwnerId != userId)
                return Unauthorized();
            _listingRepository.DeleteListingById(id);
            return Ok();
        }

        [HttpPut("{id}")]
        [Authorize]
        public IActionResult UpdateListing(string id, [FromBody] Listing listing)
        {

            var old = _listingRepository.GetListingById(listing.Id);
            if (old == null) return NotFound();

            string userId = User.FindFirst(ClaimTypes.Name)?.Value;
            if (old.OwnerId != userId) return Unauthorized();

            listing.DatePublished = DateTime.UtcNow; //temp fix for curr date with form submit
            listing.OwnerId = userId;
            _listingRepository.UpdateListing(listing);
            return Ok();
        }
    }
}