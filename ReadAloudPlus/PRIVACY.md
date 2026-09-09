# Privacy Policy for Read Aloud Plus

*Last Updated: September 9, 2026*

Read Aloud Plus is committed to protecting your privacy. This privacy policy explains how our extension handles user data.

## 1. Information Collection and Use
- **No Personal Data Collected:** Read Aloud Plus does not collect, track, transmit, or sell any personally identifiable information (PII).
- **Text & Speech Processing:** All text extraction, speech synthesis, and voice playback occur locally on your machine using the Web Speech API and your browser's built-in text-to-speech engine. No text from web pages or user input is sent to external servers or third-party APIs.
- **Local Storage:** Read Aloud Plus stores your preferences (such as selected voices, speech rate, pitch, pause durations, custom pronunciation dictionary rules, and per-site settings) locally on your device using `chrome.storage.local`. This data never leaves your browser.

## 2. Permissions Justification
- `storage`: Required to save user settings and pronunciation dictionaries locally.
- `activeTab` & `scripting`: Required to extract text from the active webpage when you choose to read aloud, and to highlight words/sentences during playback.
- `contextMenus`: Required to provide the "Read Aloud from here" right-click option.
- `<all_urls>`: Required to allow reading content on any website you choose to navigate to.

## 3. Third-Party Services
Read Aloud Plus does not integrate with any analytics services, advertising networks, or third-party tracking tools.

## 4. Changes to This Policy
If we update this Privacy Policy, the revised version will be updated in the extension repository and store listings.

## 5. Contact
If you have any questions or feedback regarding this policy, please open an issue in the extension's code repository.
