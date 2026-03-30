using TechRent.Models.Entities;

namespace TechRent.Models.Entities
{
    public class ProfileViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public string UserAvatar { get; set; }
        public bool IsPremium { get; set; }

        public Booking ActiveBooking { get; set; }
        public List<Booking> UpcomingBookings { get; set; } = new List<Booking>();
        public List<Booking> HistoryBookings { get; set; } = new List<Booking>();
    }
}