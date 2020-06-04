namespace Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AttendanceSheet",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AttendanceDate = c.DateTime(nullable: false),
                        ClassSessionId = c.Int(nullable: false),
                        MemberId = c.Int(nullable: false),
                        LastModified = c.DateTime(),
                        LastUserIdModifiedBy = c.Int(),
                        IsArchived = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ClassSession", t => t.ClassSessionId, cascadeDelete: true)
                .ForeignKey("dbo.Member", t => t.MemberId, cascadeDelete: true)
                .Index(t => t.ClassSessionId)
                .Index(t => t.MemberId)
                .Index(t => t.LastUserIdModifiedBy, name: "ModifiedById");
            
            CreateTable(
                "dbo.ClassSession",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        StartTime = c.DateTime(nullable: false),
                        EndTime = c.DateTime(nullable: false),
                        DayOfWeek = c.Int(nullable: false),
                        DisciplineId = c.Int(nullable: false),
                        LastModified = c.DateTime(),
                        LastUserIdModifiedBy = c.Int(),
                        IsArchived = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Discipline", t => t.DisciplineId, cascadeDelete: true)
                .Index(t => t.DisciplineId)
                .Index(t => t.LastUserIdModifiedBy, name: "ModifiedById");
            
            CreateTable(
                "dbo.Discipline",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        Description = c.String(),
                        LastModified = c.DateTime(),
                        LastUserIdModifiedBy = c.Int(),
                        IsArchived = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.LastUserIdModifiedBy, name: "ModifiedById");
            
            CreateTable(
                "dbo.DisciplineEnrolledMember",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Cost = c.Decimal(nullable: false, precision: 18, scale: 2),
                        RemainingCost = c.Decimal(nullable: false, precision: 18, scale: 2),
                        MembershipLength = c.Int(nullable: false),
                        StartDate = c.DateTime(nullable: false),
                        EndDate = c.DateTime(nullable: false),
                        DisciplineId = c.Int(nullable: false),
                        MemberId = c.Int(nullable: false),
                        LastModified = c.DateTime(),
                        LastUserIdModifiedBy = c.Int(),
                        IsArchived = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Discipline", t => t.DisciplineId, cascadeDelete: true)
                .ForeignKey("dbo.Member", t => t.MemberId, cascadeDelete: true)
                .Index(t => t.DisciplineId)
                .Index(t => t.MemberId)
                .Index(t => t.LastUserIdModifiedBy, name: "ModifiedById");
            
            CreateTable(
                "dbo.Member",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FirstName = c.String(nullable: false),
                        LastName = c.String(nullable: false),
                        IsInstructor = c.Boolean(nullable: false),
                        Note = c.String(),
                        LastModified = c.DateTime(),
                        LastUserIdModifiedBy = c.Int(),
                        IsArchived = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.LastUserIdModifiedBy, name: "ModifiedById");
            
            CreateTable(
                "dbo.Contact",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        RelationShip = c.String(),
                        IsPrimary = c.Boolean(nullable: false),
                        MemberId = c.Int(nullable: false),
                        LastModified = c.DateTime(),
                        LastUserIdModifiedBy = c.Int(),
                        IsArchived = c.Boolean(nullable: false),
                        User_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Member", t => t.MemberId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.User_Id)
                .Index(t => t.MemberId)
                .Index(t => t.LastUserIdModifiedBy, name: "ModifiedById")
                .Index(t => t.User_Id);
            
            CreateTable(
                "dbo.MemberAddress",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Street = c.String(),
                        City = c.String(),
                        State = c.String(),
                        ZipCode = c.Int(nullable: false),
                        ContactID = c.Int(nullable: false),
                        LastModified = c.DateTime(),
                        LastUserIdModifiedBy = c.Int(),
                        IsArchived = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Contact", t => t.ContactID, cascadeDelete: true)
                .Index(t => t.ContactID)
                .Index(t => t.LastUserIdModifiedBy, name: "ModifiedById");
            
            CreateTable(
                "dbo.MemberEmail",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Email = c.String(),
                        ContactID = c.Int(nullable: false),
                        LastModified = c.DateTime(),
                        LastUserIdModifiedBy = c.Int(),
                        IsArchived = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Contact", t => t.ContactID, cascadeDelete: true)
                .Index(t => t.ContactID)
                .Index(t => t.LastUserIdModifiedBy, name: "ModifiedById");
            
            CreateTable(
                "dbo.MemberPhone",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PhoneNumber = c.String(),
                        ContactID = c.Int(nullable: false),
                        LastModified = c.DateTime(),
                        LastUserIdModifiedBy = c.Int(),
                        IsArchived = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Contact", t => t.ContactID, cascadeDelete: true)
                .Index(t => t.ContactID)
                .Index(t => t.LastUserIdModifiedBy, name: "ModifiedById");
            
            CreateTable(
                "dbo.User",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Username = c.String(maxLength: 100),
                        Password = c.String(),
                        LastModified = c.DateTime(),
                        LastUserIdModifiedBy = c.Int(),
                        IsArchived = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Member", t => t.Id)
                .Index(t => t.Id)
                .Index(t => t.Username, name: "UsernameIndex")
                .Index(t => t.LastUserIdModifiedBy, name: "ModifiedById");
            
            CreateTable(
                "dbo.Payment",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Description = c.String(maxLength: 100),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Date = c.DateTime(nullable: false),
                        PaymentType = c.Int(nullable: false),
                        MemberID = c.Int(nullable: false),
                        LastModified = c.DateTime(),
                        LastUserIdModifiedBy = c.Int(),
                        IsArchived = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Member", t => t.MemberID, cascadeDelete: true)
                .Index(t => t.MemberID)
                .Index(t => t.LastUserIdModifiedBy, name: "ModifiedById");
            
            CreateTable(
                "dbo.Waiver",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Note = c.String(),
                        DateSigned = c.DateTime(nullable: false),
                        IsSigned = c.Boolean(nullable: false),
                        MemberId = c.Int(nullable: false),
                        LastModified = c.DateTime(),
                        LastUserIdModifiedBy = c.Int(),
                        IsArchived = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Member", t => t.MemberId, cascadeDelete: true)
                .Index(t => t.MemberId)
                .Index(t => t.LastUserIdModifiedBy, name: "ModifiedById");
            
            CreateTable(
                "dbo.Session",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        SessionHash = c.String(),
                        RememberMe = c.Boolean(nullable: false),
                        UserId = c.Int(nullable: false),
                        AttendanceLock = c.Boolean(nullable: false),
                        Expires = c.DateTime(nullable: false),
                        LastModified = c.DateTime(),
                        LastUserIdModifiedBy = c.Int(),
                        IsArchived = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.LastUserIdModifiedBy, name: "ModifiedById");


        }

        public override void Down()
        {
            DropForeignKey("dbo.Session", "UserId", "dbo.User");
            DropForeignKey("dbo.Waiver", "MemberId", "dbo.Member");
            DropForeignKey("dbo.User", "Id", "dbo.Member");
            DropForeignKey("dbo.Payment", "MemberID", "dbo.Member");
            DropForeignKey("dbo.DisciplineEnrolledMember", "MemberId", "dbo.Member");
            DropForeignKey("dbo.Contact", "User_Id", "dbo.User");
            DropForeignKey("dbo.MemberPhone", "ContactID", "dbo.Contact");
            DropForeignKey("dbo.MemberEmail", "ContactID", "dbo.Contact");
            DropForeignKey("dbo.MemberAddress", "ContactID", "dbo.Contact");
            DropForeignKey("dbo.Contact", "MemberId", "dbo.Member");
            DropForeignKey("dbo.AttendanceSheet", "MemberId", "dbo.Member");
            DropForeignKey("dbo.DisciplineEnrolledMember", "DisciplineId", "dbo.Discipline");
            DropForeignKey("dbo.ClassSession", "DisciplineId", "dbo.Discipline");
            DropForeignKey("dbo.AttendanceSheet", "ClassSessionId", "dbo.ClassSession");
            DropIndex("dbo.Session", new[] { "UserId" });
            DropIndex("dbo.Waiver", "ModifiedById");
            DropIndex("dbo.Waiver", new[] { "MemberId" });
            DropIndex("dbo.Payment", "ModifiedById");
            DropIndex("dbo.Payment", new[] { "MemberID" });
            DropIndex("dbo.User", "ModifiedById");
            DropIndex("dbo.User", "UsernameIndex");
            DropIndex("dbo.User", new[] { "Id" });
            DropIndex("dbo.MemberPhone", "ModifiedById");
            DropIndex("dbo.MemberPhone", new[] { "ContactID" });
            DropIndex("dbo.MemberEmail", "ModifiedById");
            DropIndex("dbo.MemberEmail", new[] { "ContactID" });
            DropIndex("dbo.MemberAddress", "ModifiedById");
            DropIndex("dbo.MemberAddress", new[] { "ContactID" });
            DropIndex("dbo.Contact", new[] { "User_Id" });
            DropIndex("dbo.Contact", "ModifiedById");
            DropIndex("dbo.Contact", new[] { "MemberId" });
            DropIndex("dbo.Member", "ModifiedById");
            DropIndex("dbo.DisciplineEnrolledMember", "ModifiedById");
            DropIndex("dbo.DisciplineEnrolledMember", new[] { "MemberId" });
            DropIndex("dbo.DisciplineEnrolledMember", new[] { "DisciplineId" });
            DropIndex("dbo.Discipline", "ModifiedById");
            DropIndex("dbo.ClassSession", "ModifiedById");
            DropIndex("dbo.ClassSession", new[] { "DisciplineId" });
            DropIndex("dbo.AttendanceSheet", "ModifiedById");
            DropIndex("dbo.AttendanceSheet", new[] { "MemberId" });
            DropIndex("dbo.AttendanceSheet", new[] { "ClassSessionId" });
            DropTable("dbo.Session");
            DropTable("dbo.Waiver");
            DropTable("dbo.Payment");
            DropTable("dbo.User");
            DropTable("dbo.MemberPhone");
            DropTable("dbo.MemberEmail");
            DropTable("dbo.MemberAddress");
            DropTable("dbo.Contact");
            DropTable("dbo.Member");
            DropTable("dbo.DisciplineEnrolledMember");
            DropTable("dbo.Discipline");
            DropTable("dbo.ClassSession");
            DropTable("dbo.AttendanceSheet");
        }
    }
}
