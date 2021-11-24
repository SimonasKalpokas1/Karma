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
using System.Threading.Tasks;

namespace Karma.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ListingController : ControllerBase
    {
        private readonly ILogger<ListingController> _logger;

        private readonly IListingRepository _listingRepository;

        private readonly Lazy<IUserService> _userService;

        private readonly IListingNotification _notification;

        public ListingController(ILogger<ListingController> logger, IListingRepository listingRepository, Lazy<IUserService> userService, IListingNotification notification)

        {
            _logger = logger;
            _listingRepository = listingRepository;
            _userService = userService;
            _notification = notification;
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> PostAsync(Listing listing)
        {
            listing.DatePublished = DateTime.UtcNow; //temp fix for curr date with form submit

            var userId = this.TryGetUserId();
            if (!userId.HasValue)
                return Unauthorized();

            listing.OwnerId = (int) userId;

            listing.isReserved = false;

            return await _listingRepository.AddAsync(listing) ? Ok() : StatusCode(500);
        }

        [HttpPost("id={id}/reserve={reserve}/for={receiverId}")]
        [Authorize]
        public async Task<ActionResult> ReserveListingAsync(int id, bool reserve, string receiverId)
        {
            var userId = this.TryGetUserId();
            var listing = await _listingRepository.GetByIdAsync(id);
            if (listing == null)
                return NotFound();
            if (userId != listing.OwnerId)
                return Unauthorized();

            listing.isReserved = reserve;
            listing.recipientId = receiverId;

            return await _listingRepository.UpdateAsync(listing) ? Ok() : StatusCode(500);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Listing>>> GetAllListingsAsync()
        {
            var listings = await _listingRepository.GetAllAsync();
            return listings != null ? Ok(listings) : StatusCode(500);
        }

        [HttpGet("userId={id}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Listing>>> GetListingsOfUserAsync(int id)
        {
            var userId = this.TryGetUserId();
            if (userId == null || id != userId)
                return Unauthorized();

            var listings = await _listingRepository.GetAllUserListingsAsync(id);
            return listings != null ? Ok(listings) : StatusCode(500);
        }

        [HttpGet("requesteeId={id}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Listing>>> GetRequestedListingsOfUserAsync(int id)
        {
            var userId = this.TryGetUserId();
            if (id != userId)
                return Unauthorized();

            var listings = await _listingRepository.GetRequestedListingsAsync(id);
            return listings != null ? Ok(listings) : StatusCode(500);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Listing>> GetListingByIdAsync(int id)
        {
            var listing = (await _listingRepository.GetByIdAsync(id));
            return listing != null ? Ok(listing) : NotFound();
        }

        [HttpGet("request/{id}")]
        [Authorize]
        public async Task<ActionResult> RequestListingAsync(int id)
        {
            var userId = this.TryGetUserId();
            if (userId == null)
                return Unauthorized();

            var listing = await _listingRepository.GetByIdAsync(id);
            if (listing == null)
                return NotFound();

            if (listing.OwnerId == userId)
                return Forbid();

            var user = _userService.Value.GetUserById((int) userId);
            if (listing.RequestedUserIDs.Contains((int) userId) || user.RequestedListings.Contains(id))
                return Conflict();

            listing.RequestedUserIDs.Add((int)userId);
            if (!await _listingRepository.UpdateAsync(listing))
                return StatusCode(500);

            Notify saveNotificationHandler = delegate {
                _logger.LogInformation("{0} - INFO - ListingController: User {1} requested listing {2}.", DateTime.UtcNow, userId, id);
            };

            _notification.saveNotification += saveNotificationHandler;
            _notification.Start();

            user.RequestedListings.Add(listing.Id);

            return Ok();
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult> DeleteListingAsync(int id)
        {
            var userId = this.TryGetUserId();
            var listing = await _listingRepository.GetByIdAsync(id);
            if (listing == null)
                return NotFound();
            if (userId == null || listing.OwnerId != userId)
                return Unauthorized();
            if (!await _listingRepository.DeleteByIdAsync(id))
                return StatusCode(500);
            _userService.Value.GetUserById((int) userId).RequestedListings.Remove(id);
            return Ok();
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult> UpdateListingAsync(int id, [FromBody] Listing listing)
        {

            var old = await _listingRepository.GetByIdAsync(listing.Id);
            if (old == null) 
                return NotFound();

            var userId = this.TryGetUserId();
            if (userId == null || old.OwnerId != userId)
                return Unauthorized();

            listing.DatePublished = DateTime.UtcNow; //temp fix for curr date with form submit
            listing.RequestedUserIDs = old.RequestedUserIDs; // temp fix for saving old requests
            listing.OwnerId = (int) userId;
            if (!await _listingRepository.UpdateAsync(listing))
                return StatusCode(500);

            return Ok();
        }
    }
}