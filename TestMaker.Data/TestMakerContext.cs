using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using TestMaker.Model;

namespace TestMaker.Data
{
    public class TestMakerContext : DbContext
    {
        public TestMakerContext()
            :base("TestMakerContext")
        {

        }

        public DbSet<Question> Questions { get; set; }
    }
}
