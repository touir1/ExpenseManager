RUN QA AUDIT

Mode: ${mode:FULL}

Target URL: ${url:"https://localhost"}
Module: ${module:frontend-dashboard}

Mode: ${mode:FULL} (functional + UX/UI + security + destructive testing)
Destructive Testing: ${destructive:true}

Email Inbox URL: ${mail_url:"http://localhost:8025"}

Environment: TEST / STAGING


Available tools:
- Chrome browser automation (via integration)
- Email inbox inspection via ${mail_url}

---

# EMAIL TESTING BEHAVIOR

If ${mail_url} != none:

1. Open a new Chrome tab to ${mail_url}
2. Monitor inbox visually (like a real user)
2. Detect and validate all outgoing emails triggered during tests
3. Support inbox types:
   - Mailpit / Mailhog (local SMTP test inbox)
   - Gmail (real or test account)
   - Outlook / O365
   - Any web-based inbox UI

4. For each email found:
   - verify subject correctness
   - verify sender identity
   - validate body content (text + HTML)
   - check links (correct domain, valid tokens)
   - verify formatting consistency
   - validate timestamps and ordering

5. Test email-driven flows:
   - registration confirmation
   - password reset
   - MFA / OTP emails
   - notifications (updates, alerts)
   - transactional emails (orders, actions)

6. Edge cases:
   - duplicate email sending
   - missing emails
   - delayed delivery
   - spam protection triggers
   - multiple rapid triggers (rate limiting)

If ${mail_url} = none:
Skip all email-related testing.

---

## MODE BEHAVIOR

### FULL (default)
Run all test categories below.

### SECURITY_ONLY
Only security testing.

### UX_ONLY
Only UX/UI testing.

### SMOKE
Only basic app health checks.

---

## TEST CATEGORIES

### 1. Functional Testing
- authentication flows
- navigation
- CRUD operations
- form validation
- search/filter/sort
- file uploads
- error handling
- multi-tab consistency

---

### 2. UX/UI Testing
- layout consistency
- responsiveness (desktop/mobile)
- accessibility (keyboard, ARIA, contrast, focus)
- usability friction points

---

### 3. Security Testing (non-invasive)
- XSS attempts
- IDOR via URL/API manipulation
- auth/session validation
- token storage safety
- CORS and headers
- privilege escalation attempts

---

### 4. Destructive Testing (ONLY if enabled)

If Destructive Testing = true:

- bulk create and bulk delete operations
- repeated submissions / rapid actions
- refresh during write operations
- invalid extreme inputs (long strings, unicode, special chars)
- unsafe sequences (delete → edit → refresh)
- concurrency stress (multi-tab conflicts)

If Destructive Testing = false:
Skip this entire section.

---

### 5. Console + Network Inspection
- JavaScript errors
- failed API calls
- unexpected HTTP status codes

---

## OUTPUT

Generate Markdown report:

docs\issues\ongoing\qa_test_results\{{date}}-{{module}}-qa.md

Example:
docs\issues\ongoing\qa_test_results\2026-03-22-frontend-dashboard-qa.md

---

## REPORT FORMAT

Must include:
- Executive Summary
- Scope (mode + destructive flag)
- Functional Issues
- UX/UI Issues
- Security Findings
- Destructive Test Results (only if enabled)
- Performance Notes
- Reproduction Steps
- Severity (Critical / High / Medium / Low)
- Recommendations