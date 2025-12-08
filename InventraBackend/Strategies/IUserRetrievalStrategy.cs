using System.Threading.Tasks;

namespace InventraBackend.Strategies
{
    public interface IUserRetrievalStrategy
    {
        Task<object?> GetUserProfileAsync(string username);
    }
}