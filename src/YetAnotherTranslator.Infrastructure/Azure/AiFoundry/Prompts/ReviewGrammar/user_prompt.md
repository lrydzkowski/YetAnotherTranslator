Review the following English text for grammar and vocabulary:

{text}

Provide:

1. Grammar issues with corrections and explanations
2. Vocabulary suggestions for improvement
3. Modified text with corrections applied

Return response as structured JSON:

```json
{
  "grammarIssues": [{
      "issue": "Subject-verb disagreement",
      "correction": "The cat sits (not 'sit')",
      "explanation": "Singular subject requires singular verb"
    }
  ],
  "vocabularySuggestions": [{
      "original": "good",
      "suggestion": "excellent",
      "context": "More impactful in formal writing"
    }
  ],
  "modifiedText": "The cat sits on the mat. It is an excellent day."
}
```

Only return the JSON, no additional text.