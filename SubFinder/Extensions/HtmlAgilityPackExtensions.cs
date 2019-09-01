using HtmlAgilityPack;
namespace SubFinder.Extensions
{
    public static class HtmlAgilityPackExtensions
    {
        private const string CharacterDataStart = "<![CDATA[";
        private const string CharacterDataEnd = "]]>";

        public static string CharacterData(this HtmlNode node)
        {
            if (node != null && node.InnerLength > CharacterDataStart.Length + CharacterDataEnd.Length)
            {
                return node.InnerHtml
                    .Replace(CharacterDataStart, string.Empty)
                    .Replace(CharacterDataEnd, string.Empty);
            }

            return string.Empty;
        }
    }
}
