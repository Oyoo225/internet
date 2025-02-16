using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace FriendSQLiteMVCVSCode.Models
{
    public class FriendDbContext : DbContext
    {
        public FriendDbContext(DbContextOptions<FriendDbContext> options) : base(options)
        {}
        public DbSet<FriendViewModel> Friends { get; set; }  // Friends is Db's Table Name
    }
}