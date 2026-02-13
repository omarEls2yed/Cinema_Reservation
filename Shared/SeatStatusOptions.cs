using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public enum SeatStatusOptions
    {
        available = 0, // no lock in Redis and no ticket in SQL, it is available.
        locked = 1, // Status becomes locked (saved in Redis) this mean someone try to take it.
        booked = 2 // This status is stored permanently in your SQL Database (as a Ticket record).
    }

    /*
        Seat Map (Showtime)      	YES	10,000 people view this at once. Caching saves the DB.
        Seat Locking	            YES	Needs to be fast and temporary (expires in 10 mins).
        User Profile/History	    NO	Only 1 user views this. No performance gain from caching.
        Payment Processing	        NO	Critical financial data. Must go directly to SQL to ensure it's saved permanently.
        List of Movies	            YES	The list of movies doesn't change often, but everyone looks at it.
        Admin Reports	            NO	Admins run this once a month. Speed doesn't matter.
     */
}
