using System.DirectoryServices.Protocols;
using System.Net;
using Janus.Domain.Interfaces.Services;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity;
using Janus.Domain.Entities;
using Janus.Domain.Exceptions.Auth;
using Janus.Domain.Dtos;

namespace Janus.API.Services.Auth;

using System.Security.Authentication;
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

    public async Task<string> LoginAsync(LoginDto loginDto)
    {
        if (loginDto == null)
            throw new ArgumentNullException(nameof(loginDto));
            
        string searchBase = _ldapOptions.SearchBase;
        string filter = string.Format(_ldapOptions.UserFilter, loginDto.Email);
        string uniqueIdAttr = _ldapOptions.UniqueIdAttribute;

        var serverUri = new Uri(_ldapOptions.Server);
        var ldapIdentifier = new LdapDirectoryIdentifier(serverUri.Host, serverUri.Port);
        
        using var ldap = new LdapConnection(ldapIdentifier);
        
        try
        {
            ldap.AuthType = AuthType.Basic;
            ldap.SessionOptions.ProtocolVersion = 3;
            
            var adminCredential = new NetworkCredential(_ldapOptions.BindDn, _ldapOptions.BindPassword);
            ldap.Bind(adminCredential);

            var searchRequest = new SearchRequest(
                searchBase,
                filter,
                SearchScope.Subtree,
                uniqueIdAttr, "dn", "givenName", "sn"
            );
            var searchResponse = (SearchResponse)ldap.SendRequest(searchRequest);
            if (searchResponse.Entries.Count == 0)
                throw new AuthenticationException("User not found in LDAP directory");
                
            var userEntry = searchResponse.Entries[0];
            var userDn = userEntry.DistinguishedName;
            
            var userCredential = new NetworkCredential(userDn, loginDto.Password);
            ldap.Bind(userCredential);

            var uniqueId = userEntry.Attributes[uniqueIdAttr][0];
            if (uniqueId == null)
                throw new AuthenticationException($"LDAP attribute '{uniqueIdAttr}' is missing for user {loginDto.Email}");
            
            string uuid = uniqueId is byte[] bytes ? new Guid(bytes).ToString() : uniqueId.ToString() ?? string.Empty;

            var firstName = userEntry.Attributes["givenName"]?[0]?.ToString() ?? "LDAP";
            var lastName = userEntry.Attributes["sn"]?[0]?.ToString() ?? "User";

            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null)
            {
                user = new User {
                    UserName = loginDto.Email,
                    Email = loginDto.Email,
                    FirstName = firstName,
                    LastName = lastName,
                    LdapUuid = uuid
                };
                var result = await _userManager.CreateAsync(user);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description);
                    throw new UserCreationException(errors);
                }
            }
            else
            {
                if (string.IsNullOrEmpty(user.LdapUuid) || user.LdapUuid != uuid)
                {
                    user.LdapUuid = uuid;
                    await _userManager.UpdateAsync(user);
                }
            }

            await _signInManager.SignInAsync(user, isPersistent: false);

            _logger.LogInformation("LDAP authentication successful for {Email}, UUID: {Uuid}", loginDto.Email, uuid);
            return uuid;
        }
        catch (AuthenticationException)
        {
            _logger.LogWarning("LDAP authentication failed for {Email}", loginDto.Email);
            throw;
        }
        catch (UserCreationException)
        {
            _logger.LogError("Failed to create local user from LDAP for {Email}", loginDto.Email);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected LDAP error for {Email}", loginDto.Email);
            throw new AuthenticationException("LDAP authentication failed due to server error", ex);
        }
    }

    public Task<Guid> SignupAsync(SignUpDto signupDto)
    {
        throw new NotSupportedException("LDAP does not support user signup. Please create users directly in the LDAP directory.");
    }
}