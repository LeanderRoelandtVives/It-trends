using System;
using System.Collections.Generic;
using System.Text;

namespace RestaurantAi.Dto.Request
{
    public class MenuItemRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; }
        public string Category { get; set; }
    }
}
