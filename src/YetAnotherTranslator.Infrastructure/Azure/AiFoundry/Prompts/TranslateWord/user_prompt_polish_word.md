Translate the Polish word '{word}' to English.

Provide:

1. Multiple translations ranked by popularity (most common first)
2. For each translation:

    - The English word
    - Part of speech (noun, verb, adjective, etc.)
    - Countability (countable/uncountable for nouns, N/A otherwise)
    - CMU Arpabet phonetic transcription using standard CMU dictionary format
    - Example sentences demonstrating usage, at lest three per translation, show example sentences in English only

3. If a word has pronunciation variants by part of speech (e.g., "record" as noun vs verb), provide separate CMU Arpabet for each variant

Return response as structured JSON with schema:

```json
{
  "translations": [{
      "rank": 1,
      "word": "cat",
      "partOfSpeech": "noun",
      "countability": "countable",
      "cmuArpabet": "K AE1 T",
      "examples": ["The cat sat on the mat."]
    }
  ]
}
```

If CMU Arpabet cannot be generated for a word, use null for the cmuArpabet field.

Only return the JSON, no additional text.