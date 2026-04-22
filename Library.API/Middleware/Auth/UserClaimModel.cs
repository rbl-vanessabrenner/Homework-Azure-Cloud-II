namespace Library.API.Middleware.Auth;

public class UserClaimModel
{   
    public string ClaimUserEmail { get; set; }
    public IEnumerable<string> ClaimRoles { get; set; } = [];
}
