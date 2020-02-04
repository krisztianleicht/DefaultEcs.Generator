using System;
using System.Collections.Generic;
using System.Text;

namespace DefaultEcs.Generator.Generators
{
    public static class Extensions
    {

        public static bool HasComponent(this Type type, Type componentType)
        {
            var attributes = type.GetCustomAttributes(componentType, false);
            return attributes.Length != 0;
        }

        public static string ReplaceClassInformation(this string input, Type type)
        {
            var name = type.Name.Replace("Component", "");
            var fullName = type.FullName;
            var fullNameEscaped = fullName.ToLowerFirstChar().ToCSharpIdentifier();

            return input
                     .Replace("*ClassName*", name)
                     .Replace("*classEscapedFullName*", fullNameEscaped)
                     .Replace("*ClassFullName*", fullName);
        }

        public static string ToCSharpIdentifier(this string input)
        {
            StringBuilder sb = new StringBuilder(input.Length);

            // Need to check if the first character is number
            if (input.Length > 0 && input[0] >= 48 && input[0] <= 57)
            {
                sb.Append("_"); // need to start with and underscore
            }

            for (int index = 0, length = input.Length; index < length; index++)
            {
                var c = input[index];
                if (
                    (c >= 48 && c <= 57) || // numbers
                    (c >= 65 && c <= 90) || // upper case characters
                    (c >= 97 && c <= 122) // lower case characters
                    )
                {
                    sb.Append(c);
                }
                //else
                //{
                //    sb.Append('');
                //}
            }

            return sb.ToString();
        }

        public static string ToLowerFirstChar(this string value)
        {
            return char.ToLowerInvariant(value[0]) + value.Substring(1);
        }

        public static string ToUpperFirstChar(this string value)
        {
            return char.ToUpperInvariant(value[0]) + value.Substring(1);
        }

    }
}
