using System.ClientModel;
using System.Reflection;
using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Options;
using OpenAI.Chat;
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

namespace YetAnotherTranslator.Infrastructure.Azure.AiFoundry;

internal class AzureAiFoundryProvider
    : ReviewGrammarLargeLanguageModelProvider,
        TranslateTextLargeLanguageModelProvider,
        TranslateWordLargeLanguageModelProvider
{
    private readonly AzureAiFoundryOptions _azureAiFoundryOptions;
    private readonly IEmbeddedFile _embeddedFile;
    private readonly ISerializer _serializer;

    public AzureAiFoundryProvider(
        IOptions<AzureAiFoundryOptions> azureAiFoundryOptions,
        ISerializer serializer,
        IEmbeddedFile embeddedFile
    )
    {
        _serializer = serializer;
        _embeddedFile = embeddedFile;
        _azureAiFoundryOptions = azureAiFoundryOptions.Value;
    }

    public async Task<GrammarReviewResult?> ReviewGrammarAsync(
        string text,
        CancellationToken cancellationToken = default
    )
    {
        ChatClient chatClient = BuildChatClient();
        string systemPrompt = GetContent("ReviewGrammar/system_prompt.md");
        string userPrompt = GetContent("ReviewGrammar/user_prompt.md").Replace("{text}", text);
        List<ChatMessage> messages =
        [
            new SystemChatMessage(systemPrompt),
            new UserChatMessage(userPrompt)
        ];
        ClientResult<ChatCompletion>? clientResult =
            await chatClient.CompleteChatAsync(messages, new ChatCompletionOptions(), cancellationToken);
        GrammarReviewResult? grammarReviewResult =
            _serializer.Deserialize<GrammarReviewResult>(clientResult.Value.Content[0].Text);

        return grammarReviewResult;
    }

    public async Task<string> TranslateTextAsync(
        string text,
        string? sourceLanguage,
        string? targetLanguage,
        CancellationToken cancellationToken = default
    )
    {
        ChatClient chatClient = BuildChatClient();
        string systemPrompt = GetContent("TranslateText/system_prompt.md");
        string userPrompt = "";
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

        List<ChatMessage> messages =
        [
            new SystemChatMessage(systemPrompt),
            new UserChatMessage(userPrompt)
        ];
        ClientResult<ChatCompletion>? clientResult =
            await chatClient.CompleteChatAsync(messages, new ChatCompletionOptions(), cancellationToken);

        return clientResult.Value.Content[0].Text;
    }

    public async Task<TranslationResult?> TranslateWordAsync(
        string word,
        string? sourceLanguage,
        string? targetLanguage,
        CancellationToken cancellationToken = default
    )
    {
        ChatClient chatClient = BuildChatClient();
        string systemPrompt = GetContent("TranslateWord/system_prompt.md");
        string userPrompt = "";
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

        List<ChatMessage> messages =
        [
            new SystemChatMessage(systemPrompt),
            new UserChatMessage(userPrompt)
        ];
        ClientResult<ChatCompletion>? clientResult =
            await chatClient.CompleteChatAsync(messages, new ChatCompletionOptions(), cancellationToken);
        TranslationResult? translations =
            _serializer.Deserialize<TranslationResult>(clientResult.Value.Content[0].Text);

        return translations;
    }

    private ChatClient BuildChatClient()
    {
        AzureOpenAIClient azureClient = new(
            new Uri(_azureAiFoundryOptions.Endpoint),
            new AzureKeyCredential(_azureAiFoundryOptions.ApiKey)
        );
        ChatClient chatClient = azureClient.GetChatClient(_azureAiFoundryOptions.DeploymentName);

        return chatClient;
    }

    private string GetContent(string path)
    {
        return _embeddedFile.GetContent($"Azure/AiFoundry/Prompts/{path}", Assembly.GetExecutingAssembly());
    }
}
