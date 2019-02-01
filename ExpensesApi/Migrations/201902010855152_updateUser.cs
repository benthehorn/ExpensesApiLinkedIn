namespace ExpensesApi.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateUser : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "Hash", c => c.String());
            AddColumn("dbo.Users", "Salt", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "Salt");
            DropColumn("dbo.Users", "Hash");
        }
    }
}
