namespace SimpleIdentityServer.Core
{
    using System.Data.Entity;

    public class Model : DbContext
    {
        public Model()
            : base("name=Model")
        {
        }

        // public virtual DbSet<MyEntity> MyEntities { get; set; }
    }

    //public class MyEntity
    //{
    //    public int Id { get; set; }
    //    public string Name { get; set; }
    //}
}