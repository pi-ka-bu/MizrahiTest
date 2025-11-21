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
    public partial class MathRequest : IEquatable<MathRequest>
    {
        [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public enum OperationEnum
        {
            [EnumMember(Value = "add")]
            AddEnum = 0,

            [EnumMember(Value = "subtract")]
            SubtractEnum = 1,

            [EnumMember(Value = "multiply")]
            MultiplyEnum = 2,

            [EnumMember(Value = "divide")]
            DivideEnum = 3,
        }

        [Required]
        [DataMember(Name = "operation")]
        public OperationEnum Operation { get; set; }

        [Required]
        [DataMember(Name = "x")]
        public decimal X { get; set; }

        [Required]
        [DataMember(Name = "y")]
        public decimal Y { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class MathRequest {\n");
            sb.Append("  Operation: ").Append(Operation).Append("\n");
            sb.Append("  X: ").Append(X).Append("\n");
            sb.Append("  Y: ").Append(Y).Append("\n");
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
            return obj.GetType() == GetType() && Equals((MathRequest)obj);
        }

        public bool Equals(MathRequest other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;

            return (
                    Operation == other.Operation
                    || Operation != null && Operation.Equals(other.Operation)
                )
                && (X == other.X || X != null && X.Equals(other.X))
                && (Y == other.Y || Y != null && Y.Equals(other.Y));
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                var hashCode = 41;
                // Suitable nullity checks etc, of course :)
                if (Operation != null)
                    hashCode = hashCode * 59 + Operation.GetHashCode();
                if (X != null)
                    hashCode = hashCode * 59 + X.GetHashCode();
                if (Y != null)
                    hashCode = hashCode * 59 + Y.GetHashCode();
                return hashCode;
            }
        }

        #region Operators
#pragma warning disable 1591

        public static bool operator ==(MathRequest left, MathRequest right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(MathRequest left, MathRequest right)
        {
            return !Equals(left, right);
        }

#pragma warning restore 1591
        #endregion Operators
    }
}
