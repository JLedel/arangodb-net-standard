﻿using ArangoDBNetStandard.AuthApi.Models;
using ArangoDBNetStandard.Serialization;
using ArangoDBNetStandard.Transport;
using System.Threading.Tasks;

namespace ArangoDBNetStandard.AuthApi
{
    /// <summary>
    /// ArangoDB authentication endpoints.
    /// </summary>
    public class AuthApiClient : ApiClientBase, IAuthApiClient
    {
        /// <summary>
        /// The transport client used to communicate with the ArangoDB host.
        /// </summary>
        protected IApiClientTransport _client;

        /// <summary>
        /// Create an instance of <see cref="AuthApiClient"/>
        /// using the provided transport layer and the default JSON serialization.
        /// </summary>
        /// <param name="client"></param>
        public AuthApiClient(IApiClientTransport client)
            : base(new JsonNetApiClientSerialization())
        {
            _client = client;
        }

        /// <summary>
        /// Creates an instance of <see cref="AuthApiClient"/>
        /// using the provided transport and serialization layers.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="serializer"></param>
        public AuthApiClient(IApiClientTransport client, IApiClientSerialization serializer)
            : base(serializer)
        {
            _client = client;
        }

        /// <summary>
        /// Gets a JSON Web Token generated by the ArangoDB server.
        /// </summary>
        /// <param name="username">The username of the user for whom to generate a JWT token.</param>
        /// <param name="password">The user's password.</param>
        /// <returns>Object containing the encoded JWT token value.</returns>
        public virtual async Task<JwtTokenResponse> GetJwtTokenAsync(string username, string password)
        {
            return await GetJwtTokenAsync(new JwtTokenRequestBody
            {
                Username = username,
                Password = password
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets a JSON Web Token generated by the ArangoDB server.
        /// </summary>
        /// <param name="body">Object containing username and password.</param>
        /// <returns>Object containing the encoded JWT token value.</returns>
        public virtual async Task<JwtTokenResponse> GetJwtTokenAsync(
            JwtTokenRequestBody body)
        {
            byte[] content = await GetContentAsync(body, new ApiClientSerializationOptions(true, false)).ConfigureAwait(false);
            using (var response = await _client.PostAsync("/_open/auth", content).ConfigureAwait(false))
            {
                if (response.IsSuccessStatusCode)
                {
                    var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                    return await DeserializeJsonFromStreamAsync<JwtTokenResponse>(stream).ConfigureAwait(false);
                }
                throw await GetApiErrorExceptionAsync(response).ConfigureAwait(false);
            }
        }
    }
}
