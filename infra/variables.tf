# -----------------------------------------------------------------------------
# Environment
# -----------------------------------------------------------------------------

variable "environment" {
  description = "Deployment environment (dev or prod)"
  type        = string
  validation {
    condition     = contains(["dev", "prod"], var.environment)
    error_message = "environment must be 'dev' or 'prod'."
  }
}

variable "location" {
  description = "Azure region for all resources"
  type        = string
  default     = "uksouth"
}

# -----------------------------------------------------------------------------
# PostgreSQL
# -----------------------------------------------------------------------------

variable "postgresql_sku" {
  description = "PostgreSQL Flexible Server SKU"
  type        = string
}

variable "postgresql_storage_mb" {
  description = "PostgreSQL storage in MB"
  type        = number
}

variable "postgresql_admin_username" {
  description = "PostgreSQL administrator username"
  type        = string
  default     = "lykeadmin"
}

variable "postgresql_admin_password" {
  description = "PostgreSQL administrator password"
  type        = string
  sensitive   = true
}

# -----------------------------------------------------------------------------
# Storage
# -----------------------------------------------------------------------------

variable "storage_replication_type" {
  description = "Storage account replication type (LRS or GRS)"
  type        = string
}

# -----------------------------------------------------------------------------
# Container App
# -----------------------------------------------------------------------------

variable "container_cpu" {
  description = "Container App CPU cores"
  type        = number
}

variable "container_memory" {
  description = "Container App memory (e.g. 1Gi)"
  type        = string
}

variable "container_min_replicas" {
  description = "Minimum number of container replicas"
  type        = number
}

variable "container_max_replicas" {
  description = "Maximum number of container replicas"
  type        = number
}

# -----------------------------------------------------------------------------
# Log Analytics
# -----------------------------------------------------------------------------

variable "log_retention_days" {
  description = "Log Analytics workspace retention in days"
  type        = number
}

# -----------------------------------------------------------------------------
# Application Secrets
# -----------------------------------------------------------------------------

variable "jwt_secret" {
  description = "JWT signing key (min 32 chars)"
  type        = string
  sensitive   = true
}

variable "google_client_id" {
  description = "Google OAuth client ID"
  type        = string
  sensitive   = true
}

variable "apple_app_id" {
  description = "Apple Sign-In app ID"
  type        = string
  sensitive   = true
}

# -----------------------------------------------------------------------------
# Email
# -----------------------------------------------------------------------------

variable "email_sender_address" {
  description = "Email sender address"
  type        = string
  default     = "DoNotReply@be-lyke.clothing"
}

variable "email_app_base_url" {
  description = "Application base URL for email links"
  type        = string
}

variable "email_api_base_url" {
  description = "API base URL for email links"
  type        = string
}

variable "email_dry_run" {
  description = "Whether email sending is in dry-run mode"
  type        = bool
}

# -----------------------------------------------------------------------------
# ASPNETCORE
# -----------------------------------------------------------------------------

variable "aspnetcore_environment" {
  description = "ASP.NET Core environment name"
  type        = string
}
