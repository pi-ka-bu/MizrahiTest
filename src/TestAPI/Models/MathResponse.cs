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
    public partial class MathResponse : IEquatable<MathResponse>
    {
        [Required]
        [DataMember(Name = "requestId")]
        public string RequestId { get; set; }

        [DataMember(Name = "operation")]
        public string Operation { get; set; }

        [DataMember(Name = "x")]
        public decimal? X { get; set; }

        [DataMember(Name = "y")]
        public decimal? Y { get; set; }

        [Required]
        [DataMember(Name = "result")]
        public decimal Result { get; set; }

        [DataMember(Name = "fromCache")]
        public bool? FromCache { get; set; }

        [Required]
        [DataMember(Name = "timestamp")]
        public DateTime Timestamp { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class MathResponse {\n");
            sb.Append("  RequestId: ").Append(RequestId).Append("\n");
            sb.Append("  Operation: ").Append(Operation).Append("\n");
            sb.Append("  X: ").Append(X).Append("\n");
            sb.Append("  Y: ").Append(Y).Append("\n");
            sb.Append("  Result: ").Append(Result).Append("\n");
            sb.Append("  FromCache: ").Append(FromCache).Append("\n");
            sb.Append("  Timestamp: ").Append(Timestamp).Append("\n");
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
            return obj.GetType() == GetType() && Equals((MathResponse)obj);
        }

        public bool Equals(MathResponse other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;

            return (
                    RequestId == other.RequestId
                    || RequestId != null && RequestId.Equals(other.RequestId)
                )
                && (
                    Operation == other.Operation
                    || Operation != null && Operation.Equals(other.Operation)
                )
                && (X == other.X || X != null && X.Equals(other.X))
                && (Y == other.Y || Y != null && Y.Equals(other.Y))
                && (Result == other.Result || Result != null && Result.Equals(other.Result))
                && (
                    FromCache == other.FromCache
                    || FromCache != null && FromCache.Equals(other.FromCache)
                )
                && (
                    Timestamp == other.Timestamp
                    || Timestamp != null && Timestamp.Equals(other.Timestamp)
                );
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                var hashCode = 41;
                // Suitable nullity checks etc, of course :)
                if (RequestId != null)
                    hashCode = hashCode * 59 + RequestId.GetHashCode();
                if (Operation != null)
                    hashCode = hashCode * 59 + Operation.GetHashCode();
                if (X != null)
                    hashCode = hashCode * 59 + X.GetHashCode();
                if (Y != null)
                    hashCode = hashCode * 59 + Y.GetHashCode();
                if (Result != null)
                    hashCode = hashCode * 59 + Result.GetHashCode();
                if (FromCache != null)
                    hashCode = hashCode * 59 + FromCache.GetHashCode();
                if (Timestamp != null)
                    hashCode = hashCode * 59 + Timestamp.GetHashCode();
                return hashCode;
            }
        }

        #region Operators
#pragma warning disable 1591

        public static bool operator ==(MathResponse left, MathResponse right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(MathResponse left, MathResponse right)
        {
            return !Equals(left, right);
        }

#pragma warning restore 1591
        #endregion Operators
    }
}
