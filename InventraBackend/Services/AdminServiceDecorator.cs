public abstract class AdminServiceDecorator : IAdminService
{
    protected readonly IAdminService _inner;

    protected AdminServiceDecorator(IAdminService inner)
    {
        _inner = inner;
    }

    public virtual Task<List<User>> GetUsers() => _inner.GetUsers();
    public virtual Task<bool> DeleteUser(int id) => _inner.DeleteUser(id);
    public virtual Task<bool> PromoteUser(int id) => _inner.PromoteUser(id);
    public virtual Task<bool> DemoteUser(int id) => _inner.DemoteUser(id);
}