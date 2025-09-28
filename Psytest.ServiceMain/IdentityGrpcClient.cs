using PsyTest.identity;

namespace Psytest.ServiceMain
{
    public class IdentityGrpcClient
    {
        private readonly Identity.IdentityClient _client;

        public IdentityGrpcClient(Identity.IdentityClient client)
        {
            _client = client;
        }

        public async Task<string?> GetUserIdAsync(string token)
        {
            var response = await _client.ValidateTokenAsync(new ValidateTokenRequest
            {
                Token = token
            });

            return response.IsValid ? response.UserId : null;
        }
    }
}
