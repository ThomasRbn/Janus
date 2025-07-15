namespace Janus.API.Options;

public class LdapOptions
{
    public string Server { get; set; } = string.Empty;
    public string SearchBase { get; set; } = string.Empty;
    public string UserFilter { get; set; } = string.Empty; // ex: (mail={0}) ou (uid={0})
    public string UniqueIdAttribute { get; set; } = string.Empty; // ex: objectGUID, entryUUID, uidNumber
    public string BaseDn { get; set; } = string.Empty;
    public string BindDn { get; set; } = string.Empty;
    public string BindPassword { get; set; } = string.Empty;
}