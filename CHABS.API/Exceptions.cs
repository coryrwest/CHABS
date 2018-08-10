using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace CHABS.API {
    public class NoResultsFoundException : Exception {
        public NoResultsFoundException(Type type, string field, string value)
            : base(string.Format("No result found for {0} with {1} of {2}.", type.Name, field, value)) {
        }

        public NoResultsFoundException(Type type, string whereClause)
            : base(string.Format("No result found for {0} with where clause: {1}.", type.Name, whereClause)) {
        }
    }

    public class PermissionsException : Exception {
        public PermissionsException()
            : base("You do not have permission to perform that action.") {
        }
    }

    public class BankServiceException : Exception {
        public string ErrorMessage { get; set; }
        public string ErrorType { get; set; }
        public string ErrorCode { get; set; }

        public BankServiceException(string bankServiceResponse) : base(
            $"An error response was returned by the bank service. Content: {bankServiceResponse}") {
            var obj = JsonConvert.DeserializeObject<Dictionary<string, string>>(bankServiceResponse);
            if (obj.ContainsKey("error_code")) {
                ErrorCode = obj["error_code"];
            }
            if (obj.ContainsKey("error_message")) {
                ErrorCode = obj["error_message"];
            }
            if (obj.ContainsKey("error_type")) {
                ErrorCode = obj["error_type"];
            }
        }
    }
}
