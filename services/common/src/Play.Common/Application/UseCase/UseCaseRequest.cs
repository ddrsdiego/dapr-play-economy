namespace Play.Common.Application.UseCase
{
    using System;

    public abstract class UseCaseRequest
    {
        protected UseCaseRequest()
            : this(Guid.NewGuid().ToString().Split('-')[0])
        {
        }

        protected UseCaseRequest(string requestId)
        {
            RequestAt = DateTime.UtcNow;
            RequestId = requestId;
        }

        public string RequestId { get; protected set; }

        public virtual UseCaseRequest SetRequestId(string requestId)
        {
            RequestId = requestId;
            return this;
        }
        
        public DateTime RequestAt { get; }
    }
}