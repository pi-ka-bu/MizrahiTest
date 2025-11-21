using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace TestAPI.Models
{
    [DataContract]
    public partial class ErrorResponse : IEquatable<ErrorResponse>
    {
        [DataMember(Name = "code")]
        public int? Code { get; set; }

        [DataMember(Name = "message")]
        public string Message { get; set; }

        [DataMember(Name = "details")]
        public List<string> Details { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class ErrorResponse {\n");
            sb.Append("  Code: ").Append(Code).Append("\n");
            sb.Append("  Message: ").Append(Message).Append("\n");
            sb.Append("  Details: ").Append(Details).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            return obj.GetType() == GetType() && Equals((ErrorResponse)obj);
        }

        public bool Equals(ErrorResponse other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;

            return (Code == other.Code || Code != null && Code.Equals(other.Code))
                && (Message == other.Message || Message != null && Message.Equals(other.Message))
                && (
                    Details == other.Details
                    || Details != null && Details.SequenceEqual(other.Details)
                );
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = 41;
                if (Code != null)
                    hashCode = hashCode * 59 + Code.GetHashCode();
                if (Message != null)
                    hashCode = hashCode * 59 + Message.GetHashCode();
                if (Details != null)
                    hashCode = hashCode * 59 + Details.GetHashCode();
                return hashCode;
            }
        }

        #region Operators
#pragma warning disable 1591

        public static bool operator ==(ErrorResponse left, ErrorResponse right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ErrorResponse left, ErrorResponse right)
        {
            return !Equals(left, right);
        }

#pragma warning restore 1591
        #endregion Operators
    }
}
