using YetAnotherTranslator.Core.Common.Models;

namespace YetAnotherTranslator.Core.Handlers.ReviewGrammar.Models;

public record ReviewGrammarRequest(CommandType CommandType, string Text, bool UseCache = true);
