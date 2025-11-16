Translate the English word '{word}' to Polish.

Provide:

1. Multiple translations ranked by popularity (most common first)
2. For each translation:

    - The Polish word
    - Part of speech (noun, verb, adjective, etc.)
    - Countability (countable/uncountable for nouns, N/A otherwise)
    - Example sentence demonstrating usage

Return response as structured JSON with schema:

```json
{
  "translations": [{
      "rank": 1,
      "word": "cat",
      "partOfSpeech": "noun",
      "countability": "countable",
      "examples": ["The cat sat on the mat."]
    }
  ]
}
```

Only return the JSON, no additional text.