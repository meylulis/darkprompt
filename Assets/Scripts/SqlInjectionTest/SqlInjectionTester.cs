using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SQLInjectionTester : MonoBehaviour
{
    private List<string> queries = new List<string>
    {
        // Уязвимые
        "SELECT * FROM users WHERE username = '" + "\" + input + \"" + "'",
        "SELECT * FROM users WHERE name = '" + "\" + input + \"" + "' AND password = '" + "\" + pass + \"" + "'",
        // Безопасный
        "SELECT * FROM users WHERE username = @username"
    };

    public bool IsQueryUnsafe(string query)
    {
        return query.Contains("+ input +") || query.Contains("+ pass +");
    }

    public List<string> GetRandomQueries(int count)
    {
        return queries.OrderBy(q => UnityEngine.Random.value).Take(count).ToList();
    }
}
