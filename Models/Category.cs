using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace Karma.Models
{
    public class Category: Entity
	{
		public Category()
		{
			this.Listings = new List<Listing>();
		}

		public string Name { get; set; }

		[JsonIgnore]
		public virtual List<Listing> Listings { get; set; }

		public string ImagePath { get; set; }
	}
}