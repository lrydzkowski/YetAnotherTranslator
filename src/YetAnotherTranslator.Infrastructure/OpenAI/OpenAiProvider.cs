#pragma warning disable OPENAI001

using System.ClientModel;
using System.Reflection;
using Microsoft.Extensions.Options;
using OpenAI.Responses;
using YetAnotherTranslator.Core.Common.Extensions;
using YetAnotherTranslator.Core.Common.Services;
using YetAnotherTranslator.Core.Handlers.ReviewGrammar.Models;
using YetAnotherTranslator.Core.Handlers.TranslateWord.Models;
using ReviewGrammarLargeLanguageModelProvider =
    YetAnotherTranslator.Core.Handlers.ReviewGrammar.Interfaces.ILargeLanguageModelProvider;
using TranslateTextLargeLanguageModelProvider =
    YetAnotherTranslator.Core.Handlers.TranslateText.Interfaces.ILargeLanguageModelProvider;
using TranslateWordLargeLanguageModelProvider =
    YetAnotherTranslator.Core.Handlers.TranslateWord.Interfaces.ILargeLanguageModelProvider;

namespace YetAnotherTranslator.Infrastructure.OpenAI;

internal class OpenAiProvider
    : ReviewGrammarLargeLanguageModelProvider,
        TranslateTextLargeLanguageModelProvider,
        TranslateWordLargeLanguageModelProvider
{
    private readonly IEmbeddedFile _embeddedFile;
    private readonly OpenAiOptions _openAiOptions;
    private readonly ISerializer _serializer;

    public OpenAiProvider(
        IOptions<OpenAiOptions> openAiOptions,
        ISerializer serializer,
        IEmbeddedFile embeddedFile
    )
    {
        _serializer = serializer;
        _embeddedFile = embeddedFile;
        _openAiOptions = openAiOptions.Value;
    }

    public async Task<GrammarReviewResult?> ReviewGrammarAsync(
        string text,
        CancellationToken cancellationToken = default
    )
    {
        ResponsesClient client = BuildResponseClient();
        string systemPrompt = GetContent("ReviewGrammar/system_prompt.md");
        string userPrompt = GetContent("ReviewGrammar/user_prompt.md").Replace("{text}", text);
        List<ResponseItem> inputItems =
        [
            ResponseItem.CreateDeveloperMessageItem(systemPrompt),
            ResponseItem.CreateUserMessageItem(userPrompt)
        ];
        CreateResponseOptions options = new(inputItems);
        ResponseResult response = await client.CreateResponseAsync(options, cancellationToken);
        GrammarReviewResult? grammarReviewResult =
            _serializer.Deserialize<GrammarReviewResult>(response.GetOutputText());

        return grammarReviewResult;
    }

    public async Task<string> TranslateTextAsync(
        string text,
        string? sourceLanguage,
        string? targetLanguage,
        CancellationToken cancellationToken = default
    )
    {
        ResponsesClient client = BuildResponseClient();
        string systemPrompt = GetContent("TranslateText/system_prompt.md");
        string userPrompt;
        if (sourceLanguage is null || targetLanguage is null)
        {
            userPrompt = GetContent("TranslateText/user_prompt_auto_detection.md")
                .Replace("{text}", text);
        }
        else
        {
            userPrompt = GetContent("TranslateText/user_prompt.md")
                .Replace("{sourceLanguage}", sourceLanguage)
                .Replace("{targetLanguage}", targetLanguage)
                .Replace("{text}", text);
        }

        List<ResponseItem> inputItems =
        [
            ResponseItem.CreateDeveloperMessageItem(systemPrompt),
            ResponseItem.CreateUserMessageItem(userPrompt)
        ];
        CreateResponseOptions options = new(inputItems);
        ResponseResult response = await client.CreateResponseAsync(options, cancellationToken);

        return response.GetOutputText();
    }

    public async Task<TranslationResult?> TranslateWordAsync(
        string word,
        string? sourceLanguage,
        string? targetLanguage,
        CancellationToken cancellationToken = default
    )
    {
        ResponsesClient client = BuildResponseClient();
        string systemPrompt = GetContent("TranslateWord/system_prompt.md");
        string userPrompt;
        if (sourceLanguage is null || targetLanguage is null)
        {
            userPrompt = GetContent("TranslateWord/user_prompt_auto_detection.md").Replace("{word}", word);
        }
        else if (sourceLanguage.EqualsIgnoreCase("Polish"))
        {
            userPrompt = GetContent("TranslateWord/user_prompt_polish_word.md").Replace("{word}", word);
        }
        else
        {
            userPrompt = GetContent("TranslateWord/user_prompt_english_word.md").Replace("{word}", word);
        }

        List<ResponseItem> inputItems =
        [
            ResponseItem.CreateDeveloperMessageItem(systemPrompt),
            ResponseItem.CreateUserMessageItem(userPrompt)
        ];
        CreateResponseOptions options = new(inputItems);
        ResponseResult response = await client.CreateResponseAsync(options, cancellationToken);
        TranslationResult? translations = _serializer.Deserialize<TranslationResult>(response.GetOutputText());

        return translations;
    }

    private ResponsesClient BuildResponseClient()
    {
        return new ResponsesClient(
            _openAiOptions.ModelName,
            new ApiKeyCredential(_openAiOptions.ApiKey)
        );
    }

    private string GetContent(string path)
    {
        return _embeddedFile.GetContent($"Azure/AiFoundry/Prompts/{path}", Assembly.GetExecutingAssembly());
    }
}
