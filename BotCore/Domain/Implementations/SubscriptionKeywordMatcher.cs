using System.Text.Json;
using System.Text.RegularExpressions;
using BotCore.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nestor;

namespace BotCore.Domain.Implementations;

public class SubscriptionKeywordMatcher(
    IHostEnvironment environment,
    ILogger<SubscriptionKeywordMatcher> logger)
    : ISubscriptionKeywordMatcher
{
    private readonly NestorMorph morph = new();

    private readonly string keywordsDirectory = Path.GetFullPath(
        Path.Combine(
            environment.ContentRootPath,
            "..",
            "..",
            "..",
            "SubscriptionKeywords"));

    public async Task<bool> IsMatchAsync(
        string subscriptionTypeId,
        string text,
        CancellationToken ct)
    {
        
        var path = Path.Combine(
            keywordsDirectory,
            $"{subscriptionTypeId}.json");

        logger.LogInformation(
            "Проверка keywords. SubscriptionTypeId={SubscriptionTypeId}, Path={Path}, Text={Text}",
            subscriptionTypeId,
            path,
            text);

        if (!File.Exists(path))
        {
            logger.LogWarning(
                "Файл keywords не найден: {Path}",
                path);

            return false;
        }

        await using var stream = File.OpenRead(path);

        var config = await JsonSerializer.DeserializeAsync<SubscriptionKeywordsConfig>(
            stream,
            cancellationToken: ct);

        if (config?.Keywords is null || config.Keywords.Count == 0)
            return false;

        var keywordLemmas = new HashSet<string>(
            config.Keywords
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(GetLemma)
                .Where(x => x is not null)
                .Select(x => x!),
            StringComparer.OrdinalIgnoreCase);

        if (keywordLemmas.Count == 0)
            return false;

        var words = Regex
            .Matches(text, @"[\p{L}\p{N}]+")
            .Select(x => x.Value)
            .ToArray();

        foreach (var word in words)
        {
            var lemma = GetLemma(word);

            logger.LogDebug(
                "Word={Word}, Lemma={Lemma}",
                word,
                lemma);

            if (lemma is not null && keywordLemmas.Contains(lemma))
            {
                logger.LogInformation(
                    "Найдено ключевое слово. Word={Word}, Lemma={Lemma}",
                    word,
                    lemma);

                return true;
            }
        }

        logger.LogInformation(
            "Ключевые слова не найдены. SubscriptionTypeId={SubscriptionTypeId}",
            subscriptionTypeId);

        return false;
    }

    private string? GetLemma(string word)
    {
        word = word.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(word))
            return null;

        var words = morph.WordInfo(word);

        if (words.Length == 0)
            return null;

        return words[0].Lemma.Word.ToLowerInvariant();
    }
}