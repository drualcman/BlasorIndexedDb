namespace BlazorIndexedDb.Helpers
{
    /// <summary>
    /// Internal utilities
    /// </summary>
    internal sealed class Utils
    {
        internal static CommandResponse CommandResponse(ResponseJsDb response) =>
            new(response.Result, response.Message, new List<ResponseJsDb> { response });

        internal static CommandResponse CommandResponse(List<ResponseJsDb> response)
        {
            bool allGood = true;
            int c = response.Count;
            int i = 0;

            while (i < c && allGood)
            {
                allGood = response[i].Result;
                i++;
            }

            return new(allGood, allGood ? "All transactions finished with true" : "Some transaction can't be finished", response);
        }

        /// <summary>
        /// Get only the name from the type passed if it's from a generic type
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        internal static string GetGenericTypeName(Type t)
        {
            if(!t.IsGenericType)
                return t.FullName;

            string genericArgs = string.Join(".",
                t.GetGenericArguments()
                    .Select(ta => GetGenericTypeName(ta)).ToArray());
            string result;
            string[] nom = genericArgs.Split(".", StringSplitOptions.RemoveEmptyEntries);
            if(nom.Length > 0) result = nom[nom.Length - 1];
            else result = genericArgs;
            return result;
        }
    }
}
