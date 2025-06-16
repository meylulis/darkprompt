using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;
using UnityEngine;
using System.Linq;

public class SafetyDomainTester : MonoBehaviour
{
    //тут добавляем фишинговые ссылки
    private List<string> UnsafeDomains = new List<string>
    {
        "shady-site.com",
        "malicious-domain.net",
        "phishing-page.org",
        "example-scam.com",
        "http://facebok-login.com", "https://appleid.verify-apple.com", "http://microsoft-security-update.com", "https://netflix-premium.gift", "http://paypall-confirm.com", "https://google-secure-login.xyz", "http://amaz0n-prime.com", "https://steamcommunity.ru", "http://instagram-help-center.com", "https://whatsapp-web.download", "http://linkedin-verify-profile.net", "https://twitter-account-recovery.com", "http://dropbox-file-share.xyz", "https://discord-nitro-free.gift", "http://ebay-item-confirm.com", "https://spotify-premium-unlock.com", "http://tiktok-verify-account.net", "https://binance-wallet-secure.com", "http://roblox-free-robux-generator.com", "https://adobe-photoshop-free-download.net",
        "https://www.facebook.com", "https://appleid.apple.com", "https://www.microsoft.com", "https://www.netflix.com", "https://www.paypal.com", "https://accounts.google.com", "https://www.amazon.com", "https://steamcommunity.com", "https://www.instagram.com", "https://web.whatsapp.com", "https://www.linkedin.com", "https://twitter.com", "https://www.dropbox.com", "https://discord.com", "https://www.ebay.com", "https://www.spotify.com", "https://www.tiktok.com", "https://www.binance.com", "https://www.roblox.com", "https://www.adobe.com", "https://www.youtube.com", "https://www.reddit.com", "https://github.com", "https://www.twitch.tv", "https://www.wikipedia.org", "https://www.office.com", "https://outlook.live.com", "https://www.cloudflare.com", "https://www.nginx.com", "https://unity.com"
    };


    private static readonly Regex DomainExtractor = new Regex(
        @"^(https?:\/\/)?(www\.)?([^\/\?:]+)(\/|\?|:|$)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);
    public bool IsPhishingLink(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            Debug.LogWarning("Пустая ссылка");
            return false;
        }

        try
        {
            // Извлекаем домен из URL
            string domain = ExtractDomain(url);
            if (string.IsNullOrEmpty(domain))
            {
                Debug.LogWarning($"Не удалось извлечь домен из URL: {url}");
                return false;
            }

            // Проверяем домен в списке фишинговых
            if (UnsafeDomains.Contains(domain))
            {
                Debug.LogWarning($"Обнаружен фишинговый домен: {domain}");
                return true;
            }

            // Проверяем на подозрительные признаки
            if (HasPhishingCharacteristics(url, domain))
            {
                Debug.LogWarning($"Подозрительная ссылка: {url}");
                return true;
            }

            return false;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Ошибка при проверке URL {url}: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// Извлекает домен из URL
    /// </summary>
    private string ExtractDomain(string url)
    {
        Match match = DomainExtractor.Match(url);
        if (match.Success && match.Groups.Count >= 4)
        {
            string domain = match.Groups[3].Value.ToLower();

            // Удаляем порт если есть
            int portIndex = domain.IndexOf(':');
            if (portIndex > 0)
            {
                domain = domain.Substring(0, portIndex);
            }

            return domain;
        }
        return null;
    }

    /// <summary>
    /// Проверяет URL на фишинговые характеристики
    /// </summary>
    private bool HasPhishingCharacteristics(string url, string domain)
    {
        // Проверка на подмену популярных доменов (typosquatting)
        string[] popularDomains = { "facebook", "google", "amazon", "microsoft", "paypal" };
        foreach (string popular in popularDomains)
        {
            if (domain.Contains(popular) && !domain.EndsWith(popular + ".com"))
            {
                return true;
            }
        }

        // Проверка на использование подозрительных TLD
        string[] suspiciousTlds = { ".gift", ".xyz", ".ru", ".net", ".download" };
        foreach (string tld in suspiciousTlds)
        {
            if (domain.EndsWith(tld))
            {
                return true;
            }
        }

        // Проверка на ключевые слова фишинга
        string[] phishingKeywords = { "login", "verify", "secure", "account", "confirm", "recovery" };
        foreach (string keyword in phishingKeywords)
        {
            if (domain.Contains(keyword))
            {
                return true;
            }
        }

        return false;
    }
    public List<string> GetRandomUrls(int count)
    {
        List<string> urls = new List<string>();
        for (int i = 0; i < count; i++)
        {
            var freeUrls = UnsafeDomains.Except(urls);
            urls.Add(freeUrls.ElementAt(UnityEngine.Random.Range(0, freeUrls.Count())));
        }
        return urls;
    }
}
