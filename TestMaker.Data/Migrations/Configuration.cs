namespace TestMaker.Data.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using TestMaker.Data;

    public sealed class Configuration : DbMigrationsConfiguration<TestMakerContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            this.AutomaticMigrationDataLossAllowed = true;            
        }

        protected override void Seed(TestMakerContext context)
        {
        }
    }
}
