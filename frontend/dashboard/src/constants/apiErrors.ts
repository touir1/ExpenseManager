export const API_ERRORS = {
  SERVER: 'Server error, please retry later.',
  RATE_LIMIT: 'Too many requests. Please wait and try again.',
  NOT_FOUND: 'Resource not found.',
  FORBIDDEN: 'Access denied. You might not have permission.',
  BAD_REQUEST: 'Invalid request. Please check your input.',
  NETWORK: 'Network error. Please check your connection and try again.',
  UNAUTHORIZED: 'Unauthorized',
} as const

export const BACKEND_ERROR_CODES: Record<string, string> = {
  MISSING_PARAMETERS: 'Please fill in all required fields.',
  SERVER_ERROR: 'Server error, please retry later.',
  INVALID_USERNAME_OR_PASSWORD: 'Invalid email or password.',
  NO_ASSIGNED_ROLE: 'Your account has no assigned role. Please contact support.',
  EMAIL_VERIFICATION_FAILED: 'Email verification failed. Please check the link and try again.',
  NOT_MATCHING_CONFIRM_PASSWORD: 'Passwords do not match.',
  SET_NEW_PASSWORD_FAILED: 'Failed to set the new password. Please try again.',
  REQUEST_PASSWORD_RESET_FAILED: 'Failed to request a password reset. Please try again.',
  RESET_PASSWORD_FAILED: 'Failed to reset your password. Please try again.',
  MISSING_TOKEN: 'Authentication token is missing. Please log in again.',
  INVALID_TOKEN: 'Invalid authentication token. Please log in again.',
}
