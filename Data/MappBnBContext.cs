using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MAppBnB;

namespace MAppBnB.Data
{
    public class MappBnBContext : DbContext
    {
        public MappBnBContext (DbContextOptions<MappBnBContext> options)
            : base(options)
        {
        }

        public DbSet<MAppBnB.Booking> Booking { get; set; } = default!;
        public DbSet<MAppBnB.Room> Room { get; set; } = default!;
        public DbSet<MAppBnB.Accommodation> Accommodation { get; set; } = default!;
        public DbSet<MAppBnB.Document> Document { get; set; } = default!;
        public DbSet<MAppBnB.Person> Person { get; set; } = default!;
        public DbSet<MAppBnB.BookingPerson> BookingPerson { get; set; } = default!;
    }
}
