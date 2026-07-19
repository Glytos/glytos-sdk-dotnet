using System;

namespace Glytos
{
    /// <summary>
    /// Thrown on any non-2xx API response, and on transport failures (with
    /// <see cref="Status"/> set to <c>0</c>).
    /// </summary>
    public sealed class GlytosException : Exception
    {
        /// <summary>HTTP status code of the response, or <c>0</c> for a transport failure.</summary>
        public int Status { get; }

        /// <summary>The API's machine-readable error code (e.g. <c>not_found</c>).</summary>
        public string ErrorCode { get; }

        /// <summary>The <c>X-Request-Id</c> of the response, when present, for support.</summary>
        public string? RequestId { get; }

        /// <summary>Creates a new <see cref="GlytosException"/>.</summary>
        public GlytosException(
            int status,
            string errorCode,
            string message,
            string? requestId = null,
            Exception? innerException = null)
            : base(message, innerException)
        {
            Status = status;
            ErrorCode = errorCode;
            RequestId = requestId;
        }
    }
}
