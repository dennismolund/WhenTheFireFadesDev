using System.Security.Claims;

namespace Web.Helpers;

public class SessionHelper(IHttpContextAccessor httpContextAccessor)
{
    private const string TempUserIdKey = "TempUserId";
    private const string PlayerNicknameKey = "PlayerNickname";
    private const string CurrentGameCodeKey = "CurrentGameCode";

    private ISession? Session => httpContextAccessor.HttpContext?.Session;

    public int GetOrCreateTempUserId()
    {
        var tempUserId = GetTempUserId();
        if (tempUserId != null) return tempUserId.Value;
        tempUserId = GenerateTempUserId();
        SetTempUserId(tempUserId.Value);
        var playerNickname = $"Player#{tempUserId}";
        SetPlayerNickname(playerNickname);
        return tempUserId.Value;
    }

    public virtual int? GetTempUserId()
    {
        return Session?.GetInt32(TempUserIdKey);
    }

    public void SetTempUserId(int tempUserId)
    {
        Session?.SetInt32(TempUserIdKey, tempUserId);
    }

    public void ClearTempUserId()
    {
        Session?.Remove(TempUserIdKey);
    }

    public string? GetPlayerNickname()      
    {
        return Session?.GetString(PlayerNicknameKey);
    }

    public void SetPlayerNickname(string nickname)
    {
        Session?.SetString(PlayerNicknameKey, nickname);
    }
    
    public string? GetCurrentGameCode()
    {
        return Session?.GetString(CurrentGameCodeKey);
    }

    public void SetCurrentGameCode(string code)
    {
        Session?.SetString(CurrentGameCodeKey, code);
    }

    public void ClearCurrentGameCode()
    {
        Session?.Remove(CurrentGameCodeKey);
    }
    
    public string? GetAuthenticatedUserId()
    {
        return httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    public string? GetAuthenticatedUserName()
    {
        var user = httpContextAccessor.HttpContext?.User;
        return user?.Identity?.IsAuthenticated == true ? user.Identity?.Name : null;
    }

    private static int GenerateTempUserId()
    {
        return Random.Shared.Next(10000, 99999);
    }
}
