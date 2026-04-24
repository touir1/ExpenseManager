export const API_ERRORS = {
  SERVER: 'Server error, please retry later.',
  RATE_LIMIT: 'Too many requests. Please wait and try again.',
  NOT_FOUND: 'Resource not found.',
  FORBIDDEN: 'Access denied. You might not have permission.',
  BAD_REQUEST: 'Invalid request. Please check your input.',
  NETWORK: 'Network error. Please check your connection and try again.',
  UNAUTHORIZED: 'Unauthorized',
} as const
