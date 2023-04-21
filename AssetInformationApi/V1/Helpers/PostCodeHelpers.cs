using System.Text.RegularExpressions;

namespace AssetInformationApi.V1.Helpers
{
    public static class PostCodeHelpers
    {
        public static string NormalizePostcode(string postcode)
        {
            //removes space in middle spaces
            postcode = postcode.Replace(" ", "");

            postcode = postcode.ToUpper();

            //adds middle spaces based on postcode character count
            return postcode.Length switch
            {
                5 => postcode.Insert(2, " "),
                6 => postcode.Insert(3, " "),
                7 => postcode.Insert(4, " "),
                _ => postcode,
            };
        }

        public static bool IsValidPostCode(string postcode)
        {
            if (postcode == null) return false;

            var trimmed = postcode.Replace(" ", "");

            if (trimmed.Length > 7) return false;

            const string pattern = @"^[A-Z]{1,2}[0-9]{1,2} ?[0-9][A-Z]{2}$";
            RegexOptions options = RegexOptions.IgnoreCase;

            return Regex.Match(trimmed, pattern, options).Success;
        }
    }
}
