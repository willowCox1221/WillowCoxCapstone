public interface IAdminService
{
    Task<List<User>> GetUsers();
    Task<bool> DeleteUser(int id);
    Task<bool> PromoteUser(int id);
    Task<bool> DemoteUser(int id);
}