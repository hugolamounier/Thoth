using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Thoth.Dashboard.Audit;

public class ThothJwtAudit : IThothManagerAudit
{
    private readonly IEnumerable<string> _claimsToLog;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private const string ClaimType2005Namespace = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims";
    private const string ClaimTypeNamespace = "http://schemas.microsoft.com/ws/2008/06/identity/claims";

    public ThothJwtAudit(IHttpContextAccessor httpContextAccessor, IEnumerable<string> claimsToLog)
    {
        _httpContextAccessor = httpContextAccessor;
        _claimsToLog = claimsToLog;
    }

    public string AddAuditExtras()
    {
        if (!_claimsToLog.Any())
            return string.Empty;

        var info = new Dictionary<string, string>();

        foreach (var claimName in _claimsToLog)
        {
            var claim = _httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(x => x.Type == claimName);
            if (claim is null) continue;

            if (claimName.StartsWith(ClaimType2005Namespace))
            {
                var replacedClaimName = claimName.Replace($"{ClaimType2005Namespace}/", "");
                info.Add(replacedClaimName, claim.Value);
                continue;
            }

            if (claimName.StartsWith(ClaimTypeNamespace))
            {
                var replacedClaimName = claimName.Replace($"{ClaimTypeNamespace}/", "");
                info.Add(replacedClaimName, claim.Value);
                continue;
            }

            info.Add(claimName, claim.Value);
        }

        return JsonConvert.SerializeObject(info);
    }
}