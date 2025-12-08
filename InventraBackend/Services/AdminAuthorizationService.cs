public class AdminAuthorizationDecorator : AdminServiceDecorator
{
    private readonly IHttpContextAccessor _http;

    public AdminAuthorizationDecorator(IAdminService inner, IHttpContextAccessor http)
        : base(inner)
    {
        _http = http;
    }

    private void CheckAdmin()
    {
        var role = _http.HttpContext?.Session?.GetString("Role");
        if (role != "admin")
            throw new UnauthorizedAccessException("Admin access required.");
    }

    public override async Task<List<User>> GetUsers()
    {
        CheckAdmin();
        return await base.GetUsers();
    }

    public override async Task<bool> DeleteUser(int id)
    {
        CheckAdmin();
        return await base.DeleteUser(id);
    }

    public override async Task<bool> PromoteUser(int id)
    {
        CheckAdmin();
        return await base.PromoteUser(id);
    }

    public override async Task<bool> DemoteUser(int id)
    {
        CheckAdmin();
        return await base.DemoteUser(id);
    }
}