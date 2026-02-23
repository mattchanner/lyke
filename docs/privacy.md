# Privacy & GDPR

LYKE is designed with privacy as a core principle. This document outlines how we protect user data, especially sensitive body profile information.

## Core Privacy Principles

### 1. Privacy by Design

All features are built with privacy considerations from the start, not added as an afterthought.

### 2. Data Minimization

We only collect data necessary for the service to function.

### 3. Purpose Limitation

Data is used only for its stated purpose and not repurposed without consent.

### 4. User Control

Users have full control over their data, including the right to delete.

---

## Body Profile Data Protection

Body profile data (height, weight, body type) is the most sensitive data in LYKE. We implement strict protections:

### What We Collect

| Data           | Purpose           | Shared with Retailers |
| -------------- | ----------------- | --------------------- |
| Height (cm)    | Feed matching     | Never directly        |
| Weight (kg)    | Feed matching     | Never directly        |
| Body Type      | Feed matching     | Aggregated only       |
| Fit Preference | Content filtering | Aggregated only       |

### Protection Measures

#### 1. No Direct Sharing

Body profile data is **never** shared directly with retailers. Retailers only see:

- Aggregated engagement by body type bands
- Anonymous fit feedback trends
- Range-based insights (e.g., "5'4" - 5'6"" not exact measurements)

#### 2. Minimum Thresholds

Data is only shown to retailers when sufficient sample sizes exist to prevent identification of individuals in small segments.

#### 3. Encryption

Body profile data is encrypted at rest in the database.

#### 4. Access Controls

- Only the user can view their exact measurements
- Admin access is logged and auditable
- API endpoints return only necessary data

---

## Retailer Data Access

### What Retailers CAN Access

| Data Type            | Format                   | Example                       |
| -------------------- | ------------------------ | ----------------------------- |
| Body type engagement | Percentage bands         | "Hourglass: 28.5% engagement" |
| Height engagement    | Range bands              | "5'4" - 5'6": 35% engagement" |
| Fit feedback         | Product-level aggregates | "45 users said True to Size"  |
| Click data           | Anonymous totals         | "425 clicks this month"       |
| Conversion data      | Anonymous totals         | "28 conversions"              |

### What Retailers CANNOT Access

| Data Type                    | Reason                     |
| ---------------------------- | -------------------------- |
| Individual user profiles     | Privacy protection         |
| Exact measurements           | Privacy protection         |
| User identities              | Privacy protection         |
| Individual browsing history  | Privacy protection         |
| Personal contact information | Not shared without consent |

### Example Retailer Insight Response

```json
{
  "engagementByHeightRange": [
    {
      "range": "5'4\" - 5'6\"",
      "engagementPercentage": 35.2
    }
  ]
}
```

Note: No individual heights are exposed.

---

## GDPR Compliance

### User Rights

LYKE supports all GDPR user rights:

#### 1. Right to Access (Article 15)

Users can view all data we hold about them through:

- `GET /api/profile/v1/me` - Profile data
- `GET /api/profile/v1/body` - Body profile
- Creator/Retailer profile endpoints as applicable

#### 2. Right to Rectification (Article 16)

Users can update their data through:

- `PUT /api/profile/v1/` - Update profile
- `PUT /api/profile/v1/body` - Update body profile

#### 3. Right to Erasure (Article 17)

Users can delete their account:

**Endpoint:** `DELETE /api/auth/v1/account`

**What Gets Deleted:**

- User account deactivated
- Body profile permanently deleted
- Creator profile deleted (if applicable)
- All engagement records deleted
- Saved posts associations deleted

**What Gets Anonymized:**

- Click events (UserId set to null, metrics preserved)
- Attribution data (for commission records)

**What Gets Retained:**

- Posts (with anonymized creator reference) - for content integrity
- Aggregated analytics - cannot be tied to individual

#### 4. Right to Data Portability (Article 20)

Users can export their data in a machine-readable format.

#### 5. Right to Withdraw Consent

GDPR consent flags tracked:

```json
{
  "gdprConsentedAt": "2024-01-01T00:00:00Z",
  "gdprConsentVersion": "1.0"
}
```

Users can withdraw consent at any time.

---

## Data Retention

| Data Type        | Retention Period        | After Deletion          |
| ---------------- | ----------------------- | ----------------------- |
| Active accounts  | Indefinite while active | See deletion policy     |
| Deleted accounts | 30 days recovery period | Permanently deleted     |
| Click events     | 2 years                 | Anonymized              |
| Audit logs       | 7 years                 | Required for compliance |
| Analytics        | Aggregated indefinitely | Not tied to individuals |

---

## Security Measures

### Technical Controls

1. **Encryption in Transit**: All API traffic over HTTPS (TLS 1.3)
2. **Encryption at Rest**: Sensitive data encrypted in database
3. **Access Logging**: All data access logged to audit trail
4. **Rate Limiting**: Prevents bulk data extraction
5. **Input Validation**: Prevents injection attacks

### Administrative Controls

1. **Role-Based Access**: Admins have minimal necessary access
2. **Audit Trail**: All admin actions logged with IP address
3. **Regular Audits**: Periodic review of access patterns
4. **Training**: Staff trained on data protection

### Organizational Controls

1. **Data Protection Officer**: Designated DPO for compliance
2. **Privacy Impact Assessments**: For new features
3. **Incident Response Plan**: For potential breaches
4. **Vendor Assessment**: Third-party security reviews

---

## Third-Party Data Sharing

### We Share With

| Party              | Data Shared         | Purpose          | Legal Basis         |
| ------------------ | ------------------- | ---------------- | ------------------- |
| Retailers          | Anonymized insights | Service delivery | Legitimate interest |
| Payment processors | Payout details      | Creator payments | Contract            |
| Cloud providers    | Encrypted data      | Infrastructure   | Contract (DPA)      |

### We Do NOT Share With

- Advertising networks
- Data brokers
- Any party without legal basis

---

## Cookie Policy

LYKE mobile app uses minimal local storage:

| Storage          | Purpose        | Duration          |
| ---------------- | -------------- | ----------------- |
| Auth tokens      | Authentication | Session / 30 days |
| User preferences | App settings   | Persistent        |
| Cache            | Performance    | 24 hours          |

No third-party tracking cookies are used.

---

## Children's Privacy

LYKE is not intended for users under 16. We do not knowingly collect data from children. If we discover such data, it will be deleted immediately.

---

## International Transfers

For users outside the EEA:

- Data may be processed in regions with adequate protections
- Standard Contractual Clauses in place with processors
- Privacy Shield-certified vendors where applicable

---

## Contact

For privacy inquiries:

- Email: privacy@lyke.app
- Data Protection Officer: dpo@lyke.app

For data subject requests:

- Use in-app account settings
- Email: privacy@lyke.app with verification

---

## Updates

This privacy documentation is versioned:

- Current version: 1.0
- Last updated: February 2026
- Users notified of material changes via email
