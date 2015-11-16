namespace SimpleIdentityServer.Core.Repositories
{
    public interface IJwtBearerClientRepository
    {
        bool Insert(string id);

        bool Exist(string id);
    }
}
