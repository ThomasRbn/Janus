using System.DirectoryServices.Protocols;
using System.Net;
using Janus.Domain.Interfaces.Services;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity;
using Janus.Domain.Entities;

namespace Janus.API.Services.Auth;

using Janus.API.Options;

public class LdapAuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly LdapOptions _ldapOptions;
    private readonly ILogger<LdapAuthService> _logger;

    public LdapAuthService(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IOptions<LdapOptions> ldapOptions,
        ILogger<LdapAuthService> logger
    )
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _ldapOptions = ldapOptions.Value;
        _logger = logger;
    }

    public Task<string> LoginAsync(string email, string password)
    {
        string searchBase = _ldapOptions.SearchBase;
        string filter = string.Format(_ldapOptions.UserFilter, email);
        string uniqueIdAttr = _ldapOptions.UniqueIdAttribute;

        // Parse the LDAP server URL to extract host and port
        var serverUri = new Uri(_ldapOptions.Server);
        var ldapIdentifier = new LdapDirectoryIdentifier(serverUri.Host, serverUri.Port);
        
        using var ldap = new LdapConnection(ldapIdentifier);
        
        try
        {
            ldap.AuthType = AuthType.Basic;
            ldap.SessionOptions.ProtocolVersion = 3; // Force LDAPv3
            
            // First, bind with admin credentials to search for the user
            var adminCredential = new NetworkCredential(_ldapOptions.BindDn, _ldapOptions.BindPassword);
            ldap.Bind(adminCredential);

            // Search for the user
            var searchRequest = new SearchRequest(
                searchBase,
                filter,
                SearchScope.Subtree,
                uniqueIdAttr, "dn"
            );
            var searchResponse = (SearchResponse)ldap.SendRequest(searchRequest);
            if (searchResponse.Entries.Count == 0)
                throw new UnauthorizedAccessException("User not found in LDAP");
                
            var userEntry = searchResponse.Entries[0];
            var userDn = userEntry.DistinguishedName;
            
            // Now authenticate the user with their actual DN
            var userCredential = new NetworkCredential(userDn, password);
            ldap.Bind(userCredential);

            // Get the unique ID from the found entry
            var uniqueId = userEntry.Attributes[uniqueIdAttr][0];
            if (uniqueId == null)
                throw new Exception($"LDAP attribute '{uniqueIdAttr}' is missing or null for user {email}");
            string uuid = uniqueId is byte[] bytes ? new Guid(bytes).ToString() : uniqueId.ToString() ?? string.Empty;

            // Pour l'instant, on retourne juste l'UUID sans synchronisation avec la DB
            _logger.LogInformation("LDAP authentication successful for {Email}, UUID: {Uuid}", email, uuid);

            return Task.FromResult(uuid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LDAP authentication failed for {Email}", email);
            throw new UnauthorizedAccessException("LDAP authentication failed", ex);
        }
    }

    public async Task<Guid> SignupAsync(string email, string password)
    {
        var user = new User {
            UserName = email,
            Email = email,
            FirstName = "LDAP",
            LastName = "User"
        };
        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
            throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));

        return user.Id;
    }
}