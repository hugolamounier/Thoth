namespace Thoth.Dashboard.Audit;

public interface IThothManagerAudit
{
    /// <summary>
    /// Should return a string that contains any extra information that would be useful for auditing changes
    /// </summary>
    /// <returns></returns>
    string AddAuditExtras();
}