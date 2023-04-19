namespace Play.Common.Application
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json.Serialization;
    using Microsoft.AspNetCore.Http;

    public readonly struct Error
    {
        public Error(string code, string message, int statusCode = StatusCodes.Status400BadRequest)
        {
            Code = code ?? throw new ArgumentNullException(nameof(code));
            Message = message ?? throw new ArgumentNullException(nameof(message));
            StatusCode = statusCode;
            Details = new List<Error>();
        }

        public string Code { get; }
        public string Message { get; }
        [JsonIgnore] public int StatusCode { get; }

        public Error AddErrorDetail(Error error)
        {
            if (error.Code.Equals(Code, StringComparison.InvariantCultureIgnoreCase))
                throw new ArgumentNullException(nameof(error.Code));

            if (Details.Any(x => x.Code.Equals(error.Code, StringComparison.InvariantCultureIgnoreCase)))
                return error;

            Details.Add(error);

            return this;
        }

        public List<Error> Details { get; }
    }
}