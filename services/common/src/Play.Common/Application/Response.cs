namespace Play.Common.Application
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// An envelope to encapsulate content and request failures
    /// </summary>
    public readonly struct Response
    {
        private Response(string requestId, int statusCode, ResponseContent content, ErrorResponse errorResponse,
            long requestElapsedTime) =>
            (RequestId, StatusCode, Content, ErrorResponse, ElapsedTime) =
            (requestId, statusCode, content, errorResponse, requestElapsedTime);

        /// <summary>
        /// Identification for the Request
        /// </summary>
        public string RequestId { get; }

        /// <summary>
        /// Status Code for the Request
        /// </summary>
        public int StatusCode { get; }

        /// <summary>
        /// Elapsed Time for the Request
        /// </summary>
        public long ElapsedTime { get; }

        /// <summary>
        /// Content for the Request in case a valid response
        /// </summary>
        public ResponseContent Content { get; }

        /// <summary>
        /// ErrorResponse for the Request in case a invalid response
        /// </summary>
        public ErrorResponse ErrorResponse { get; }

        /// <summary>
        /// Create a response with no content and Status Code 200 (OK)
        /// </summary>
        /// <returns></returns>
        public static Response Ok() => Ok(new ResponseContent(), StatusCodes.Status200OK);

        /// <summary>
        /// Valid content for response with Status Code 200 ( OK )
        /// </summary>
        /// <param name="content">Valid content for response</param>
        /// <returns>Returns a valid response with requested content</returns>
        public static Response Ok(ResponseContent content)
        {
            if (content.Equals(default(ResponseContent)))
                throw new ArgumentNullException(nameof(content));

            return new Response(string.Empty, StatusCodes.Status200OK, content, new ErrorResponse(), default);
        }

        /// <summary>
        /// Valid content for response
        /// </summary>
        /// <param name="content">Valid content for response</param>
        /// <param name="statusCode">Status Code for response</param>
        /// <returns>Returns a valid response with requested content</returns>
        public static Response Ok(ResponseContent content, int statusCode)
        {
            if (statusCode < 200 || statusCode > 299)
                throw new ArgumentException(nameof(statusCode));

            return new Response(string.Empty, statusCode, content, new ErrorResponse(), default);
        }

        /// <summary>
        /// Valid content for response
        /// </summary>
        /// <param name="content">Valid content for response</param>
        /// <param name="statusCode">Status Code for response</param>
        /// <param name="requestId">Indentification of request to link the response</param>
        /// <returns>Returns a valid response with requested content</returns>
        public static Response Ok(ResponseContent content, int statusCode, string requestId)
        {
            if (content.Equals(default(ResponseContent)))
                throw new ArgumentNullException(nameof(content));

            if (statusCode < 200 || statusCode > 299)
                throw new ArgumentException(nameof(statusCode));

            if (string.IsNullOrEmpty(requestId))
                throw new ArgumentException(nameof(requestId));

            return new Response(requestId, statusCode, content, new ErrorResponse(), default);
        }

        /// <summary>
        /// Valid content for response
        /// </summary>
        /// <param name="content">Valid content for response</param>
        /// <param name="statusCode">Status Code for response</param>
        /// <param name="requestId">Indentification of request to link the response</param>
        /// <param name="elapsedTime">Elapsed Time for response</param>
        /// <returns>Returns a valid response with requested content</returns>
        public static Response Ok(ResponseContent content, int statusCode, string requestId, long elapsedTime)
        {
            if (content.Equals(default))
                throw new ArgumentNullException(nameof(content));

            return new Response(requestId, statusCode, content, new ErrorResponse(), elapsedTime);
        }

        /// <summary>
        /// Valid content for response with Status Code 200 ( OK )
        /// </summary>
        /// <param name="content">Valid content for response</param>
        /// <param name="requestId">Indentification of request to link the response</param>
        /// <returns>Returns a valid response with requested content</returns>
        public static Response Ok(ResponseContent content, string requestId)
        {
            if (content.Equals(default))
                throw new ArgumentNullException(nameof(content));

            return new Response(requestId, StatusCodes.Status200OK, content, new ErrorResponse(), default);
        }

        /// <summary>
        /// Valid content for response with Status Code 200 ( OK )
        /// </summary>
        /// <param name="content">Valid content for response</param>
        /// <param name="requestId">Indentification of request to link the response</param>
        /// <param name="elapsedTime">Elapsed Time for response</param>
        /// <returns>Returns a valid response with requested content</returns>
        public static Response Ok(ResponseContent content, string requestId, long elapsedTime)
        {
            if (content.Equals(default))
                throw new ArgumentNullException(nameof(content));

            return new Response(requestId, StatusCodes.Status200OK, content, new ErrorResponse(), elapsedTime);
        }

        /// <summary>
        /// Valid content for response with Status Code 200 ( OK )
        /// </summary>
        /// <param name="content">Valid content for response</param>
        /// <param name="elapsedTime">Elapsed Time for response</param>
        /// <returns>Returns a valid response with requested content</returns>
        public static Response Ok(ResponseContent content, long elapsedTime)
        {
            if (content.Equals(default(ResponseContent)))
                throw new ArgumentNullException(nameof(content));

            return new Response(string.Empty, StatusCodes.Status200OK, content, new ErrorResponse(), elapsedTime);
        }

        /// <summary>
        /// Valid content for response
        /// </summary>
        /// <param name="statusCode">Status Code for response</param>
        /// <returns>Returns a valid response with requested content</returns>
        public static Response Ok(int statusCode) =>
            new Response(string.Empty, statusCode, new ResponseContent(), new ErrorResponse(), default);

        /// <summary>
        /// Valid content for response
        /// </summary>
        /// <param name="statusCode">Status Code for response</param>
        /// <param name="requestId">Indentification of request to link the response</param>
        /// <returns>Returns a valid response with requested content</returns>
        public static Response Ok(int statusCode, string requestId) =>
            new Response(requestId, statusCode, new ResponseContent(), new ErrorResponse(), default);

        /// <summary>
        /// Valid content for response
        /// </summary>
        /// <param name="statusCode">Status Code for response</param>
        /// <param name="requestId">Indentification of request to link the response</param>
        /// <param name="elapsedTime">Elapsed Time for response</param>
        /// <returns>Returns a valid response with requested content</returns>
        public static Response Ok(int statusCode, string requestId, long elapsedTime) =>
            new Response(requestId, statusCode, new ResponseContent(), new ErrorResponse(), elapsedTime);

        /// <summary>
        /// Creates a response that contains the Error and Status Code 400 ( Bad Request )
        /// </summary>
        /// <param name="error">Validation Error - ( Error - ErrorResponse )</param>
        /// <returns>Returns a response containing the Validation Error</returns>
        public static Response Fail(Error error)
        {
            if (error.Equals(default(Error)))
                throw new ArgumentNullException(nameof(error));

            return new Response(string.Empty, StatusCodes.Status400BadRequest, new ResponseContent(),
                new ErrorResponse(error), default);
        }

        /// <summary>
        /// Creates a response that contains the Error and Status Code that should be in the range of 4xx
        /// </summary>
        /// <param name="error">Validation Error - ( Error - ErrorResponse )</param>
        /// <param name="statusCode">Status Code that should be in the range of 4xx</param>
        /// <returns>Returns a response containing the Validation Error</returns>
        public static Response Fail(Error error, int statusCode)
        {
            if (statusCode <= 399 || statusCode >= 500)
                throw new ArgumentException(nameof(statusCode));

            if (error.Equals(default(Error)))
                throw new ArgumentNullException(nameof(error));

            return new Response(string.Empty, statusCode, new ResponseContent(), new ErrorResponse(error), default);
        }

        public Response AddError(Error error) => Fail(error);

        /// <summary>
        /// Creates a response that contains the Error and the Request-Id and Status Code that should be in the range of 4xx
        /// </summary>
        /// <param name="error">Validation Error - ( Error - ErrorResponse )</param>
        /// <param name="statusCode">Status Code that should be in the range of 4xx</param>
        /// <param name="requestId">Request identification</param>
        /// <returns>Returns a response containing the Validation Error</returns>
        public static Response Fail(Error error, int statusCode, string requestId)
        {
            if (statusCode <= 399 || statusCode >= 500)
                throw new ArgumentException(nameof(statusCode));

            if (error.Equals(default(Error)))
                throw new ArgumentNullException(nameof(error));

            if (string.IsNullOrEmpty(requestId))
                throw new ArgumentNullException(nameof(error));

            return new Response(requestId, statusCode, new ResponseContent(), new ErrorResponse(error), default);
        }

        /// <summary>
        /// Creates a response that contains the Error and the Request-Id
        /// </summary>
        /// <param name="error">Validation Error - ( Error - ErrorResponse )</param>
        /// <param name="requestId">Request identification</param>
        /// <returns>Returns a response containing the Validation Error</returns>
        public static Response Fail(Error error, string requestId)
        {
            if (string.IsNullOrEmpty(requestId))
                throw new ArgumentNullException(nameof(requestId));

            if (error.Equals(default))
                throw new ArgumentNullException(nameof(error));

            return new Response(requestId, StatusCodes.Status400BadRequest, new ResponseContent(),
                new ErrorResponse(error), default);
        }

        /// <summary>
        /// Checks whether the response contains invalid data
        /// </summary>
        [JsonIgnore]
        public bool IsFailure => !IsSuccess;

        /// <summary>
        /// Checks whether the response contains valid data
        /// </summary>
        [JsonIgnore]
        public bool IsSuccess => VerifyResponseIsSuccess();

        private bool VerifyResponseIsSuccess()
            => EqualityComparer<ErrorResponse>.Default.Equals(ErrorResponse, default) ||
               EqualityComparer<Error>.Default.Equals(ErrorResponse.Error, default);

        public static Response Fail(string errorCode, string errorMessage) => Fail(new Error(errorCode, errorMessage));

        public static Response Fail(string errorCode, string errorMessage, string requestId) =>
            Fail(new Error(errorCode, errorMessage), requestId);

        public bool HasBodyToWrite =>
            (StatusCode == StatusCodes.Status200OK 
             || StatusCode == StatusCodes.Status201Created 
             | StatusCode == StatusCodes.Status202Accepted)
            && Content.ValueAsJsonUtf8Bytes is not null;
    }
}