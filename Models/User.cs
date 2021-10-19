using System;
using System.Collections.Generic;


namespace Karma.Models
{
    public class User: Entity
    {
        public User()
        {
            this.Listings = new List<Listing>();
            this.Comments = new List<Comment>();
        }
        public string Username { get; set; }
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public virtual List<Listing> Listings { get; set; }

        public virtual List<Comment> Comments { get; set; }

        public string AvatarPath { get; set; }

        public string Token { get; set; }
    }
}