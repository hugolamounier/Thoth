using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Thoth.Dashboard.Audit;

public class ThothJwtAudit : IThothManagerAudit
{
    private readonly IEnumerable<string> _claimsToLog;
    private readonly IHttpContextAccessor _httpContextAccessor;

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
            if (claim is not null)
                info.Add(claimName, claim.Value);
        }

        return JsonConvert.SerializeObject(info);
    }
}