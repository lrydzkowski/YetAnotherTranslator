using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using YetAnotherTranslator.Core.Handlers.TranslateWord;
using YetAnotherTranslator.Tests.Integration.Infrastructure;

namespace YetAnotherTranslator.Tests.Integration.Features;

public class TranslateWordTests : TestBase
{
    private TranslateWordHandler _handler = null!;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        var validator = new TranslateWordValidator();
        var llmProvider = new TestLlmProvider(WireMockServer.Url!);
        var historyRepository = ServiceProvider.GetRequiredService<Core.Interfaces.IHistoryRepository>();

        _handler = new TranslateWordHandler(llmProvider, validator, historyRepository);
    }

    [Fact]
    public async Task TranslateWord_PolishToEnglish_ReturnsCmuArpabet()
    {
        string mockResponse = @"{
  ""translations"": [
    {
      ""rank"": 1,
      ""word"": ""cat"",
      ""partOfSpeech"": ""noun"",
      ""countability"": ""countable"",
      ""cmuArpabet"": ""K AE1 T"",
      ""examples"": [""The cat sat on the mat.""]
    }
  ]
}";

        WireMockServer.Given(
            Request.Create()
                .WithPath("/v1/messages")
                .UsingPost()
        )
        .RespondWith(
            Response.Create()
                .WithStatusCode(200)
                .WithBody($@"{{""content"":[{{""type"":""text"",""text"":{System.Text.Json.JsonSerializer.Serialize(mockResponse)}}}]}}")
        );

        var request = new TranslateWordRequest("kot", "Polish", "English", UseCache: false);

        TranslationResult result = await _handler.HandleAsync(request);

        result.Should().NotBeNull();
        result.SourceLanguage.Should().Be("Polish");
        result.TargetLanguage.Should().Be("English");
        result.InputText.Should().Be("kot");
        result.Translations.Should().HaveCount(1);

        Translation translation = result.Translations[0];
        translation.Rank.Should().Be(1);
        translation.Word.Should().Be("cat");
        translation.PartOfSpeech.Should().Be("noun");
        translation.Countability.Should().Be("countable");
        translation.CmuArpabet.Should().Be("K AE1 T");
        translation.Examples.Should().ContainSingle().Which.Should().Be("The cat sat on the mat.");
    }

    [Fact]
    public async Task TranslateWord_EnglishToPolish_NoCmuArpabet()
    {
        string mockResponse = @"{
  ""translations"": [
    {
      ""rank"": 1,
      ""word"": ""kot"",
      ""partOfSpeech"": ""rzeczownik"",
      ""countability"": ""policzalny"",
      ""examples"": [""Kot siedział na macie.""]
    }
  ]
}";

        WireMockServer.Given(
            Request.Create()
                .WithPath("/v1/messages")
                .UsingPost()
        )
        .RespondWith(
            Response.Create()
                .WithStatusCode(200)
                .WithBody($@"{{""content"":[{{""type"":""text"",""text"":{System.Text.Json.JsonSerializer.Serialize(mockResponse)}}}]}}")
        );

        var request = new TranslateWordRequest("cat", "English", "Polish", UseCache: false);

        TranslationResult result = await _handler.HandleAsync(request);

        result.Should().NotBeNull();
        result.Translations.Should().HaveCount(1);

        Translation translation = result.Translations[0];
        translation.Word.Should().Be("kot");
        translation.CmuArpabet.Should().BeNull();
    }

    [Fact]
    public async Task TranslateWord_MultipleTranslations_RankedByPopularity()
    {
        string mockResponse = @"{
  ""translations"": [
    {
      ""rank"": 1,
      ""word"": ""castle"",
      ""partOfSpeech"": ""noun"",
      ""countability"": ""countable"",
      ""cmuArpabet"": ""K AE1 S AH0 L"",
      ""examples"": [""The castle stood on the hill.""]
    },
    {
      ""rank"": 2,
      ""word"": ""lock"",
      ""partOfSpeech"": ""noun"",
      ""countability"": ""countable"",
      ""cmuArpabet"": ""L AA1 K"",
      ""examples"": [""Turn the lock to open the door.""]
    },
    {
      ""rank"": 3,
      ""word"": ""zipper"",
      ""partOfSpeech"": ""noun"",
      ""countability"": ""countable"",
      ""cmuArpabet"": ""Z IH1 P ER0"",
      ""examples"": [""The zipper on my jacket is broken.""]
    }
  ]
}";

        WireMockServer.Given(
            Request.Create()
                .WithPath("/v1/messages")
                .UsingPost()
        )
        .RespondWith(
            Response.Create()
                .WithStatusCode(200)
                .WithBody($@"{{""content"":[{{""type"":""text"",""text"":{System.Text.Json.JsonSerializer.Serialize(mockResponse)}}}]}}")
        );

        var request = new TranslateWordRequest("zamek", "Polish", "English", UseCache: false);

        TranslationResult result = await _handler.HandleAsync(request);

        result.Translations.Should().HaveCount(3);
        result.Translations[0].Rank.Should().Be(1);
        result.Translations[0].Word.Should().Be("castle");
        result.Translations[1].Rank.Should().Be(2);
        result.Translations[1].Word.Should().Be("lock");
        result.Translations[2].Rank.Should().Be(3);
        result.Translations[2].Word.Should().Be("zipper");
    }

    [Fact]
    public async Task TranslateWord_PronunciationVariants_DifferentArpabetByPartOfSpeech()
    {
        string mockResponse = @"{
  ""translations"": [
    {
      ""rank"": 1,
      ""word"": ""record"",
      ""partOfSpeech"": ""noun"",
      ""countability"": ""countable"",
      ""cmuArpabet"": ""R EH1 K ER0 D"",
      ""examples"": [""I bought a vinyl record.""]
    },
    {
      ""rank"": 2,
      ""word"": ""record"",
      ""partOfSpeech"": ""verb"",
      ""countability"": ""N/A"",
      ""cmuArpabet"": ""R IH0 K AO1 R D"",
      ""examples"": [""Please record this meeting.""]
    }
  ]
}";

        WireMockServer.Given(
            Request.Create()
                .WithPath("/v1/messages")
                .UsingPost()
        )
        .RespondWith(
            Response.Create()
                .WithStatusCode(200)
                .WithBody($@"{{""content"":[{{""type"":""text"",""text"":{System.Text.Json.JsonSerializer.Serialize(mockResponse)}}}]}}")
        );

        var request = new TranslateWordRequest("nagrywać", "Polish", "English", UseCache: false);

        TranslationResult result = await _handler.HandleAsync(request);

        result.Translations.Should().HaveCount(2);

        var nounTranslation = result.Translations.First(t => t.PartOfSpeech == "noun");
        nounTranslation.CmuArpabet.Should().Be("R EH1 K ER0 D");

        var verbTranslation = result.Translations.First(t => t.PartOfSpeech == "verb");
        verbTranslation.CmuArpabet.Should().Be("R IH0 K AO1 R D");
    }

    [Fact]
    public async Task TranslateWord_CacheHit_ReturnsFromCache()
    {
        string mockResponse = @"{
  ""translations"": [
    {
      ""rank"": 1,
      ""word"": ""dog"",
      ""partOfSpeech"": ""noun"",
      ""countability"": ""countable"",
      ""cmuArpabet"": ""D AO1 G"",
      ""examples"": [""The dog barked loudly.""]
    }
  ]
}";

        WireMockServer.Given(
            Request.Create()
                .WithPath("/v1/messages")
                .UsingPost()
        )
        .RespondWith(
            Response.Create()
                .WithStatusCode(200)
                .WithBody($@"{{""content"":[{{""type"":""text"",""text"":{System.Text.Json.JsonSerializer.Serialize(mockResponse)}}}]}}")
        );

        var request = new TranslateWordRequest("pies", "Polish", "English", UseCache: true);

        TranslationResult firstResult = await _handler.HandleAsync(request);
        firstResult.Should().NotBeNull();

        WireMockServer.ResetMappings();

        TranslationResult cachedResult = await _handler.HandleAsync(request);

        cachedResult.Should().NotBeNull();
        cachedResult.InputText.Should().Be(firstResult.InputText);
        cachedResult.Translations.Should().HaveCount(firstResult.Translations.Count);
    }

    [Fact]
    public async Task TranslateWord_NoCacheFlag_BypassesCache()
    {
        string mockResponse1 = @"{
  ""translations"": [
    {
      ""rank"": 1,
      ""word"": ""house"",
      ""partOfSpeech"": ""noun"",
      ""countability"": ""countable"",
      ""cmuArpabet"": ""HH AW1 S"",
      ""examples"": [""I live in a big house.""]
    }
  ]
}";

        string mockResponse2 = @"{
  ""translations"": [
    {
      ""rank"": 1,
      ""word"": ""home"",
      ""partOfSpeech"": ""noun"",
      ""countability"": ""countable"",
      ""cmuArpabet"": ""HH OW1 M"",
      ""examples"": [""Welcome to my home.""]
    }
  ]
}";

        WireMockServer.Given(
            Request.Create()
                .WithPath("/v1/messages")
                .UsingPost()
        )
        .InScenario("no-cache")
        .WillSetStateTo("second-call")
        .RespondWith(
            Response.Create()
                .WithStatusCode(200)
                .WithBody($@"{{""content"":[{{""type"":""text"",""text"":{System.Text.Json.JsonSerializer.Serialize(mockResponse1)}}}]}}")
        );

        WireMockServer.Given(
            Request.Create()
                .WithPath("/v1/messages")
                .UsingPost()
        )
        .InScenario("no-cache")
        .WhenStateIs("second-call")
        .RespondWith(
            Response.Create()
                .WithStatusCode(200)
                .WithBody($@"{{""content"":[{{""type"":""text"",""text"":{System.Text.Json.JsonSerializer.Serialize(mockResponse2)}}}]}}")
        );

        var request = new TranslateWordRequest("dom", "Polish", "English", UseCache: false);

        TranslationResult firstResult = await _handler.HandleAsync(request);
        firstResult.Translations[0].Word.Should().Be("house");

        TranslationResult secondResult = await _handler.HandleAsync(request);
        secondResult.Translations[0].Word.Should().Be("home");
    }

    [Fact]
    public async Task TranslateWord_EmptyWord_ThrowsValidationException()
    {
        var request = new TranslateWordRequest("", "Polish", "English");

        Func<Task> act = async () => await _handler.HandleAsync(request);

        await act.Should().ThrowAsync<FluentValidation.ValidationException>()
            .WithMessage("*Word cannot be empty*");
    }

    [Fact]
    public async Task TranslateWord_TooLongWord_ThrowsValidationException()
    {
        string longWord = new string('a', 101);
        var request = new TranslateWordRequest(longWord, "Polish", "English");

        Func<Task> act = async () => await _handler.HandleAsync(request);

        await act.Should().ThrowAsync<FluentValidation.ValidationException>()
            .WithMessage("*cannot exceed 100 characters*");
    }

    [Fact]
    public async Task TranslateWord_CmuArpabetSavedToCache_RetrievedOnSubsequentCall()
    {
        string mockResponse = @"{
  ""translations"": [
    {
      ""rank"": 1,
      ""word"": ""tree"",
      ""partOfSpeech"": ""noun"",
      ""countability"": ""countable"",
      ""cmuArpabet"": ""T R IY1"",
      ""examples"": [""The tree is tall.""]
    }
  ]
}";

        WireMockServer.Given(
            Request.Create()
                .WithPath("/v1/messages")
                .UsingPost()
        )
        .RespondWith(
            Response.Create()
                .WithStatusCode(200)
                .WithBody($@"{{""content"":[{{""type"":""text"",""text"":{System.Text.Json.JsonSerializer.Serialize(mockResponse)}}}]}}")
        );

        var request = new TranslateWordRequest("drzewo", "Polish", "English", UseCache: true);

        TranslationResult firstResult = await _handler.HandleAsync(request);
        firstResult.Translations[0].CmuArpabet.Should().Be("T R IY1");

        WireMockServer.ResetMappings();

        TranslationResult cachedResult = await _handler.HandleAsync(request);
        cachedResult.Translations[0].CmuArpabet.Should().Be("T R IY1");
    }

    [Fact]
    public async Task TranslateWord_NullCmuArpabet_SavedToCacheAndDisplaysNA()
    {
        string mockResponse = @"{
  ""translations"": [
    {
      ""rank"": 1,
      ""word"": ""xenophobia"",
      ""partOfSpeech"": ""noun"",
      ""countability"": ""uncountable"",
      ""cmuArpabet"": null,
      ""examples"": [""Xenophobia is a serious issue.""]
    }
  ]
}";

        WireMockServer.Given(
            Request.Create()
                .WithPath("/v1/messages")
                .UsingPost()
        )
        .RespondWith(
            Response.Create()
                .WithStatusCode(200)
                .WithBody($@"{{""content"":[{{""type"":""text"",""text"":{System.Text.Json.JsonSerializer.Serialize(mockResponse)}}}]}}")
        );

        var request = new TranslateWordRequest("ksenofobia", "Polish", "English", UseCache: true);

        TranslationResult firstResult = await _handler.HandleAsync(request);
        firstResult.Translations[0].CmuArpabet.Should().BeNull();

        WireMockServer.ResetMappings();

        TranslationResult cachedResult = await _handler.HandleAsync(request);
        cachedResult.Translations[0].CmuArpabet.Should().BeNull();
    }

    [Fact]
    public async Task TranslateWord_OfflineCache_RetrievesFromCacheWhenServerUnavailable()
    {
        string mockResponse = @"{
  ""translations"": [
    {
      ""rank"": 1,
      ""word"": ""book"",
      ""partOfSpeech"": ""noun"",
      ""countability"": ""countable"",
      ""cmuArpabet"": ""B UH1 K"",
      ""examples"": [""I read a good book.""]
    }
  ]
}";

        WireMockServer.Given(
            Request.Create()
                .WithPath("/v1/messages")
                .UsingPost()
        )
        .RespondWith(
            Response.Create()
                .WithStatusCode(200)
                .WithBody($@"{{""content"":[{{""type"":""text"",""text"":{System.Text.Json.JsonSerializer.Serialize(mockResponse)}}}]}}")
        );

        var request = new TranslateWordRequest("książka", "Polish", "English", UseCache: true);

        TranslationResult firstResult = await _handler.HandleAsync(request);
        firstResult.Should().NotBeNull();
        firstResult.Translations[0].CmuArpabet.Should().Be("B UH1 K");

        WireMockServer.Stop();

        TranslationResult offlineResult = await _handler.HandleAsync(request);

        offlineResult.Should().NotBeNull();
        offlineResult.Translations[0].Word.Should().Be("book");
        offlineResult.Translations[0].CmuArpabet.Should().Be("B UH1 K");
    }

    [Fact]
    public async Task TranslateWord_PolishDiacritics_RendersCorrectlyInUtf8()
    {
        string mockResponse = @"{
  ""translations"": [
    {
      ""rank"": 1,
      ""word"": ""yellow bile"",
      ""partOfSpeech"": ""noun"",
      ""countability"": ""uncountable"",
      ""cmuArpabet"": ""Y EH1 L OW0 B AY1 L"",
      ""examples"": [""Yellow bile was one of the four humors.""]
    }
  ]
}";

        WireMockServer.Given(
            Request.Create()
                .WithPath("/v1/messages")
                .UsingPost()
        )
        .RespondWith(
            Response.Create()
                .WithStatusCode(200)
                .WithBody($@"{{""content"":[{{""type"":""text"",""text"":{System.Text.Json.JsonSerializer.Serialize(mockResponse)}}}]}}")
        );

        var request = new TranslateWordRequest("żółć", "Polish", "English", UseCache: false);

        TranslationResult result = await _handler.HandleAsync(request);

        result.Should().NotBeNull();
        result.InputText.Should().Be("żółć");

        byte[] utf8Bytes = System.Text.Encoding.UTF8.GetBytes(result.InputText);
        string roundtrip = System.Text.Encoding.UTF8.GetString(utf8Bytes);
        roundtrip.Should().Be("żółć");
    }
}
