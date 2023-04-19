namespace Play.Customer.Core.Domain.AggregateModel.CustomerAggregate
{
    using System;
    using System.Globalization;
    using System.Text.RegularExpressions;

    public readonly struct Email
    {
        public Email(string value)
        {
            // if (!IsValidEmail(value))
            //     throw new Exception();
            
            Value = value;
        }
        
        public string Value { get; }
        
        private static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                email = Regex.Replace(email, @"(@)(.+)$", DomainMapper,
                    RegexOptions.None, TimeSpan.FromMilliseconds(200));
                
                string DomainMapper(Match match)
                {
                    var idn = new IdnMapping();
                    
                    var domainName = idn.GetAscii(match.Groups[2].Value);
                    return match.Groups[1].Value + domainName;
                }
                
            }
            catch (RegexMatchTimeoutException e)
            {
                return false;
            }
            catch (ArgumentException e)
            {
                return false;
            }

            try
            {
                return Regex.IsMatch(email,
                    @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                    RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }
    }
}